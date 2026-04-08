using System.Text;
using System.Text.RegularExpressions;
using CSCraft.Detranspiler.Translators;

namespace CSCraft.Detranspiler;

/// <summary>
/// Orchestrates the full conversion of a Java Fabric mod project to a CSCraft C# project.
/// </summary>
public class DetranspilerEngine
{
    private readonly string _inputDir;
    private readonly string _outputDir;

    public DetranspilerEngine(string inputDir, string outputDir)
    {
        _inputDir  = inputDir;
        _outputDir = outputDir;
    }

    public void Run()
    {
        Directory.CreateDirectory(_outputDir);

        // Step 1 — Parse fabric.mod.json for metadata and copy assets
        Console.WriteLine("Step 1: Reading mod metadata and assets...");
        var assets = new AssetHandler(_inputDir, _outputDir);
        assets.ParseModJson();
        assets.CopyAssets();

        // Step 2 — Find all Java mod files to convert
        Console.WriteLine("\nStep 2: Finding Java source files...");
        var javaFiles = FindJavaFiles(_inputDir);
        Console.WriteLine($"  Found {javaFiles.Count} Java file(s)");

        // Step 3 — Convert each Java file to C#
        Console.WriteLine("\nStep 3: Converting Java files to C#...");
        foreach (var javaFile in javaFiles)
        {
            ConvertJavaFile(javaFile, assets.ModId);
        }

        // Step 4 — Generate .csproj
        Console.WriteLine("\nStep 4: Generating project file...");
        var generator = new ProjectGenerator(
            outputDir:  _outputDir,
            modId:      assets.ModId,
            modName:    assets.ModName,
            modVersion: assets.ModVersion);
        generator.Generate();

        // Step 5 — Print summary
        PrintSummary(assets);
    }

    private List<string> FindJavaFiles(string dir)
    {
        var results = new List<string>();
        // Only look in src/main/java — skip mixin files and auto-generated files
        string? javaRoot = FindJavaRoot(dir);
        if (javaRoot == null)
        {
            // Fall back to any .java files
            results.AddRange(Directory.GetFiles(dir, "*.java", SearchOption.AllDirectories)
                .Where(f => !f.Contains("mixin", StringComparison.OrdinalIgnoreCase)
                         && !f.Contains("Mixin", StringComparison.OrdinalIgnoreCase)));
            return results;
        }

        foreach (var f in Directory.GetFiles(javaRoot, "*.java", SearchOption.AllDirectories))
        {
            string name = Path.GetFileName(f);
            // Skip mixin files — they don't have CSCraft equivalents
            if (name.Contains("Mixin", StringComparison.OrdinalIgnoreCase)) continue;
            // Skip ModPlayerData helper — CSCraft SDK handles this
            if (name is "ModPlayerData.java") continue;
            results.Add(f);
        }
        return results;
    }

    private static string? FindJavaRoot(string dir)
    {
        // Look for src/main/java
        string candidate = Path.Combine(dir, "src", "main", "java");
        if (Directory.Exists(candidate)) return candidate;

        // Search recursively
        foreach (var d in Directory.GetDirectories(dir, "java", SearchOption.AllDirectories))
            if (d.Contains(Path.Combine("src", "main"))) return d;

        return null;
    }

    private void ConvertJavaFile(string javaFile, string modId)
    {
        string className = Path.GetFileNameWithoutExtension(javaFile);
        Console.WriteLine($"  Converting {className}.java...");

        string javaSource;
        try { javaSource = File.ReadAllText(javaFile); }
        catch (Exception ex)
        {
            Console.WriteLine($"    [error] Could not read file: {ex.Message}");
            return;
        }

        // Determine if this is the main mod class (implements ModInitializer)
        bool isMainClass = javaSource.Contains("implements ModInitializer");

        string csSource;
        if (isMainClass)
        {
            var translator = new ClassTranslator(javaSource, modId, className);
            csSource = translator.Translate();
        }
        else
        {
            // For helper/data classes — do a basic type-substitution pass
            csSource = ConvertHelperClass(javaSource, className);
        }

        string outputPath = Path.Combine(_outputDir, $"{className}.cs");
        File.WriteAllText(outputPath, csSource, Encoding.UTF8);
        Console.WriteLine($"    -> {className}.cs");
    }

    private static string ConvertHelperClass(string javaSource, string className)
    {
        var sb = new StringBuilder();
        sb.AppendLine("using CSCraft;");
        sb.AppendLine();
        sb.AppendLine($"// Converted from {className}.java");
        sb.AppendLine($"// NOTE: This file was not a standard Fabric mod class.");
        sb.AppendLine($"// Manual review required.");
        sb.AppendLine();

        // Do basic type substitutions line by line
        foreach (var line in javaSource.Split('\n'))
        {
            string t = line.Trim();
            if (t.StartsWith("package ") || t.StartsWith("import ")) continue;
            string mapped = ApplyTypeSubstitutions(line);
            sb.AppendLine(mapped);
        }
        return sb.ToString();
    }

    private static string ApplyTypeSubstitutions(string line)
    {
        line = Regex.Replace(line, @"\bServerPlayerEntity\b", "McPlayer");
        line = Regex.Replace(line, @"\bMinecraftServer\b",   "McServer");
        line = Regex.Replace(line, @"\bServerWorld\b",       "McWorld");
        line = Regex.Replace(line, @"\bBlockPos\b",          "McBlockPos");
        line = Regex.Replace(line, @"\bItemStack\b",         "McItemStack");
        line = Regex.Replace(line, @"\bNbtCompound\b",       "McNbt");
        line = Regex.Replace(line, @"\bString\b",            "string");
        line = Regex.Replace(line, @"\bboolean\b",           "bool");
        return line;
    }

    private static void PrintSummary(AssetHandler assets)
    {
        Console.WriteLine("\n=== Conversion complete ===");
        Console.WriteLine($"  Mod:     {assets.ModName} v{assets.ModVersion}");
        Console.WriteLine($"  Author:  {assets.ModAuthor}");
        Console.WriteLine($"  Mod ID:  {assets.ModId}");
        Console.WriteLine();
        Console.WriteLine("Next steps:");
        Console.WriteLine("  1. Review the generated .cs file(s) — look for '// TODO:' comments");
        Console.WriteLine("  2. Add [ModInfo] attribute to your main class");
        Console.WriteLine("  3. Run: dotnet build");
        Console.WriteLine("  4. Fix any remaining compile errors (usually minor type adjustments)");
    }
}
