namespace CSCraft;

/// <summary>
/// Fluent API for declaring crafting recipes.
/// All calls are resolved at build time — the build system generates the
/// recipe JSON files in data/modid/recipes/ automatically.
/// Use string literal IDs so the build system can read them statically.
/// </summary>
public static class Recipes
{
    // ── Shaped ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Declare a shaped crafting recipe.
    /// pattern: 2D array of strings, each cell a key char or " " for empty.
    /// ingredients: maps key strings to item IDs, e.g. { ["D"] = "mymod:my_gem" }
    /// result: tuple of (itemId, count).
    /// Example:
    ///   Recipes.AddShaped("mymod:my_sword",
    ///       result: ("mymod:my_sword", 1),
    ///       pattern: new[,] { {"D"," "}, {"D"," "}, {"S"," "} },
    ///       ingredients: new() { ["D"] = "mymod:my_gem", ["S"] = "minecraft:stick" });
    /// </summary>
    public static void AddShaped(string id, (string item, int count) result,
        string[,] pattern, Dictionary<string, string> ingredients) { }

    // ── Shapeless ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Declare a shapeless recipe (ingredients can be in any order).
    /// ingredients: array of item IDs (repeat an ID to require multiple).
    /// result: tuple of (itemId, count).
    /// </summary>
    public static void AddShapeless(string id, (string item, int count) result,
        string[] ingredients) { }

    // ── Cooking ───────────────────────────────────────────────────────────────

    /// <summary>Declare a furnace smelting recipe.</summary>
    public static void AddSmelting(string id, string ingredient, string result,
        float experience = 0.1f, int cookTime = 200) { }

    /// <summary>Declare a blast furnace recipe (ores/metals — twice as fast as furnace).</summary>
    public static void AddBlasting(string id, string ingredient, string result,
        float experience = 0.1f, int cookTime = 100) { }

    /// <summary>Declare a smoker recipe (food — twice as fast as furnace).</summary>
    public static void AddSmoking(string id, string ingredient, string result,
        float experience = 0.1f, int cookTime = 100) { }

    /// <summary>Declare a campfire cooking recipe.</summary>
    public static void AddCampfire(string id, string ingredient, string result,
        float experience = 0.1f, int cookTime = 600) { }

    /// <summary>Declare a stonecutter recipe (one ingredient, configurable output count).</summary>
    public static void AddStonecutting(string id, string ingredient, string result,
        int count = 1) { }

    // ── Smithing ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Declare a smithing table transform recipe (e.g. upgrade diamond → netherite).
    /// template: smithing template item ID.
    /// base: item to upgrade.
    /// addition: material added at the smithing table.
    /// result: the output item ID.
    /// </summary>
    public static void AddSmithingTransform(string id, string template, string baseItem,
        string addition, string result) { }
}
