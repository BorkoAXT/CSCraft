namespace CSCraft;

/// <summary>
/// Helpers for querying fluid states in the world.
/// Transpiles to Java's FluidState / Fluids checks.
/// </summary>
public static class McFluid
{
    // ── Fluid type constants ──────────────────────────────────────────────────

    public const string Water        = "minecraft:water";
    public const string FlowingWater = "minecraft:flowing_water";
    public const string Lava         = "minecraft:lava";
    public const string FlowingLava  = "minecraft:flowing_lava";

    // ── World fluid queries ───────────────────────────────────────────────────

    /// <summary>Check whether a block position contains any fluid.</summary>
    [JavaMethod("!{0}.getFluidState(new BlockPos({1},{2},{3})).isEmpty()")]
    public static bool IsFluid(McWorld world, int x, int y, int z) => false;

    /// <summary>Check whether a position is water (including flowing).</summary>
    [JavaMethod("{0}.getFluidState(new BlockPos({1},{2},{3})).isIn(net.minecraft.registry.tag.FluidTags.WATER)")]
    public static bool IsWater(McWorld world, int x, int y, int z) => false;

    /// <summary>Check whether a position is lava (including flowing).</summary>
    [JavaMethod("{0}.getFluidState(new BlockPos({1},{2},{3})).isIn(net.minecraft.registry.tag.FluidTags.LAVA)")]
    public static bool IsLava(McWorld world, int x, int y, int z) => false;

    /// <summary>Check whether a position has a source (non-flowing) fluid.</summary>
    [JavaMethod("{0}.getFluidState(new BlockPos({1},{2},{3})).isStill()")]
    public static bool IsSource(McWorld world, int x, int y, int z) => false;

    /// <summary>Get the fluid level at a position (0 = no fluid, 8 = source).</summary>
    [JavaMethod("{0}.getFluidState(new BlockPos({1},{2},{3})).getLevel()")]
    public static int GetLevel(McWorld world, int x, int y, int z) => 0;

    /// <summary>Check whether a player is submerged in fluid.</summary>
    [JavaMethod("{0}.isSubmergedInWater()")]
    public static bool IsPlayerSubmerged(McPlayer player) => false;

    /// <summary>Check whether a player is in lava.</summary>
    [JavaMethod("{0}.isInLava()")]
    public static bool IsPlayerInLava(McPlayer player) => false;
}
