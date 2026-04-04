using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text;

namespace Transpiler;

/// <summary>
/// Walks C# source files looking for McRegistry.Register* calls and generates
/// the corresponding Minecraft resource files:
///   - Item model JSON   (assets/{modId}/models/item/{name}.json)
///   - Block model JSON  (assets/{modId}/models/block/{name}.json)
///   - Blockstate JSON   (assets/{modId}/blockstates/{name}.json)
///   - Language entries   (assets/{modId}/lang/en_us.json)
///
/// Also generates block-item models for RegisterBlockItem calls.
///
/// Arguments must be compile-time string literals (same rule as RecipeGenerator).
/// </summary>
public static class ResourceGenerator
{
    public record ResourceOutput(
        Dictionary<string, string> Files,        // relative path → JSON content
        Dictionary<string, string> LangEntries,  // translation key → display name
        Dictionary<string, List<string>> BlockTags // tag path → list of block IDs
    );

    /// <summary>
    /// Parse all McRegistry.Register* calls in the given C# source text.
    /// Returns generated resource file paths and their contents.
    /// </summary>
    public static ResourceOutput Generate(string csSource, string modId, List<string> warnings)
    {
        var tree = CSharpSyntaxTree.ParseText(csSource);
        var root = tree.GetRoot();
        var files = new Dictionary<string, string>();
        var lang = new Dictionary<string, string>();
        var blockTags = new Dictionary<string, List<string>>();

        // Track block IDs registered so we know which RegisterBlockItem calls refer to blocks
        var registeredBlocks = new HashSet<string>();

        foreach (var inv in root.DescendantNodes().OfType<InvocationExpressionSyntax>())
        {
            if (inv.Expression is not MemberAccessExpressionSyntax mae) continue;
            string receiver = mae.Expression.ToString();
            if (receiver != "McRegistry") continue;

            string method = mae.Name.Identifier.Text;
            var args = inv.ArgumentList.Arguments;
            int line = inv.GetLocation().GetLineSpan().StartLinePosition.Line + 1;

            try
            {
                ProcessRegistryCall(method, args, modId, line, warnings, files, lang, registeredBlocks, blockTags);
            }
            catch (ResourceArgException ex)
            {
                warnings.Add($"Line {line}: McRegistry.{method} — {ex.Message} — resource generation skipped.");
            }
        }

        return new ResourceOutput(files, lang, blockTags);
    }

    /// <summary>
    /// Merge language entries into a single en_us.json string.
    /// </summary>
    public static string BuildLangJson(Dictionary<string, string> entries)
    {
        var sb = new StringBuilder();
        sb.AppendLine("{");
        int i = 0;
        foreach (var kvp in entries.OrderBy(e => e.Key))
        {
            string comma = i < entries.Count - 1 ? "," : "";
            sb.AppendLine($"  \"{EscapeJson(kvp.Key)}\": \"{EscapeJson(kvp.Value)}\"{comma}");
            i++;
        }
        sb.Append("}");
        return sb.ToString();
    }

    // ── Dispatch ──────────────────────────────────────────────────────────────

    private static void ProcessRegistryCall(
        string method,
        SeparatedSyntaxList<ArgumentSyntax> args,
        string modId,
        int line,
        List<string> warnings,
        Dictionary<string, string> files,
        Dictionary<string, string> lang,
        HashSet<string> registeredBlocks,
        Dictionary<string, List<string>> blockTags)
    {
        switch (method)
        {
            case "RegisterBlock":
                HandleBlock(args, modId, files, lang, registeredBlocks, blockTags);
                break;

            case "RegisterBlockItem":
                HandleBlockItem(args, modId, files, lang);
                break;

            case "RegisterItem":
                HandleItem(args, modId, files, lang);
                break;

            case "RegisterFood":
                HandleSimpleItem(args, modId, files, lang, "food");
                break;

            case "RegisterSword":
                HandleHandheldItem(args, modId, files, lang, "Sword");
                break;
            case "RegisterPickaxe":
                HandleHandheldItem(args, modId, files, lang, "Pickaxe");
                break;
            case "RegisterAxe":
                HandleHandheldItem(args, modId, files, lang, "Axe");
                break;
            case "RegisterShovel":
                HandleHandheldItem(args, modId, files, lang, "Shovel");
                break;
            case "RegisterHoe":
                HandleHandheldItem(args, modId, files, lang, "Hoe");
                break;

            case "RegisterHelmet":
                HandleSimpleItem(args, modId, files, lang, "Helmet");
                break;
            case "RegisterChestplate":
                HandleSimpleItem(args, modId, files, lang, "Chestplate");
                break;
            case "RegisterLeggings":
                HandleSimpleItem(args, modId, files, lang, "Leggings");
                break;
            case "RegisterBoots":
                HandleSimpleItem(args, modId, files, lang, "Boots");
                break;
        }
    }

    // ── Block ─────────────────────────────────────────────────────────────────

    private static void HandleBlock(
        SeparatedSyntaxList<ArgumentSyntax> args,
        string modId,
        Dictionary<string, string> files,
        Dictionary<string, string> lang,
        HashSet<string> registeredBlocks,
        Dictionary<string, List<string>> blockTags)
    {
        if (args.Count < 1) return;
        string? fullId = TryGetString(args[0].Expression);
        if (fullId == null) return;

        var (ns, name) = SplitId(fullId);
        registeredBlocks.Add(fullId);

        // Blockstate JSON
        string blockstatePath = $"assets/{ns}/blockstates/{name}.json";
        files[blockstatePath] =
            "{\n" +
            "  \"variants\": {\n" +
            $"    \"\": {{ \"model\": \"{ns}:block/{name}\" }}\n" +
            "  }\n" +
            "}";

        // Block model JSON (cube_all)
        string blockModelPath = $"assets/{ns}/models/block/{name}.json";
        files[blockModelPath] =
            "{\n" +
            $"  \"parent\": \"minecraft:block/cube_all\",\n" +
            $"  \"textures\": {{ \"all\": \"{ns}:block/{name}\" }}\n" +
            "}";

        // Lang entry
        lang[$"block.{ns}.{name}"] = PrettyName(name);

        // ── Mining tool & level tags ─────────────────────────────────────
        // Detect McMineTool and McMineLevel enum args
        string? toolName = null;
        string? levelName = null;

        foreach (var arg in args)
        {
            string argText = arg.Expression.ToString();
            if (argText.StartsWith("McMineTool."))
                toolName = argText.Substring("McMineTool.".Length);
            else if (argText.StartsWith("McMineLevel."))
                levelName = argText.Substring("McMineLevel.".Length);
        }

        if (toolName != null && toolName != "None")
        {
            // e.g. data/minecraft/tags/block/mineable/pickaxe.json
            string tagPath = $"data/minecraft/tags/block/mineable/{toolName.ToLowerInvariant()}.json";
            if (!blockTags.ContainsKey(tagPath))
                blockTags[tagPath] = new List<string>();
            blockTags[tagPath].Add(fullId);
        }

        if (levelName != null)
        {
            string? levelTag = null;
            switch (levelName)
            {
                case "Stone": levelTag = "needs_stone_tool"; break;
                case "Iron":  levelTag = "needs_iron_tool";  break;
                case "Diamond": levelTag = "needs_diamond_tool"; break;
            }
            if (levelTag != null)
            {
                string tagPath = $"data/minecraft/tags/block/{levelTag}.json";
                if (!blockTags.ContainsKey(tagPath))
                    blockTags[tagPath] = new List<string>();
                blockTags[tagPath].Add(fullId);
            }
        }
    }

    // ── Block Item ────────────────────────────────────────────────────────────

    private static void HandleBlockItem(
        SeparatedSyntaxList<ArgumentSyntax> args,
        string modId,
        Dictionary<string, string> files,
        Dictionary<string, string> lang)
    {
        if (args.Count < 1) return;
        string? fullId = TryGetString(args[0].Expression);
        if (fullId == null) return;

        var (ns, name) = SplitId(fullId);

        // Block-item model (inherits from the block model)
        string itemModelPath = $"assets/{ns}/models/item/{name}.json";
        files[itemModelPath] =
            "{\n" +
            $"  \"parent\": \"{ns}:block/{name}\"\n" +
            "}";
    }

    // ── Simple Item (generated texture layer) ─────────────────────────────────

    private static void HandleItem(
        SeparatedSyntaxList<ArgumentSyntax> args,
        string modId,
        Dictionary<string, string> files,
        Dictionary<string, string> lang)
    {
        if (args.Count < 1) return;
        string? fullId = TryGetString(args[0].Expression);
        if (fullId == null) return;

        var (ns, name) = SplitId(fullId);

        string itemModelPath = $"assets/{ns}/models/item/{name}.json";
        files[itemModelPath] =
            "{\n" +
            $"  \"parent\": \"minecraft:item/generated\",\n" +
            $"  \"textures\": {{ \"layer0\": \"{ns}:item/{name}\" }}\n" +
            "}";

        lang[$"item.{ns}.{name}"] = PrettyName(name);
    }

    // ── Simple item (armor, food, etc.) ───────────────────────────────────────

    private static void HandleSimpleItem(
        SeparatedSyntaxList<ArgumentSyntax> args,
        string modId,
        Dictionary<string, string> files,
        Dictionary<string, string> lang,
        string suffix)
    {
        if (args.Count < 1) return;
        string? fullId = TryGetString(args[0].Expression);
        if (fullId == null) return;

        var (ns, name) = SplitId(fullId);

        string itemModelPath = $"assets/{ns}/models/item/{name}.json";
        files[itemModelPath] =
            "{\n" +
            $"  \"parent\": \"minecraft:item/generated\",\n" +
            $"  \"textures\": {{ \"layer0\": \"{ns}:item/{name}\" }}\n" +
            "}";

        lang[$"item.{ns}.{name}"] = PrettyName(name);
    }

    // ── Handheld item (tools — uses handheld parent for held-in-hand display) ─

    private static void HandleHandheldItem(
        SeparatedSyntaxList<ArgumentSyntax> args,
        string modId,
        Dictionary<string, string> files,
        Dictionary<string, string> lang,
        string suffix)
    {
        if (args.Count < 1) return;
        string? fullId = TryGetString(args[0].Expression);
        if (fullId == null) return;

        var (ns, name) = SplitId(fullId);

        // Tools use "handheld" parent so they render diagonally in hand
        string itemModelPath = $"assets/{ns}/models/item/{name}.json";
        files[itemModelPath] =
            "{\n" +
            $"  \"parent\": \"minecraft:item/handheld\",\n" +
            $"  \"textures\": {{ \"layer0\": \"{ns}:item/{name}\" }}\n" +
            "}";

        lang[$"item.{ns}.{name}"] = PrettyName(name);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static (string ns, string name) SplitId(string fullId)
    {
        int colon = fullId.IndexOf(':');
        if (colon >= 0)
            return (fullId[..colon], fullId[(colon + 1)..]);
        return ("minecraft", fullId);
    }

    /// <summary>
    /// Convert "ruby_ore" → "Ruby Ore", "cooked_mystery" → "Cooked Mystery"
    /// </summary>
    private static string PrettyName(string name)
    {
        var parts = name.Split('_');
        var sb = new StringBuilder();
        foreach (var part in parts)
        {
            if (part.Length == 0) continue;
            if (sb.Length > 0) sb.Append(' ');
            sb.Append(char.ToUpper(part[0]));
            if (part.Length > 1) sb.Append(part[1..]);
        }
        return sb.ToString();
    }

    private static string? TryGetString(ExpressionSyntax expr)
    {
        if (expr is LiteralExpressionSyntax lit && lit.IsKind(SyntaxKind.StringLiteralExpression))
            return lit.Token.ValueText;
        return null;
    }

    /// <summary>
    /// Build a Minecraft tag JSON from a list of block/item IDs.
    /// </summary>
    public static string BuildTagJson(List<string> ids)
    {
        var sb = new StringBuilder();
        sb.AppendLine("{");
        sb.AppendLine("  \"replace\": false,");
        sb.AppendLine("  \"values\": [");
        for (int i = 0; i < ids.Count; i++)
        {
            string comma = i < ids.Count - 1 ? "," : "";
            sb.AppendLine($"    \"{EscapeJson(ids[i])}\"{comma}");
        }
        sb.AppendLine("  ]");
        sb.Append("}");
        return sb.ToString();
    }

    private static string EscapeJson(string s)
        => s.Replace("\\", "\\\\").Replace("\"", "\\\"");
}

internal sealed class ResourceArgException(string message) : Exception(message);
