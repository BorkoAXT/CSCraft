using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Transpiler;

namespace BuildTask;

/// <summary>
/// MSBuild task that transpiles .cscraft source files to Java
/// and places the output next to the Fabric template source tree.
///
/// Usage in a .csproj:
///   &lt;UsingTask TaskName="TranspileMod" AssemblyFile="path\to\BuildTask.dll" /&gt;
///   &lt;Target Name="Transpile" BeforeTargets="Build"&gt;
///     &lt;TranspileMod
///         SourceFiles="@(CSCraftSource)"
///         OutputDirectory="$(FabricSrcDir)"
///         PackageName="com.example.mymod" /&gt;
///   &lt;/Target&gt;
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

    // ── Outputs ───────────────────────────────────────────────────────────────

    /// <summary>The generated .java file paths (for incremental build support).</summary>
    [Output]
    public ITaskItem[] GeneratedFiles { get; set; } = [];

    // ── Execute ───────────────────────────────────────────────────────────────

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

            // Write output .java file — name matches the C# filename
            string baseName  = Path.GetFileNameWithoutExtension(csPath);
            string javaPath  = Path.Combine(OutputDirectory, baseName + ".java");

            File.WriteAllText(javaPath, result.JavaSource);
            Log.LogMessage(MessageImportance.Normal, $"CSCraft: {csPath} → {javaPath}");
            generated.Add(new TaskItem(javaPath));
        }

        GeneratedFiles = generated.ToArray();
        return success;
    }
}
