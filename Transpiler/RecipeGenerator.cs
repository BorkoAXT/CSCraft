using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text;
using System.Text.Json;

namespace Transpiler;

/// <summary>
/// Walks C# source files looking for McRecipe.Register* calls and generates
/// the corresponding Minecraft recipe JSON files (1.21.x format).
///
/// Supported calls:
///   McRecipe.RegisterShaped(id, pattern, keys, resultId, count)
///   McRecipe.RegisterShapeless(id, ingredients, resultId, count)
///   McRecipe.RegisterSmelting(id, inputId, resultId, experience, cookTimeSeconds)
///   McRecipe.RegisterBlasting(id, inputId, resultId, experience, cookTimeSeconds)
///   McRecipe.RegisterSmoking(id, inputId, resultId, experience, cookTimeSeconds)
///   McRecipe.RegisterCampfire(id, inputId, resultId, experience, cookTimeSeconds)
///   McRecipe.RegisterStonecutting(id, inputId, resultId, count)
///
/// Arguments must be compile-time constants (string/char/int/float literals or
/// simple array initializers containing literals).
/// Variable references that cannot be resolved at transpile time are skipped
/// with a warning.
/// </summary>
public static class RecipeGenerator
{
    /// <summary>
    /// Parse all McRecipe.Register* calls in the given C# source text.
    /// Returns a dictionary mapping filename → JSON content for every recipe found.
    /// Warnings about unresolvable calls are added to <paramref name="warnings"/>.
    /// </summary>
    public static Dictionary<string, string> Generate(
        string csSource,
        List<string> warnings)
    {
        var tree = CSharpSyntaxTree.ParseText(csSource);
        var root = tree.GetRoot();
        var results = new Dictionary<string, string>();

        foreach (var inv in root.DescendantNodes().OfType<InvocationExpressionSyntax>())
        {
            if (inv.Expression is not MemberAccessExpressionSyntax mae) continue;
            if (mae.Expression.ToString() != "McRecipe") continue;

            string method = mae.Name.Identifier.Text;
            var args = inv.ArgumentList.Arguments;
            int line = inv.GetLocation().GetLineSpan().StartLinePosition.Line + 1;

            try
            {
                string? json = method switch
                {
                    "RegisterShaped"      => BuildShaped(args, warnings, line),
                    "RegisterShapeless"   => BuildShapeless(args, warnings, line),
                    "RegisterSmelting"    => BuildCooking("minecraft:smelting", args, warnings, line),
                    "RegisterBlasting"    => BuildCooking("minecraft:blasting", args, warnings, line),
                    "RegisterSmoking"     => BuildCooking("minecraft:smoking", args, warnings, line),
                    "RegisterCampfire"    => BuildCooking("minecraft:campfire_cooking", args, warnings, line),
                    "RegisterStonecutting"=> BuildStonecutting(args, warnings, line),
                    _                    => null,
                };

                if (json == null) continue;

                // Extract recipe id from first argument
                string? recipeId = TryGetString(args[0].Expression);
                if (recipeId == null)
                {
                    warnings.Add($"Line {line}: McRecipe.{method} — recipe id must be a string literal.");
                    continue;
                }

                // Use the path portion of the id as the filename (e.g. "mymod:my_pick" → "my_pick")
                string filename = recipeId.Contains(':')
                    ? recipeId[(recipeId.IndexOf(':') + 1)..]
                    : recipeId;
                filename = filename.Replace('/', '_') + ".json";

                results[filename] = json;
            }
            catch (RecipeArgException ex)
            {
                warnings.Add($"Line {line}: McRecipe.{method} — {ex.Message} — recipe skipped.");
            }
        }

        return results;
    }

    // ── Shaped ────────────────────────────────────────────────────────────────

    // RegisterShaped(string id, string[] pattern, object[] keys, string resultId, int count = 1)
    private static string? BuildShaped(
        SeparatedSyntaxList<ArgumentSyntax> args,
        List<string> warnings,
        int line)
    {
        if (args.Count < 4)
            throw new RecipeArgException("expected at least 4 arguments: id, pattern[], keys[], resultId");

        string[] pattern  = RequireStringArray(args[1].Expression, "pattern");
        object[] keysRaw  = RequireObjectArray(args[2].Expression, "keys");
        string resultId   = RequireString(args[3].Expression, "resultId");
        int    count      = args.Count >= 5 ? RequireInt(args[4].Expression, "count") : 1;

        // Parse keys: alternating char, string → Dictionary<char, string>
        if (keysRaw.Length % 2 != 0)
            throw new RecipeArgException("keys array must have an even number of elements (char, itemId pairs)");

        var keyMap = new Dictionary<char, string>();
        for (int i = 0; i < keysRaw.Length; i += 2)
        {
            if (keysRaw[i] is not char k)
                throw new RecipeArgException($"keys[{i}] must be a char literal (e.g. 'X')");
            if (keysRaw[i + 1] is not string v)
                throw new RecipeArgException($"keys[{i + 1}] must be a string literal (item id)");
            keyMap[k] = v;
        }

        var sb = new StringBuilder();
        sb.AppendLine("{");
        sb.AppendLine("  \"type\": \"minecraft:crafting_shaped\",");

        // pattern
        sb.AppendLine("  \"pattern\": [");
        for (int i = 0; i < pattern.Length; i++)
            sb.AppendLine($"    \"{EscapeJson(pattern[i])}\"{(i < pattern.Length - 1 ? "," : "")}");
        sb.AppendLine("  ],");

        // key
        sb.AppendLine("  \"key\": {");
        int ki = 0;
        foreach (var (ch, itemId) in keyMap)
        {
            string comma = ki < keyMap.Count - 1 ? "," : "";
            sb.AppendLine($"    \"{ch}\": {{ \"item\": \"{EscapeJson(itemId)}\" }}{comma}");
            ki++;
        }
        sb.AppendLine("  },");

        // result (1.21.x format uses "id" not "item")
        sb.AppendLine("  \"result\": {");
        sb.AppendLine($"    \"id\": \"{EscapeJson(resultId)}\",");
        sb.AppendLine($"    \"count\": {count}");
        sb.AppendLine("  }");
        sb.Append("}");

        return sb.ToString();
    }

    // ── Shapeless ─────────────────────────────────────────────────────────────

    // RegisterShapeless(string id, string[] ingredients, string resultId, int count = 1)
    private static string? BuildShapeless(
        SeparatedSyntaxList<ArgumentSyntax> args,
        List<string> warnings,
        int line)
    {
        if (args.Count < 3)
            throw new RecipeArgException("expected at least 3 arguments: id, ingredients[], resultId");

        string[] ingredients = RequireStringArray(args[1].Expression, "ingredients");
        string resultId      = RequireString(args[2].Expression, "resultId");
        int    count         = args.Count >= 4 ? RequireInt(args[3].Expression, "count") : 1;

        var sb = new StringBuilder();
        sb.AppendLine("{");
        sb.AppendLine("  \"type\": \"minecraft:crafting_shapeless\",");

        sb.AppendLine("  \"ingredients\": [");
        for (int i = 0; i < ingredients.Length; i++)
            sb.AppendLine($"    {{ \"item\": \"{EscapeJson(ingredients[i])}\" }}{(i < ingredients.Length - 1 ? "," : "")}");
        sb.AppendLine("  ],");

        sb.AppendLine("  \"result\": {");
        sb.AppendLine($"    \"id\": \"{EscapeJson(resultId)}\",");
        sb.AppendLine($"    \"count\": {count}");
        sb.AppendLine("  }");
        sb.Append("}");

        return sb.ToString();
    }

    // ── Cooking (smelting / blasting / smoking / campfire) ────────────────────

    // RegisterSmelting(string id, string inputId, string resultId, float experience = 0.1f, int cookTimeSeconds = 10)
    private static string? BuildCooking(
        string type,
        SeparatedSyntaxList<ArgumentSyntax> args,
        List<string> warnings,
        int line)
    {
        if (args.Count < 3)
            throw new RecipeArgException("expected at least 3 arguments: id, inputId, resultId");

        string inputId    = RequireString(args[1].Expression, "inputId");
        string resultId   = RequireString(args[2].Expression, "resultId");
        float  experience = args.Count >= 4 ? RequireFloat(args[3].Expression, "experience") : 0.1f;
        int    seconds    = args.Count >= 5 ? RequireInt(args[4].Expression, "cookTimeSeconds") : DefaultCookTime(type);
        int    ticks      = seconds * 20;

        var sb = new StringBuilder();
        sb.AppendLine("{");
        sb.AppendLine($"  \"type\": \"{type}\",");
        sb.AppendLine($"  \"ingredient\": {{ \"item\": \"{EscapeJson(inputId)}\" }},");
        sb.AppendLine($"  \"result\": {{ \"id\": \"{EscapeJson(resultId)}\" }},");
        sb.AppendLine($"  \"experience\": {experience.ToString("0.0#", System.Globalization.CultureInfo.InvariantCulture)},");
        sb.AppendLine($"  \"cookingtime\": {ticks}");
        sb.Append("}");

        return sb.ToString();
    }

    private static int DefaultCookTime(string type) => type switch
    {
        "minecraft:smelting"        => 10,
        "minecraft:blasting"        => 5,
        "minecraft:smoking"         => 5,
        "minecraft:campfire_cooking"=> 30,
        _                           => 10,
    };

    // ── Stonecutting ──────────────────────────────────────────────────────────

    // RegisterStonecutting(string id, string inputId, string resultId, int count = 1)
    private static string? BuildStonecutting(
        SeparatedSyntaxList<ArgumentSyntax> args,
        List<string> warnings,
        int line)
    {
        if (args.Count < 3)
            throw new RecipeArgException("expected at least 3 arguments: id, inputId, resultId");

        string inputId  = RequireString(args[1].Expression, "inputId");
        string resultId = RequireString(args[2].Expression, "resultId");
        int    count    = args.Count >= 4 ? RequireInt(args[3].Expression, "count") : 1;

        var sb = new StringBuilder();
        sb.AppendLine("{");
        sb.AppendLine("  \"type\": \"minecraft:stonecutting\",");
        sb.AppendLine($"  \"ingredient\": {{ \"item\": \"{EscapeJson(inputId)}\" }},");
        sb.AppendLine($"  \"result\": \"{EscapeJson(resultId)}\",");
        sb.AppendLine($"  \"count\": {count}");
        sb.Append("}");

        return sb.ToString();
    }

    // ── Argument extraction helpers ───────────────────────────────────────────

    private static string RequireString(ExpressionSyntax expr, string argName)
        => TryGetString(expr)
           ?? throw new RecipeArgException($"'{argName}' must be a string literal; variable references are not supported at build time");

    private static int RequireInt(ExpressionSyntax expr, string argName)
        => TryGetInt(expr)
           ?? throw new RecipeArgException($"'{argName}' must be an integer literal");

    private static float RequireFloat(ExpressionSyntax expr, string argName)
        => TryGetFloat(expr)
           ?? throw new RecipeArgException($"'{argName}' must be a numeric literal");

    private static string[] RequireStringArray(ExpressionSyntax expr, string argName)
    {
        var items = TryGetStringArray(expr);
        if (items == null)
            throw new RecipeArgException($"'{argName}' must be a string array initializer with literal values (e.g. new[] {{\"XXX\", \"X X\"}})");
        return items;
    }

    private static object[] RequireObjectArray(ExpressionSyntax expr, string argName)
    {
        var items = TryGetObjectArray(expr);
        if (items == null)
            throw new RecipeArgException($"'{argName}' must be an object array initializer with char/string literal pairs (e.g. new object[] {{'X', \"minecraft:stick\"}})");
        return items;
    }

    private static string? TryGetString(ExpressionSyntax expr)
    {
        if (expr is LiteralExpressionSyntax lit && lit.IsKind(SyntaxKind.StringLiteralExpression))
            return lit.Token.ValueText;
        return null;
    }

    private static int? TryGetInt(ExpressionSyntax expr)
    {
        if (expr is LiteralExpressionSyntax lit &&
            lit.IsKind(SyntaxKind.NumericLiteralExpression) &&
            int.TryParse(lit.Token.ValueText, out int v))
            return v;
        // Handle prefix unary minus
        if (expr is PrefixUnaryExpressionSyntax pre &&
            pre.IsKind(SyntaxKind.UnaryMinusExpression) &&
            TryGetInt(pre.Operand) is int inner)
            return -inner;
        return null;
    }

    private static float? TryGetFloat(ExpressionSyntax expr)
    {
        if (expr is LiteralExpressionSyntax lit &&
            lit.IsKind(SyntaxKind.NumericLiteralExpression))
        {
            string text = lit.Token.ValueText.TrimEnd('f', 'F', 'd', 'D', 'm', 'M');
            if (float.TryParse(text, System.Globalization.NumberStyles.Float,
                               System.Globalization.CultureInfo.InvariantCulture, out float v))
                return v;
        }
        // integer also works as float
        if (TryGetInt(expr) is int i) return (float)i;
        return null;
    }

    private static string[]? TryGetStringArray(ExpressionSyntax expr)
    {
        // new[] { "...", "..." }  or  new string[] { "...", "..." }
        InitializerExpressionSyntax? init = expr switch
        {
            ImplicitArrayCreationExpressionSyntax ia  => ia.Initializer,
            ArrayCreationExpressionSyntax ac           => ac.Initializer,
            _                                          => null,
        };
        if (init == null) return null;

        var result = new List<string>();
        foreach (var item in init.Expressions)
        {
            string? s = TryGetString(item);
            if (s == null) return null;
            result.Add(s);
        }
        return result.ToArray();
    }

    private static object[]? TryGetObjectArray(ExpressionSyntax expr)
    {
        // new object[] { 'X', "minecraft:stick", 'Y', "minecraft:planks" }
        InitializerExpressionSyntax? init = expr switch
        {
            ImplicitArrayCreationExpressionSyntax ia  => ia.Initializer,
            ArrayCreationExpressionSyntax ac           => ac.Initializer,
            _                                          => null,
        };
        if (init == null) return null;

        var result = new List<object>();
        foreach (var item in init.Expressions)
        {
            if (item is LiteralExpressionSyntax lit)
            {
                if (lit.IsKind(SyntaxKind.CharacterLiteralExpression))
                    result.Add(lit.Token.ValueText[0]);
                else if (lit.IsKind(SyntaxKind.StringLiteralExpression))
                    result.Add(lit.Token.ValueText);
                else
                    return null;
            }
            else return null;
        }
        return result.ToArray();
    }

    private static string EscapeJson(string s)
        => s.Replace("\\", "\\\\").Replace("\"", "\\\"");
}

internal sealed class RecipeArgException(string message) : Exception(message);
