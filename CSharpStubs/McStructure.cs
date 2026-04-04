namespace CSCraft;

/// <summary>
/// Helpers for checking and generating structures in the world.
/// Structure files are data-driven NBT in data/modid/structure/.
/// Transpiles to StructureLocator / Structure calls.
/// </summary>
public static class McStructure
{
    /// <summary>
    /// Check whether a block position is inside a structure.
    /// structureId example: "minecraft:village"
    /// </summary>
    [JavaMethod("{0}.hasStructure(new BlockPos({1},{2},{3}), Registries.STRUCTURE.getOrThrow(net.minecraft.registry.RegistryKey.of(net.minecraft.registry.RegistryKeys.STRUCTURE, Identifier.of({4}))))")]
    public static bool IsInsideStructure(McWorld world, int x, int y, int z, string structureId) => false;

    /// <summary>
    /// Find the nearest structure of a type and return its block position,
    /// or null if none is found within search radius.
    /// </summary>
    [JavaMethod("{0}.locateStructure(Registries.STRUCTURE.getOrThrow(net.minecraft.registry.RegistryKey.of(net.minecraft.registry.RegistryKeys.STRUCTURE, Identifier.of({1}))), new BlockPos((int){0}.getLevelProperties().getSpawnX(), 64, (int){0}.getLevelProperties().getSpawnZ()), 100, false)")]
    public static McBlockPos? FindNearest(McWorld world, string structureId) => null;

    /// <summary>
    /// Place an NBT structure at a world position.
    /// structureId example: "mymod:my_house"
    /// </summary>
    [JavaMethod("{ var _structManager = {0}.getServer().getStructureTemplateManager(); var _template = _structManager.getTemplateOrBlank(Identifier.of({4})); var _placement = new net.minecraft.structure.StructurePlacementData(); _template.place((ServerWorld){0}, new BlockPos({1},{2},{3}), new BlockPos({1},{2},{3}), _placement, {0}.getRandom(), 2); }")]
    public static void Place(McWorld world, int x, int y, int z, string structureId) { }
}
