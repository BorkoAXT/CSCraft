namespace CSCraft;

/// <summary>
/// Helpers for declaring recipes at build time.
/// All Register* calls must use string literal IDs so the build system can generate
/// the Minecraft recipe JSON files automatically (in data/modid/recipe/).
///
/// At build time the transpiler reads these calls and writes the JSON;
/// at runtime the JSON files bundled in the mod JAR are loaded by Minecraft.
/// </summary>
public static class McRecipe
{
    // ── Shaped crafting ───────────────────────────────────────────────────────

    /// <summary>
    /// Declare a shaped crafting recipe. The build system generates the JSON automatically.
    /// id: recipe identifier, e.g. "mymod:my_sword"
    /// pattern: up to 3 strings of up to 3 chars (e.g. "XXX", "X X", "XXX")
    /// keys: alternating char/string pairs, e.g. new object[] { 'X', "minecraft:stick" }
    /// resultId: item id of the result, e.g. "mymod:my_sword"
    /// count: how many items to produce (default 1)
    /// </summary>
    public static void RegisterShaped(string id, string[] pattern, object[] keys, string resultId, int count = 1) { }

    /// <summary>
    /// Declare a shaped recipe using a flat variadic API.
    /// Usage: AddShaped("id", "result", count, "row1", "row2", "row3", 'K', "item:id", ...)
    /// Pattern rows (strings) come first, then alternating char+item pairs for key mappings.
    /// </summary>
    public static void AddShaped(string id, string resultId, int count, params object[] rest) { }

    // ── Shapeless crafting ────────────────────────────────────────────────────

    /// <summary>
    /// Declare a shapeless crafting recipe (ingredients in any order).
    /// ingredients: string array of item ids, e.g. new[] { "minecraft:stick", "minecraft:stone" }
    /// resultId: item id of the result
    /// </summary>
    public static void RegisterShapeless(string id, string[] ingredients, string resultId, int count = 1) { }

    /// <summary>
    /// Declare a shapeless recipe using a flat variadic API.
    /// Usage: AddShapeless("id", "result", count, "ingredient1", "ingredient2", ...)
    /// </summary>
    public static void AddShapeless(string id, string resultId, int count, params string[] ingredients) { }

    // ── Smelting / cooking ────────────────────────────────────────────────────

    /// <summary>Declare a furnace smelting recipe (cookTimeSeconds is multiplied by 20 to get ticks).</summary>
    public static void RegisterSmelting(string id, string inputId, string resultId, float experience = 0.1f, int cookTimeSeconds = 10) { }

    /// <summary>Declare a furnace smelting recipe (cookTimeTicks in game ticks; 200 = default 10s).</summary>
    public static void AddSmelting(string id, string inputId, string resultId, float experience = 0.1f, int cookTimeTicks = 200) { }

    /// <summary>Declare a blast furnace recipe (ores, metals).</summary>
    public static void RegisterBlasting(string id, string inputId, string resultId, float experience = 0.1f, int cookTimeSeconds = 5) { }

    /// <summary>Declare a blast furnace recipe (cookTimeTicks in ticks; 100 = default 5s).</summary>
    public static void AddBlasting(string id, string inputId, string resultId, float experience = 0.1f, int cookTimeTicks = 100) { }

    /// <summary>Declare a smoker recipe (food).</summary>
    public static void RegisterSmoking(string id, string inputId, string resultId, float experience = 0.1f, int cookTimeSeconds = 5) { }

    /// <summary>Declare a smoker recipe (cookTimeTicks in ticks; 100 = default 5s).</summary>
    public static void AddSmoking(string id, string inputId, string resultId, float experience = 0.1f, int cookTimeTicks = 100) { }

    /// <summary>Declare a campfire cooking recipe.</summary>
    public static void RegisterCampfire(string id, string inputId, string resultId, float experience = 0.1f, int cookTimeSeconds = 30) { }

    /// <summary>Declare a campfire cooking recipe (cookTimeTicks in ticks; 600 = default 30s).</summary>
    public static void AddCampfire(string id, string inputId, string resultId, float experience = 0.1f, int cookTimeTicks = 600) { }

    /// <summary>Declare a stonecutter recipe.</summary>
    public static void RegisterStonecutting(string id, string inputId, string resultId, int count = 1) { }

    /// <summary>Declare a stonecutter recipe (alias).</summary>
    public static void AddStonecutting(string id, string inputId, string resultId, int count = 1) { }

    // ── Recipe lookup ─────────────────────────────────────────────────────────

    /// <summary>Check if a player knows a specific recipe (for recipe unlocking).</summary>
    [JavaMethod("{0}.getRecipeBook().contains(Identifier.of({1}))")]
    public static bool PlayerKnowsRecipe(McPlayer player, string recipeId) => false;

    /// <summary>Unlock a recipe for a player.</summary>
    [JavaMethod("{0}.unlockRecipes(new net.minecraft.util.Identifier[] { Identifier.of({1}) })")]
    public static void UnlockForPlayer(McPlayer player, string recipeId) { }

    /// <summary>Lock (hide) a recipe from a player.</summary>
    [JavaMethod("{0}.lockRecipes(new net.minecraft.util.Identifier[] { Identifier.of({1}) })")]
    public static void LockForPlayer(McPlayer player, string recipeId) { }
}
