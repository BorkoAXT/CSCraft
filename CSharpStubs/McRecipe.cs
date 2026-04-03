namespace CSCraft;

/// <summary>
/// Helpers for registering crafting, smelting, and other recipes programmatically.
/// Transpiles to RecipeManager/RecipeBuilder calls in Java.
///
/// Note: Most recipes are data-driven (JSON in data/modid/recipes/).
/// These helpers register recipes in code for dynamic use.
/// </summary>
public static class McRecipe
{
    // ── Shaped crafting ───────────────────────────────────────────────────────

    /// <summary>
    /// Register a shaped crafting recipe.
    /// pattern: up to 3 strings of up to 3 chars each (e.g. "XXX", "X X", "XXX").
    /// keys: alternating char, McItem pairs (e.g. 'X', McRegistry.MyItem).
    /// result: the item to craft.
    /// count: how many of the result to give.
    /// </summary>
    public static void RegisterShaped(string id, string[] pattern, object[] keys, McItem result, int count = 1) { }

    // ── Shapeless crafting ────────────────────────────────────────────────────

    /// <summary>
    /// Register a shapeless crafting recipe (ingredients in any order).
    /// ingredients: the items needed.
    /// result: the item to craft.
    /// </summary>
    public static void RegisterShapeless(string id, McItem[] ingredients, McItem result, int count = 1) { }

    // ── Smelting / cooking ────────────────────────────────────────────────────

    /// <summary>Register a furnace smelting recipe.</summary>
    public static void RegisterSmelting(string id, McItem input, McItem result, float experience = 0.1f, int cookTimeSeconds = 10) { }

    /// <summary>Register a blast furnace recipe (ores, metals).</summary>
    public static void RegisterBlasting(string id, McItem input, McItem result, float experience = 0.1f, int cookTimeSeconds = 5) { }

    /// <summary>Register a smoker recipe (food).</summary>
    public static void RegisterSmoking(string id, McItem input, McItem result, float experience = 0.1f, int cookTimeSeconds = 5) { }

    /// <summary>Register a campfire cooking recipe.</summary>
    public static void RegisterCampfire(string id, McItem input, McItem result, float experience = 0.1f, int cookTimeSeconds = 30) { }

    /// <summary>Register a stonecutter recipe.</summary>
    public static void RegisterStonecutting(string id, McItem input, McItem result, int count = 1) { }

    // ── Recipe lookup ─────────────────────────────────────────────────────────

    /// <summary>Check if a player knows a specific recipe (for recipe unlocking).</summary>
    [JavaMethod("{0}.getRecipeBook().contains(new Identifier({1}))")]
    public static bool PlayerKnowsRecipe(McPlayer player, string recipeId) => false;

    /// <summary>Unlock a recipe for a player.</summary>
    [JavaMethod("{0}.unlockRecipes(new net.minecraft.util.Identifier[] { new Identifier({1}) })")]
    public static void UnlockForPlayer(McPlayer player, string recipeId) { }

    /// <summary>Lock (hide) a recipe from a player.</summary>
    [JavaMethod("{0}.lockRecipes(new net.minecraft.util.Identifier[] { new Identifier({1}) })")]
    public static void LockForPlayer(McPlayer player, string recipeId) { }
}
