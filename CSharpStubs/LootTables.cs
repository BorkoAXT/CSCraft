namespace CSCraft;

/// <summary>
/// Modify what entities drop and what is inside vanilla loot chests.
/// These calls are processed at build time to generate loot table JSON patches,
/// or at runtime via Fabric's loot table modification API.
/// </summary>
public static class LootTables
{
    // ── Mob drops ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Add an item to a mob's drop table with a configurable chance.
    /// mob: entity type ID, e.g. "minecraft:zombie"
    /// item: item ID, e.g. "mymod:my_gem"
    /// chance: 0.0 – 1.0 (1.0 = always drops)
    /// </summary>
    public static void AddMobDrop(string mob, string item, float chance, int minCount = 1, int maxCount = 1) { }

    /// <summary>
    /// Remove all drops of a specific item from a mob.
    /// </summary>
    public static void RemoveMobDrop(string mob, string item) { }

    // ── Chest loot ────────────────────────────────────────────────────────────

    /// <summary>
    /// Add an item to a vanilla chest loot table.
    /// chest: use LootChest.Dungeon, LootChest.Village, etc.
    /// chance: 0.0 – 1.0
    /// </summary>
    public static void AddChestLoot(LootChest chest, string item, float chance, int minCount = 1, int maxCount = 1) { }

    /// <summary>
    /// Add an item to any loot table by its full resource location.
    /// tableId: e.g. "minecraft:chests/simple_dungeon"
    /// </summary>
    public static void AddToTable(string tableId, string item, float chance, int minCount = 1, int maxCount = 1) { }

    /// <summary>
    /// Remove an item from any loot table.
    /// </summary>
    public static void RemoveFromTable(string tableId, string item) { }
}

/// <summary>
/// Vanilla chest loot table identifiers.
/// </summary>
public enum LootChest
{
    Dungeon,
    Mineshaft,
    Village,
    DesertTemple,
    JungleTemple,
    Stronghold,
    Mansion,
    Shipwreck,
    BuriedTreasure,
    NetherFortress,
    BasionRemnant,
    EndCity,
    PillagerOutpost,
    AncientCity,
    TrialChamber,
}
