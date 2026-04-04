using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Transpiler;

namespace BuildTask;

/// <summary>
/// MSBuild task that transpiles .cscraft source files to Java
/// and places the output next to the Fabric template source tree.
/// Also scans each source file for McRecipe.Register* calls and generates
/// the corresponding Minecraft recipe JSON files.
///
/// Usage in a .csproj:
///   &lt;UsingTask TaskName="TranspileMod" AssemblyFile="path\to\BuildTask.dll" /&gt;
///   &lt;Target Name="Transpile" BeforeTargets="Build"&gt;
///     &lt;TranspileMod
///         SourceFiles="@(CSCraftSource)"
///         OutputDirectory="$(FabricSrcDir)"
///         PackageName="com.example.mymod"
///         ResourcesDirectory="$(FabricResourcesDir)" /&gt;
///   &lt;/Target&gt;
///
/// ResourcesDirectory is optional. When provided, recipe JSONs are written to:
///   {ResourcesDirectory}/data/{modId}/recipe/{recipeName}.json
/// where modId is the last segment of PackageName (e.g. "mymod" from "com.example.mymod").
/// </summary>
public class TranspileMod : Microsoft.Build.Utilities.Task
{
    // ── Inputs ────────────────────────────────────────────────────────────────

    /// <summary>The .cs source files to transpile.</summary>
    [Required]
    public ITaskItem[] SourceFiles { get; set; } = [];

    /// <summary>Directory to write the generated .java files into.</summary>
    [Required]
    public string OutputDirectory { get; set; } = "";

    /// <summary>Java package name, e.g. "com.example.mymod".</summary>
    [Required]
    public string PackageName { get; set; } = "";

    /// <summary>
    /// Root resources directory of the Fabric project (the one containing assets/ and data/).
    /// When set, recipe JSON files are generated under {ResourcesDirectory}/data/{modId}/recipe/.
    /// Optional — omit to skip recipe JSON generation.
    /// </summary>
    public string ResourcesDirectory { get; set; } = "";

    // ── Outputs ───────────────────────────────────────────────────────────────

    /// <summary>The generated .java file paths (for incremental build support).</summary>
    [Output]
    public ITaskItem[] GeneratedFiles { get; set; } = [];

    // ── Execute ───────────────────────────────────────────────────────────────

    private readonly Dictionary<string, string> _allLangEntries = new();

    public override bool Execute()
    {
        Directory.CreateDirectory(OutputDirectory);

        var generated = new List<ITaskItem>();
        bool success  = true;

        foreach (var item in SourceFiles)
        {
            string csPath = item.GetMetadata("FullPath");

            if (!File.Exists(csPath))
            {
                Log.LogError($"CSCraft: source file not found: {csPath}");
                success = false;
                continue;
            }

            string csSource = File.ReadAllText(csPath);
            TranspileResult result;

            try
            {
                result = TranspilerRunner.Transpile(csSource, PackageName);
            }
            catch (Exception ex)
            {
                Log.LogError($"CSCraft: transpiler threw an exception for {csPath}: {ex.Message}");
                success = false;
                continue;
            }

            // Log any errors / warnings from the transpiler
            foreach (var e in result.Errors)
            {
                Log.LogError(
                    subcategory: "CSCraft",
                    errorCode: "CSCRAFT001",
                    helpKeyword: null,
                    file: csPath,
                    lineNumber: e.Line,
                    columnNumber: e.Column,
                    endLineNumber: 0,
                    endColumnNumber: 0,
                    message: e.Message
                );
                success = false;
            }

            foreach (var w in result.Warnings)
            {
                Log.LogWarning(
                    subcategory: "CSCraft",
                    warningCode: "CSCRAFT002",
                    helpKeyword: null,
                    file: csPath,
                    lineNumber: w.Line,
                    columnNumber: w.Column,
                    endLineNumber: 0,
                    endColumnNumber: 0,
                    message: w.Message
                );
            }

            if (result.Errors.Count > 0) continue;

            // Use the public class name from the Java output as the filename
            // Java requires the file name to match the public class name
            string baseName = ExtractClassName(result.JavaSource)
                              ?? Path.GetFileNameWithoutExtension(csPath);
            string javaPath = Path.Combine(OutputDirectory, baseName + ".java");

            File.WriteAllText(javaPath, result.JavaSource);
            Log.LogMessage(MessageImportance.Normal, $"CSCraft: {csPath} → {javaPath}");
            generated.Add(new TaskItem(javaPath));

            // ── Recipe JSON generation ────────────────────────────────────────
            if (!string.IsNullOrWhiteSpace(ResourcesDirectory))
            {
                // modId = last dot-segment of PackageName, e.g. "com.example.mymod" → "mymod"
                string modId = PackageName.Contains('.')
                    ? PackageName[(PackageName.LastIndexOf('.') + 1)..]
                    : PackageName;

                var recipeWarnings = new List<string>();
                var recipes = RecipeGenerator.Generate(csSource, recipeWarnings);

                foreach (var w in recipeWarnings)
                    Log.LogWarning(subcategory: "CSCraft", warningCode: "CSCRAFT003",
                        helpKeyword: null, file: csPath,
                        lineNumber: 0, columnNumber: 0, endLineNumber: 0, endColumnNumber: 0,
                        message: w);

                if (recipes.Count > 0)
                {
                    string recipeDir = Path.Combine(ResourcesDirectory, "data", modId, "recipe");
                    Directory.CreateDirectory(recipeDir);

                    foreach (var (filename, json) in recipes)
                    {
                        string recipePath = Path.Combine(recipeDir, filename);
                        File.WriteAllText(recipePath, json);
                        Log.LogMessage(MessageImportance.Normal,
                            $"CSCraft recipe: {filename} → {recipePath}");
                        generated.Add(new TaskItem(recipePath));
                    }
                }

                // ── Resource JSON generation (models, blockstates, lang) ─────
                var resourceWarnings = new List<string>();
                var resources = ResourceGenerator.Generate(csSource, modId, resourceWarnings);

                foreach (var w in resourceWarnings)
                    Log.LogWarning(subcategory: "CSCraft", warningCode: "CSCRAFT004",
                        helpKeyword: null, file: csPath,
                        lineNumber: 0, columnNumber: 0, endLineNumber: 0, endColumnNumber: 0,
                        message: w);

                // Write model/blockstate JSON files
                foreach (var (relPath, json) in resources.Files)
                {
                    string fullPath = Path.Combine(ResourcesDirectory, relPath.Replace('/', Path.DirectorySeparatorChar));
                    Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
                    File.WriteAllText(fullPath, json);
                    Log.LogMessage(MessageImportance.Normal,
                        $"CSCraft resource: {relPath} → {fullPath}");
                    generated.Add(new TaskItem(fullPath));
                }

                // Accumulate lang entries (merge across all source files)
                foreach (var (key, value) in resources.LangEntries)
                    _allLangEntries[key] = value;
            }
        }

        // ── Write merged en_us.json lang file ─────────────────────────────
        if (_allLangEntries.Count > 0 && !string.IsNullOrWhiteSpace(ResourcesDirectory))
        {
            string modId = PackageName.Contains('.')
                ? PackageName[(PackageName.LastIndexOf('.') + 1)..]
                : PackageName;

            string langDir = Path.Combine(ResourcesDirectory, "assets", modId, "lang");
            Directory.CreateDirectory(langDir);
            string langPath = Path.Combine(langDir, "en_us.json");

            // Merge with existing lang file if present
            if (File.Exists(langPath))
            {
                try
                {
                    var existing = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(
                        File.ReadAllText(langPath));
                    if (existing != null)
                    {
                        foreach (var (k, v) in existing)
                            _allLangEntries.TryAdd(k, v);
                    }
                }
                catch { /* ignore parse errors in existing file */ }
            }

            File.WriteAllText(langPath, ResourceGenerator.BuildLangJson(_allLangEntries));
            Log.LogMessage(MessageImportance.Normal,
                $"CSCraft lang: {_allLangEntries.Count} entries → {langPath}");
            generated.Add(new TaskItem(langPath));
        }

        GeneratedFiles = generated.ToArray();
        return success;
    }

    private static string? ExtractClassName(string javaSource)
    {
        var match = System.Text.RegularExpressions.Regex.Match(
            javaSource, @"public class (\w+)");
        return match.Success ? match.Groups[1].Value : null;
    }
}
