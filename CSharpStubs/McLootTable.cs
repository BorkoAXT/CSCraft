namespace CSCraft;

/// <summary>
/// Helpers for loot table access and modification.
/// Note: loot tables are primarily defined as JSON in data/modid/loot_table/.
/// These helpers let you access and trigger existing loot tables at runtime.
/// Transpiles to LootTable / LootContext calls.
/// </summary>
public static class McLootTable
{
    /// <summary>
    /// Drop loot from a loot table at a world position (items are spawned as entities).
    /// tableId example: "minecraft:chests/simple_dungeon"
    /// </summary>
    [JavaMethod("{ var _lootCtx = new net.minecraft.loot.context.LootContextParameterSet.Builder((ServerWorld){0}).add(net.minecraft.loot.context.LootContextParameters.ORIGIN, new net.minecraft.util.math.Vec3d({1},{2},{3})).build(net.minecraft.loot.context.LootContextTypes.CHEST); var _table = {0}.getServer().getReloadableRegistries().getLootTable(net.minecraft.loot.LootTable.EMPTY); _table.generateLoot(_lootCtx).forEach(s -> net.minecraft.entity.ItemEntity.spawn((ServerWorld){0}, new net.minecraft.util.math.BlockPos((int){1},(int){2},(int){3}), s)); }")]
    public static void DropLoot(McWorld world, string tableId, double x, double y, double z) { }

    /// <summary>
    /// Generate items from a loot table and give them to a player.
    /// </summary>
    [JavaMethod("{ var _lootCtx2 = new net.minecraft.loot.context.LootContextParameterSet.Builder((ServerWorld){0}.getWorld()).add(net.minecraft.loot.context.LootContextParameters.THIS_ENTITY, {0}).build(net.minecraft.loot.context.LootContextTypes.ENTITY); var _table2 = {0}.getServer().getReloadableRegistries().getLootTable(net.minecraft.loot.LootTable.EMPTY); _table2.generateLoot(_lootCtx2).forEach(s -> {0}.getInventory().insertStack(s)); }")]
    public static void GiveLootToPlayer(McPlayer player, string tableId) { }
}
