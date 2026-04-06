namespace CSCraft;

/// <summary>
/// Base class for custom items declared in C#.
/// Extend this class, add [ItemTexture("texture_name")], and CSCraft automatically:
///   - Registers the item in the Fabric item registry
///   - Generates the item model JSON pointing to your texture
///   - Copies the texture from Assets/textures/item/ into the mod jar
///
/// Example:
///   [ItemTexture("my_gem")]
///   public class MyGem : CustomItem
///   {
///       public override string Id   => "mymod:my_gem";
///       public override string Name => "Magic Gem";
///       public override ItemRarity Rarity => ItemRarity.Rare;
///
///       public override void OnUse(McPlayer player, McWorld world)
///       {
///           world.CreateExplosion(player.X, player.Y, player.Z, 2.0f);
///       }
///   }
/// </summary>
public abstract class CustomItem
{
    /// <summary>Fully-qualified item ID, e.g. "mymod:my_gem".</summary>
    public abstract string Id { get; }

    /// <summary>Display name shown in-game.</summary>
    public abstract string Name { get; }

    /// <summary>Maximum stack size (1–64). Default 64.</summary>
    public virtual int MaxStack => 64;

    /// <summary>Maximum durability. 0 = indestructible (not a tool).</summary>
    public virtual int MaxDurability => 0;

    /// <summary>Item rarity that determines name colour.</summary>
    public virtual ItemRarity Rarity => ItemRarity.Common;

    /// <summary>Whether this item is fireproof (like netherite).</summary>
    public virtual bool Fireproof => false;

    // ── Callbacks ─────────────────────────────────────────────────────────────

    /// <summary>Called when the player right-clicks with this item in hand.</summary>
    public virtual void OnUse(McPlayer player, McWorld world) { }

    /// <summary>Called when the player uses this item on a block.</summary>
    public virtual void OnUseOnBlock(McPlayer player, McBlockPos pos, McWorld world) { }

    /// <summary>Called when this item finishes being used (e.g. bow fully drawn).</summary>
    public virtual void OnFinishUsing(McPlayer player, McWorld world) { }

    /// <summary>Called when the item is crafted by a player.</summary>
    public virtual void OnCrafted(McPlayer player) { }

    /// <summary>Called each tick while the player holds this item.</summary>
    public virtual void OnInventoryTick(McPlayer player, int slot) { }
}

/// <summary>Item rarity — controls the colour of the item name tooltip.</summary>
public enum ItemRarity
{
    /// <summary>White name (default).</summary>
    Common,
    /// <summary>Yellow name.</summary>
    Uncommon,
    /// <summary>Aqua name.</summary>
    Rare,
    /// <summary>Light-purple name.</summary>
    Epic,
}
