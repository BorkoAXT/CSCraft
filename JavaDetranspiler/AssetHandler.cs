using System.Text.Json;
using System.Text.Json.Nodes;

namespace CSCraft.Detranspiler;

/// <summary>
/// Handles non-Java assets: parses fabric.mod.json and copies
/// textures, lang, sounds, recipes, loot tables to the output project.
/// </summary>
public class AssetHandler
{
    private readonly string _inputDir;
    private readonly string _outputDir;

    // Parsed from fabric.mod.json
    public string ModId      { get; private set; } = "mymod";
    public string ModName    { get; private set; } = "My Mod";
    public string ModVersion { get; private set; } = "1.0.0";
    public string ModAuthor  { get; private set; } = "Author";
    public string ModDesc    { get; private set; } = "";

    public AssetHandler(string inputDir, string outputDir)
    {
        _inputDir  = inputDir;
        _outputDir = outputDir;
    }

    public void ParseModJson()
    {
        // Search for fabric.mod.json anywhere in the input
        string? modJson = FindFile(_inputDir, "fabric.mod.json");
        if (modJson == null)
        {
            Console.WriteLine("  [warn] fabric.mod.json not found — using defaults");
            return;
        }

        try
        {
            var node = JsonNode.Parse(File.ReadAllText(modJson));
            if (node == null) return;

            ModId      = node["id"]?.GetValue<string>()          ?? ModId;
            ModVersion = node["version"]?.GetValue<string>()      ?? ModVersion;
            ModName    = node["name"]?.GetValue<string>()          ?? ModName;
            ModDesc    = node["description"]?.GetValue<string>()   ?? ModDesc;

            // Authors can be a string or array
            var authors = node["authors"];
            if (authors is JsonArray arr && arr.Count > 0)
                ModAuthor = arr[0]?.GetValue<string>() ?? ModAuthor;
            else if (authors is JsonValue sv)
                ModAuthor = sv.GetValue<string>();

            Console.WriteLine($"  Mod: {ModName} v{ModVersion} by {ModAuthor} (id={ModId})");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  [warn] Failed to parse fabric.mod.json: {ex.Message}");
        }
    }

    public void CopyAssets()
    {
        // Find the resources root in the input
        string? resourcesRoot = FindDirectory(_inputDir, "resources");
        if (resourcesRoot == null)
        {
            Console.WriteLine("  [info] No resources folder found — skipping asset copy");
            return;
        }

        string outputResources = Path.Combine(_outputDir, "FabricTemplate", "src", "main", "resources");

        CopyDirectory(
            src:  Path.Combine(resourcesRoot, "assets", ModId, "textures"),
            dest: Path.Combine(outputResources, "assets", ModId, "textures"),
            ext:  ".png", label: "textures");

        CopyDirectory(
            src:  Path.Combine(resourcesRoot, "assets", ModId, "lang"),
            dest: Path.Combine(outputResources, "assets", ModId, "lang"),
            ext:  ".json", label: "lang files");

        CopySingleFile(
            src:  Path.Combine(resourcesRoot, "assets", ModId, "sounds.json"),
            dest: Path.Combine(outputResources, "assets", ModId, "sounds.json"),
            label: "sounds.json");

        CopyDirectory(
            src:  Path.Combine(resourcesRoot, "data", ModId, "recipes"),
            dest: Path.Combine(outputResources, "..", "data", ModId, "recipes"),
            ext:  ".json", label: "recipes");

        CopyDirectory(
            src:  Path.Combine(resourcesRoot, "data", ModId, "loot_table"),
            dest: Path.Combine(outputResources, "..", "data", ModId, "loot_table"),
            ext:  ".json", label: "loot tables");

        CopyDirectory(
            src:  Path.Combine(resourcesRoot, "data", ModId, "loot_tables"),
            dest: Path.Combine(outputResources, "..", "data", ModId, "loot_tables"),
            ext:  ".json", label: "loot tables (alt)");

        CopyDirectory(
            src:  Path.Combine(resourcesRoot, "data", ModId, "tags"),
            dest: Path.Combine(outputResources, "..", "data", ModId, "tags"),
            ext:  ".json", label: "tags");

        // Note: models/ and blockstates/ are intentionally SKIPPED
        // CSCraft.Sdk auto-generates these from registered content
    }

    private static void CopyDirectory(string src, string dest, string ext, string label)
    {
        if (!Directory.Exists(src)) return;
        var files = Directory.GetFiles(src, $"*{ext}", SearchOption.AllDirectories);
        if (files.Length == 0) return;
        int count = 0;
        foreach (var file in files)
        {
            string rel  = Path.GetRelativePath(src, file);
            string target = Path.Combine(dest, rel);
            Directory.CreateDirectory(Path.GetDirectoryName(target)!);
            File.Copy(file, target, overwrite: true);
            count++;
        }
        Console.WriteLine($"  Copied {count} {label}");
    }

    private static void CopySingleFile(string src, string dest, string label)
    {
        if (!File.Exists(src)) return;
        Directory.CreateDirectory(Path.GetDirectoryName(dest)!);
        File.Copy(src, dest, overwrite: true);
        Console.WriteLine($"  Copied {label}");
    }

    private static string? FindFile(string root, string name)
    {
        foreach (var f in Directory.GetFiles(root, name, SearchOption.AllDirectories))
            return f;
        return null;
    }

    private static string? FindDirectory(string root, string name)
    {
        foreach (var d in Directory.GetDirectories(root, name, SearchOption.AllDirectories))
            return d;
        return null;
    }
}
