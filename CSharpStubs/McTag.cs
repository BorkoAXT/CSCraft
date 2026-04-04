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
    [JavaMethod("{0}.getBlockState(new BlockPos({1},{2},{3})).isIn(net.minecraft.registry.tag.BlockTags.create(Identifier.of({4})))")]
    public static bool BlockIsIn(McWorld world, int x, int y, int z, string tagId) => false;

    /// <summary>Check whether a block type matches a tag.</summary>
    [JavaMethod("{0}.getDefaultState().isIn(net.minecraft.registry.tag.BlockTags.create(Identifier.of({1})))")]
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
    [JavaMethod("{0}.isIn(net.minecraft.registry.tag.ItemTags.create(Identifier.of({1})))")]
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
    [JavaMethod("{0}.getType().isIn(net.minecraft.registry.tag.EntityTypeTags.create(Identifier.of({1})))")]
    public static bool EntityIsIn(McEntity entity, string tagId) => false;

    /// <summary>Check whether an entity is undead.</summary>
    [JavaMethod("{0}.getType().isIn(net.minecraft.registry.tag.EntityTypeTags.UNDEAD)")]
    public static bool IsUndead(McEntity entity) => false;

    /// <summary>Check whether an entity can breathe underwater.</summary>
    [JavaMethod("{0}.getType().isIn(net.minecraft.registry.tag.EntityTypeTags.CAN_BREATHE_UNDER_WATER)")]
    public static bool CanBreatheUnderwater(McEntity entity) => false;

    /// <summary>Check whether an entity is a boss (wither, ender dragon, elder guardian).</summary>
    [JavaMethod("{0}.getType().isIn(net.minecraft.registry.tag.EntityTypeTags.BEEHIVE_INHABITORS) || {0} instanceof net.minecraft.entity.boss.WitherEntity || {0} instanceof net.minecraft.entity.boss.dragon.EnderDragonEntity")]
    public static bool IsBoss(McEntity entity) => false;

    // ── Common item tags ──────────────────────────────────────────────────────

    /// <summary>Check whether an item is any armor piece.</summary>
    [JavaMethod("{0}.isIn(net.minecraft.registry.tag.ItemTags.ARMOR_MATERIALS)")]
    public static bool IsArmor(McItemStack stack) => false;

    /// <summary>Check whether an item is a hoe.</summary>
    [JavaMethod("{0}.isIn(net.minecraft.registry.tag.ItemTags.HOES)")]
    public static bool IsHoe(McItemStack stack) => false;

    /// <summary>Check whether an item is a shovel.</summary>
    [JavaMethod("{0}.isIn(net.minecraft.registry.tag.ItemTags.SHOVELS)")]
    public static bool IsShovel(McItemStack stack) => false;

    /// <summary>Check whether an item is a bow or crossbow.</summary>
    [JavaMethod("{0}.isIn(net.minecraft.registry.tag.ItemTags.BOWS) || {0}.isIn(net.minecraft.registry.tag.ItemTags.CROSSBOWS)")]
    public static bool IsRangedWeapon(McItemStack stack) => false;

    /// <summary>Check whether an item can be worn in any armor slot.</summary>
    [JavaMethod("{0}.isIn(net.minecraft.registry.tag.ItemTags.HEAD_ARMOR) || {0}.isIn(net.minecraft.registry.tag.ItemTags.CHEST_ARMOR) || {0}.isIn(net.minecraft.registry.tag.ItemTags.LEG_ARMOR) || {0}.isIn(net.minecraft.registry.tag.ItemTags.FOOT_ARMOR)")]
    public static bool IsWearable(McItemStack stack) => false;
}
