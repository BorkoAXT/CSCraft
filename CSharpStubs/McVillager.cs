namespace CSCraft;

/// <summary>
/// Helpers for villager professions and trading.
/// Use McRegistry.AddVillagerBuyTrade / AddVillagerSellTrade for trades.
/// Transpiles to VillagerTrades / TradeOffer.
/// </summary>
public static class McVillager
{
    // ── Profession queries ────────────────────────────────────────────────────

    /// <summary>Get the profession ID of a villager entity.</summary>
    [JavaMethod("Registries.VILLAGER_PROFESSION.getId(((net.minecraft.entity.passive.VillagerEntity){0}).getVillagerData().getProfession()).toString()")]
    public static string GetProfession(McEntity villager) => null!;

    /// <summary>Get the trade level (1-5) of a villager.</summary>
    [JavaMethod("((net.minecraft.entity.passive.VillagerEntity){0}).getVillagerData().getLevel()")]
    public static int GetLevel(McEntity villager) => 1;

    /// <summary>Get the villager type (biome variant) ID.</summary>
    [JavaMethod("Registries.VILLAGER_TYPE.getId(((net.minecraft.entity.passive.VillagerEntity){0}).getVillagerData().getType()).toString()")]
    public static string GetType(McEntity villager) => null!;

    // ── Trade registration ────────────────────────────────────────────────────

    /// <summary>
    /// Add a trade where the player gives emeralds and receives an item.
    /// professionId example: "minecraft:farmer"
    /// tradeLevel: 1-5 (novice to master)
    /// </summary>
    public static void AddSellTrade(string professionId, int tradeLevel, McItem item, int count, int emeraldCost, int maxUses = 12) { }

    /// <summary>
    /// Add a trade where the player gives items and receives emeralds.
    /// </summary>
    public static void AddBuyTrade(string professionId, int tradeLevel, McItem item, int itemCount, int emeraldReward, int maxUses = 16) { }
}
