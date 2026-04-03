namespace CSCraft;

/// <summary>
/// Helpers for checking Minecraft tags on blocks, items, and entities.
/// Tags are defined in data/modid/tags/ as JSON.
/// Transpiles to Java tag checks via BlockTags / ItemTags / EntityTypeTags.
/// </summary>
public static class McTag
{
    // ── Block tags ────────────────────────────────────────────────────────────

    /// <summary>Check whether the block at a position matches a tag.</summary>
    [JavaMethod("{0}.getBlockState(new BlockPos({1},{2},{3})).isIn(net.minecraft.registry.tag.BlockTags.create(new Identifier({4})))")]
    public static bool BlockIsIn(McWorld world, int x, int y, int z, string tagId) => false;

    /// <summary>Check whether a block type matches a tag.</summary>
    [JavaMethod("{0}.getDefaultState().isIn(net.minecraft.registry.tag.BlockTags.create(new Identifier({1})))")]
    public static bool IsInTag(McBlock block, string tagId) => false;

    // ── Common block tags ─────────────────────────────────────────────────────

    /// <summary>Check whether a block is a log (minecraft:logs).</summary>
    [JavaMethod("{0}.getBlockState(new BlockPos({1},{2},{3})).isIn(net.minecraft.registry.tag.BlockTags.LOGS)")]
    public static bool IsLog(McWorld world, int x, int y, int z) => false;

    /// <summary>Check whether a block is leaves.</summary>
    [JavaMethod("{0}.getBlockState(new BlockPos({1},{2},{3})).isIn(net.minecraft.registry.tag.BlockTags.LEAVES)")]
    public static bool IsLeaves(McWorld world, int x, int y, int z) => false;

    /// <summary>Check whether a block is dirt-like.</summary>
    [JavaMethod("{0}.getBlockState(new BlockPos({1},{2},{3})).isIn(net.minecraft.registry.tag.BlockTags.DIRT)")]
    public static bool IsDirt(McWorld world, int x, int y, int z) => false;

    /// <summary>Check whether a block is stone-like.</summary>
    [JavaMethod("{0}.getBlockState(new BlockPos({1},{2},{3})).isIn(net.minecraft.registry.tag.BlockTags.STONE_ORE_REPLACEABLES)")]
    public static bool IsStone(McWorld world, int x, int y, int z) => false;

    // ── Item tags ─────────────────────────────────────────────────────────────

    /// <summary>Check whether an item matches a tag.</summary>
    [JavaMethod("{0}.isIn(net.minecraft.registry.tag.ItemTags.create(new Identifier({1})))")]
    public static bool ItemIsIn(McItemStack stack, string tagId) => false;

    /// <summary>Check whether an item is a sword.</summary>
    [JavaMethod("{0}.isIn(net.minecraft.registry.tag.ItemTags.SWORDS)")]
    public static bool IsSword(McItemStack stack) => false;

    /// <summary>Check whether an item is a pickaxe.</summary>
    [JavaMethod("{0}.isIn(net.minecraft.registry.tag.ItemTags.PICKAXES)")]
    public static bool IsPickaxe(McItemStack stack) => false;

    /// <summary>Check whether an item is an axe.</summary>
    [JavaMethod("{0}.isIn(net.minecraft.registry.tag.ItemTags.AXES)")]
    public static bool IsAxe(McItemStack stack) => false;

    /// <summary>Check whether an item is food.</summary>
    [JavaMethod("{0}.isIn(net.minecraft.registry.tag.ItemTags.FISHES)")]
    public static bool IsFish(McItemStack stack) => false;

    // ── Entity type tags ──────────────────────────────────────────────────────

    /// <summary>Check whether an entity type matches a tag.</summary>
    [JavaMethod("{0}.getType().isIn(net.minecraft.registry.tag.EntityTypeTags.create(new Identifier({1})))")]
    public static bool EntityIsIn(McEntity entity, string tagId) => false;

    /// <summary>Check whether an entity is undead.</summary>
    [JavaMethod("{0}.getType().isIn(net.minecraft.registry.tag.EntityTypeTags.UNDEAD)")]
    public static bool IsUndead(McEntity entity) => false;

    /// <summary>Check whether an entity is a monster.</summary>
    [JavaMethod("{0}.getType().isIn(net.minecraft.registry.tag.EntityTypeTags.CAN_BREATHE_UNDER_WATER)")]
    public static bool CanBreatheUnderwater(McEntity entity) => false;
}
