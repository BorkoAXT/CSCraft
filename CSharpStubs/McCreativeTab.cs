namespace CSCraft;

/// <summary>
/// Helpers for managing creative mode item groups (tabs).
/// Transpiles to Fabric's ItemGroupEvents.
/// </summary>
public static class McCreativeTab
{
    // ── Add to existing tab ───────────────────────────────────────────────────

    /// <summary>Add items to the Building Blocks tab.</summary>
    public static void AddToBuildingBlocks(params McItem[] items) { }

    /// <summary>Add items to the Natural Blocks tab.</summary>
    public static void AddToNaturalBlocks(params McItem[] items) { }

    /// <summary>Add items to the Functional Blocks tab.</summary>
    public static void AddToFunctional(params McItem[] items) { }

    /// <summary>Add items to the Redstone tab.</summary>
    public static void AddToRedstone(params McItem[] items) { }

    /// <summary>Add items to the Tools and Utilities tab.</summary>
    public static void AddToTools(params McItem[] items) { }

    /// <summary>Add items to the Combat tab.</summary>
    public static void AddToCombat(params McItem[] items) { }

    /// <summary>Add items to the Food and Drinks tab.</summary>
    public static void AddToFood(params McItem[] items) { }

    /// <summary>Add items to the Ingredients tab.</summary>
    public static void AddToIngredients(params McItem[] items) { }

    /// <summary>Add items to the Spawn Eggs tab.</summary>
    public static void AddToSpawnEggs(params McItem[] items) { }

    /// <summary>Add items to the Operator Utilities tab (requires OP).</summary>
    public static void AddToOperator(params McItem[] items) { }

    /// <summary>Add items to a tab identified by its Identifier (e.g. "minecraft:building_blocks").</summary>
    public static void AddToTab(string tabId, params McItem[] items) { }
}
