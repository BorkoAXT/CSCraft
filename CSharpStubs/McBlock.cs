namespace CSCraft;

/// <summary>
/// Facade for a Minecraft block type (Block / AbstractBlock).
/// Use McRegistry.RegisterBlock() to create and register custom blocks.
/// Transpiles to Java's Block.
/// </summary>
[JavaClass("net.minecraft.block.Block")]
public class McBlock
{
    // ── Info ──────────────────────────────────────────────────────────────────

    [JavaMethod("Registries.BLOCK.getId({target}).toString()")]
    public string Id { get; } = null!;

    [JavaMethod("Registries.BLOCK.getId({target}).getNamespace()")]
    public string Namespace { get; } = null!;

    [JavaMethod("Registries.BLOCK.getId({target}).getPath()")]
    public string Path { get; } = null!;

    // ── State ─────────────────────────────────────────────────────────────────

    [JavaMethod("{target}.getDefaultState()")]
    public McBlockState GetDefaultState() => null!;

    [JavaMethod("{target}.getDefaultState().isOf({0})")]
    public bool IsOf(McBlock other) => false;

    [JavaMethod("{target}.getDefaultState().isAir()")]
    public bool IsAir { get; }

    [JavaMethod("{target}.getDefaultState().isOpaque()")]
    public bool IsOpaque { get; }

    [JavaMethod("{target}.getDefaultState().isSolid()")]
    public bool IsSolid { get; }

    // ── Drops ─────────────────────────────────────────────────────────────────

    [JavaMethod("{target}.getPickStack(null, null, null)")]
    public McItemStack GetPickStack() => null!;
}

/// <summary>
/// Represents a BlockState (a block with specific property values).
/// </summary>
[JavaClass("net.minecraft.block.BlockState")]
public class McBlockState
{
    [JavaMethod("{target}.getBlock()")]
    public McBlock Block { get; } = null!;

    [JavaMethod("{target}.isAir()")]
    public bool IsAir { get; }

    [JavaMethod("{target}.isOpaque()")]
    public bool IsOpaque { get; }

    [JavaMethod("{target}.isSolid()")]
    public bool IsSolid { get; }

    [JavaMethod("{target}.isOf({0})")]
    public bool IsOf(McBlock block) => false;

    [JavaMethod("{target}.getHardness(null, null)")]
    public float Hardness { get; }

    [JavaMethod("{target}.getLuminance()")]
    public int Luminance { get; }
}

/// <summary>
/// Builder for block settings — used inside McRegistry.RegisterBlock().
/// </summary>
[JavaClass("net.minecraft.block.AbstractBlock.Settings")]
public sealed class McBlockSettings
{
    /// <summary>Create default settings.</summary>
    [JavaMethod("AbstractBlock.Settings.create()")]
    public static McBlockSettings Create() => null!;

    /// <summary>Copy settings from an existing block.</summary>
    [JavaMethod("AbstractBlock.Settings.copyOf({0})")]
    public static McBlockSettings CopyOf(McBlock block) => null!;

    [JavaMethod("{target}.strength({0})")]
    public McBlockSettings Strength(float hardnessAndResistance) => null!;

    [JavaMethod("{target}.strength({0}, {1})")]
    public McBlockSettings Strength(float hardness, float resistance) => null!;

    [JavaMethod("{target}.requiresCorrectToolForDrops()")]
    public McBlockSettings RequiresTool() => null!;

    [JavaMethod("{target}.noCollision()")]
    public McBlockSettings NoCollision() => null!;

    [JavaMethod("{target}.nonOpaque()")]
    public McBlockSettings NonOpaque() => null!;

    [JavaMethod("{target}.luminance(state -> {0})")]
    public McBlockSettings Luminance(int level) => null!;

    [JavaMethod("{target}.ticksRandomly()")]
    public McBlockSettings TicksRandomly() => null!;

    [JavaMethod("{target}.dropsNothing()")]
    public McBlockSettings DropsNothing() => null!;

    [JavaMethod("{target}.noBlockBreakParticles()")]
    public McBlockSettings NoParticles() => null!;

    [JavaMethod("{target}.replaceable()")]
    public McBlockSettings Replaceable() => null!;

    [JavaMethod("{target}.sounds(BlockSoundGroup.{0})")]
    public McBlockSettings Sounds(string soundGroup) => null!;

    [JavaMethod("{target}.mapColor(MapColor.{0})")]
    public McBlockSettings MapColor(string color) => null!;
}

/// <summary>
/// The tool type required to mine a block efficiently.
/// Used with McRegistry.RegisterBlock to auto-generate Fabric block tags.
/// </summary>
public enum McMineTool
{
    None,
    Pickaxe,
    Axe,
    Shovel,
    Hoe
}

/// <summary>
/// The minimum tool tier required to drop items from the block.
/// Maps to Fabric's needs_*_tool block tags.
/// </summary>
public enum McMineLevel
{
    /// <summary>Any tool works (wood or above).</summary>
    Wood = 0,
    /// <summary>Needs at least a stone tool.</summary>
    Stone = 1,
    /// <summary>Needs at least an iron tool.</summary>
    Iron = 2,
    /// <summary>Needs at least a diamond tool.</summary>
    Diamond = 3
}
