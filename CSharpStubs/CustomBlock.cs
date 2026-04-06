namespace CSCraft;

/// <summary>
/// Base class for custom blocks declared in C#.
/// Extend this class, add [BlockTexture("texture_name")], and CSCraft automatically:
///   - Registers the block and its BlockItem in the Fabric registry
///   - Generates the block model JSON (cube_all by default)
///   - Generates the blockstate JSON
///   - Copies textures from Assets/textures/block/ into the mod jar
///
/// Example — simple block:
///   [BlockTexture("magic_stone")]
///   public class MagicStone : CustomBlock
///   {
///       public override string Id         => "mymod:magic_stone";
///       public override string Name       => "Magic Stone";
///       public override float  Hardness   => 3.0f;
///       public override string SoundGroup => "stone";
///
///       public override void OnSteppedOn(McPlayer player)
///           => player.GiveEffect("minecraft:speed", 60, 1);
///   }
///
/// Example — directional block (different top/side/bottom textures):
///   [BlockTexture(side: "magic_stone_side", top: "magic_stone_top", bottom: "magic_stone_bottom")]
///   public class MagicFurnace : CustomBlock { ... }
/// </summary>
public abstract class CustomBlock
{
    /// <summary>Fully-qualified block ID, e.g. "mymod:magic_stone".</summary>
    public abstract string Id { get; }

    /// <summary>Display name shown in-game.</summary>
    public abstract string Name { get; }

    /// <summary>Hardness — time to mine (1.0 = stone, 3.0 = iron ore). Default 1.5.</summary>
    public virtual float Hardness => 1.5f;

    /// <summary>Blast resistance (default = Hardness * 3).</summary>
    public virtual float Resistance => Hardness * 3f;

    /// <summary>
    /// Sound group name used for step/break/place sounds.
    /// Values: "stone", "wood", "gravel", "grass", "sand", "metal",
    ///         "glass", "wool", "snow", "ladder", "anvil", "slime", "honey"
    /// </summary>
    public virtual string SoundGroup => "stone";

    /// <summary>Light emission level (0–15). Default 0.</summary>
    public virtual int LightLevel => 0;

    /// <summary>Whether this block requires the correct tool to drop (like ores).</summary>
    public virtual bool RequiresTool => false;

    /// <summary>
    /// Mining tool required: McMineTool.Pickaxe, .Axe, .Shovel, .Hoe, or .None.
    /// If set, block tags are auto-generated.
    /// </summary>
    public virtual McMineTool MineTool => McMineTool.None;

    /// <summary>Minimum tool tier required. Default Wood (any tool works).</summary>
    public virtual McMineLevel MineLevel => McMineLevel.Wood;

    // ── Callbacks ─────────────────────────────────────────────────────────────

    /// <summary>Called every tick when an entity stands on top of this block.</summary>
    public virtual void OnSteppedOn(McPlayer player) { }

    /// <summary>Called when a player right-clicks this block.</summary>
    public virtual void OnInteract(McPlayer player, McWorld world, McBlockPos pos) { }

    /// <summary>Called when the block is broken by a player.</summary>
    public virtual void OnBroken(McPlayer player, McWorld world, McBlockPos pos) { }

    /// <summary>Called when the block is placed by a player.</summary>
    public virtual void OnPlaced(McPlayer player, McWorld world, McBlockPos pos) { }

    /// <summary>Called on each random tick (requires block registered with TicksRandomly).</summary>
    public virtual void OnRandomTick(McWorld world, McBlockPos pos) { }

    /// <summary>
    /// Called when a neighbour block changes.
    /// Return false to break this block (it will drop as an item).
    /// </summary>
    public virtual bool OnNeighborUpdate(McWorld world, McBlockPos pos) => true;
}
