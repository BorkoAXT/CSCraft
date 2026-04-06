using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System.Reflection;
using Transpiler;

namespace BuildTask;

/// <summary>
/// MSBuild task that transpiles .cscraft source files to Java
/// and places the output next to the Fabric template source tree.
/// Also scans each source file for McRecipe.Register* calls and generates
/// the corresponding Minecraft recipe JSON files.
/// </summary>
public class TranspileMod : Microsoft.Build.Utilities.Task
{
    // ── Assembly resolver ─────────────────────────────────────────────────────
    // MSBuild doesn't automatically probe the task DLL's directory for
    // dependencies (Microsoft.CodeAnalysis, Transpiler, etc.).
    // Hook AssemblyResolve in the static constructor — BEFORE any method
    // referencing Transpiler types is JIT-compiled.

    static TranspileMod()
    {
        AppDomain.CurrentDomain.AssemblyResolve += ResolveAssembly;
    }

    private static string GetTaskDirectory()
    {
        try
        {
            string loc = typeof(TranspileMod).Assembly.Location;
            if (!string.IsNullOrEmpty(loc))
                return Path.GetDirectoryName(loc) ?? "";
        }
        catch { /* ignore */ }

        try
        {
            // Fallback for .NET Framework: CodeBase is a file:// URI
#pragma warning disable SYSLIB0012
            string codeBase = typeof(TranspileMod).Assembly.CodeBase;
#pragma warning restore SYSLIB0012
            if (!string.IsNullOrEmpty(codeBase))
            {
                var uri = new Uri(codeBase);
                return Path.GetDirectoryName(uri.LocalPath) ?? "";
            }
        }
        catch { /* ignore */ }

        return "";
    }

    private static Assembly? ResolveAssembly(object? sender, ResolveEventArgs args)
    {
        string name = new AssemblyName(args.Name).Name;

        // Skip system libraries that VS/MSBuild already provides
        if (name.StartsWith("System.") || name.StartsWith("Microsoft.Build") || name == "mscorlib")
            return null;

        string taskDir = GetTaskDirectory();
        if (string.IsNullOrEmpty(taskDir)) return null;

        string candidate = Path.Combine(taskDir, name + ".dll");
        if (File.Exists(candidate))
        {
            try
            {
                // Check if already loaded to avoid double-load errors
                var loaded = AppDomain.CurrentDomain.GetAssemblies()
                            .FirstOrDefault(a => a.GetName().Name == name);
                if (loaded != null) return loaded;

                return Assembly.LoadFrom(candidate);
            }
            catch { /* ignore */ }
        }
        return null;
    }

    // ── Inputs ────────────────────────────────────────────────────────────────

    [Required]
    public ITaskItem[] SourceFiles { get; set; } = [];

    [Required]
    public string OutputDirectory { get; set; } = "";

    [Required]
    public string PackageName { get; set; } = "";

    public string ResourcesDirectory { get; set; } = "";

    // ── Outputs ───────────────────────────────────────────────────────────────

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

            // Write extra helper classes if any
            if (result.ExtraJavaFiles != null)
            {
                foreach (var kvp in result.ExtraJavaFiles)
                {
                    string extraPath = Path.Combine(OutputDirectory, kvp.Key);
                    File.WriteAllText(extraPath, kvp.Value);
                    Log.LogMessage(MessageImportance.Normal, $"CSCraft: Generated helper {kvp.Key}");
                    generated.Add(new TaskItem(extraPath));
                }
            }

            // ── Recipe JSON generation ────────────────────────────────────────
            if (!string.IsNullOrWhiteSpace(ResourcesDirectory))
            {
                string modId = PackageName.Contains('.')
                    ? PackageName.Substring(PackageName.LastIndexOf('.') + 1)
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

                    foreach (var kvp in recipes)
                    {
                        string recipePath = Path.Combine(recipeDir, kvp.Key);
                        File.WriteAllText(recipePath, kvp.Value);
                        Log.LogMessage(MessageImportance.Normal,
                            $"CSCraft recipe: {kvp.Key} → {recipePath}");
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

                // Write block tag JSON files (mineable/pickaxe, needs_iron_tool, etc.)
                foreach (var kvp in resources.BlockTags)
                {
                    string tagPath = Path.Combine(ResourcesDirectory, kvp.Key.Replace('/', Path.DirectorySeparatorChar));
                    string tagDir = Path.GetDirectoryName(tagPath);
                    if (tagDir != null) Directory.CreateDirectory(tagDir);

                    var ids = new List<string>(kvp.Value);
                    if (File.Exists(tagPath))
                    {
                        try
                        {
                            var existing = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(
                                File.ReadAllText(tagPath));
                            if (existing != null && existing.ContainsKey("values"))
                            {
                                var arr = existing["values"] as System.Text.Json.JsonElement?;
                                if (arr.HasValue && arr.Value.ValueKind == System.Text.Json.JsonValueKind.Array)
                                {
                                    foreach (var elem in arr.Value.EnumerateArray())
                                    {
                                        string? val = elem.GetString();
                                        if (val != null && !ids.Contains(val))
                                            ids.Add(val);
                                    }
                                }
                            }
                        }
                        catch { /* ignore parse errors */ }
                    }

                    File.WriteAllText(tagPath, ResourceGenerator.BuildTagJson(ids));
                    Log.LogMessage(MessageImportance.Normal,
                        $"CSCraft tag: {kvp.Key} → {tagPath}");
                    generated.Add(new TaskItem(tagPath));
                }

                // Write model/blockstate JSON files
                foreach (var kvp in resources.Files)
                {
                    string fullPath = Path.Combine(ResourcesDirectory, kvp.Key.Replace('/', Path.DirectorySeparatorChar));
                    string dir = Path.GetDirectoryName(fullPath);
                    if (dir != null) Directory.CreateDirectory(dir);
                    File.WriteAllText(fullPath, kvp.Value);
                    Log.LogMessage(MessageImportance.Normal,
                        $"CSCraft resource: {kvp.Key} → {fullPath}");
                    generated.Add(new TaskItem(fullPath));
                }

                // Accumulate lang entries (merge across all source files)
                foreach (var kvp in resources.LangEntries)
                    _allLangEntries[kvp.Key] = kvp.Value;
            }
        }

        // ── Write merged en_us.json lang file ─────────────────────────────
        if (_allLangEntries.Count > 0 && !string.IsNullOrWhiteSpace(ResourcesDirectory))
        {
            string modId = PackageName.Contains('.')
                ? PackageName.Substring(PackageName.LastIndexOf('.') + 1)
                : PackageName;

            string langDir = Path.Combine(ResourcesDirectory, "assets", modId, "lang");
            Directory.CreateDirectory(langDir);
            string langPath = Path.Combine(langDir, "en_us.json");

            if (File.Exists(langPath))
            {
                try
                {
                    var existing = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(
                        File.ReadAllText(langPath));
                    if (existing != null)
                    {
                        foreach (var kvp in existing)
                            if (!_allLangEntries.ContainsKey(kvp.Key))
                                _allLangEntries[kvp.Key] = kvp.Value;
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
