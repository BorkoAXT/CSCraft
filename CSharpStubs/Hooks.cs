namespace CSCraft;

/// <summary>
/// Pre-event interceptors that fire before an action happens and can cancel or
/// modify its outcome. Unlike Events (which fire after), Hooks fire before.
///
/// Hook vs Event quick reference:
///   Events — fire after the action, cannot cancel, read-only args.
///   Hooks  — fire before the action, can cancel (e.Cancel()), can modify args.
/// </summary>
public static class Hooks
{
    /// <summary>
    /// Intercept block drops when a block is broken.
    /// Modify e.Drops to change what drops; call e.Cancel() to prevent all drops.
    /// </summary>
    public static event Action<BlockDropEvent> BlockDrop = null!;

    /// <summary>
    /// Intercept chat before it is broadcast.
    /// Modify e.Message to censor/filter it; call e.Cancel() to suppress entirely.
    /// </summary>
    public static event Action<PlayerChatHookEvent> PlayerChat = null!;

    /// <summary>
    /// Intercept damage before it is applied to any entity.
    /// Set e.Amount to change the damage; call e.Cancel() to prevent it entirely.
    /// </summary>
    public static event Action<EntityDamageEvent> EntityDamage = null!;

    /// <summary>
    /// Intercept explosions before they affect the world.
    /// Set e.DestroyBlocks = false to make explosions non-griefing;
    /// set e.PlayerDamageMultiplier to scale player damage.
    /// </summary>
    public static event Action<ExplosionHookEvent> ExplosionDamage = null!;

    /// <summary>
    /// Intercept player movement every tick.
    /// Expensive — keep handlers fast. Call e.Cancel() to revert the move.
    /// </summary>
    public static event Action<PlayerMoveEvent> PlayerMove = null!;

    /// <summary>
    /// Intercept player damage specifically (subset of EntityDamage for players).
    /// </summary>
    public static event Action<PlayerDamageEvent> PlayerDamage = null!;
}

// ── Event arg types ───────────────────────────────────────────────────────────

/// <summary>Block drop hook arguments.</summary>
public class BlockDropEvent
{
    /// <summary>The block that was broken (e.g. "minecraft:diamond_ore").</summary>
    public string Block { get; set; } = null!;
    /// <summary>The player who broke the block, or null if broken by the environment.</summary>
    public McPlayer? Player { get; set; }
    /// <summary>The position of the broken block.</summary>
    public McBlockPos Pos { get; set; } = null!;
    /// <summary>The items that will drop. Modify this list to change drops.</summary>
    public List<DroppedItem> Drops { get; set; } = null!;
    /// <summary>Prevent all drops from this break.</summary>
    public void Cancel() { }
}

/// <summary>A single item drop entry.</summary>
public record DroppedItem(string ItemId, int Count);

/// <summary>Player chat hook arguments.</summary>
public class PlayerChatHookEvent
{
    public McPlayer Player { get; set; } = null!;
    /// <summary>The message text. Set this to modify what is sent.</summary>
    public string Message { get; set; } = null!;
    /// <summary>Override the full display format, e.g. "[ADMIN] Name: message".</summary>
    public string? Format { get; set; }
    /// <summary>Suppress this chat message entirely.</summary>
    public void Cancel() { }
}

/// <summary>Entity damage hook arguments.</summary>
public class EntityDamageEvent
{
    /// <summary>The attacker, or null if the damage source has no entity.</summary>
    public McEntity? Attacker { get; set; }
    public McEntity Target { get; set; } = null!;
    /// <summary>Damage amount. Set this to change how much damage is dealt.</summary>
    public float Amount { get; set; }
    public DamageCause Cause { get; set; }
    /// <summary>Cancel the damage entirely.</summary>
    public void Cancel() { }
}

/// <summary>Explosion hook arguments.</summary>
public class ExplosionHookEvent
{
    /// <summary>The entity that caused the explosion (e.g. "minecraft:creeper"), or null.</summary>
    public string? Source { get; set; }
    /// <summary>Set to false to prevent the explosion from destroying blocks.</summary>
    public bool DestroyBlocks { get; set; } = true;
    /// <summary>Multiply or zero out player damage (1.0 = normal, 0 = no damage to players).</summary>
    public float PlayerDamageMultiplier { get; set; } = 1.0f;
    /// <summary>Cancel the explosion entirely (no block damage, no entity damage).</summary>
    public void Cancel() { }
}

/// <summary>Player move hook arguments.</summary>
public class PlayerMoveEvent
{
    public McPlayer Player { get; set; } = null!;
    public McBlockPos From { get; set; } = null!;
    public McBlockPos To { get; set; } = null!;
    /// <summary>Cancel the move (teleports player back to From).</summary>
    public void Cancel() { }
}

/// <summary>Player damage hook arguments (player-specific subset of EntityDamageEvent).</summary>
public class PlayerDamageEvent
{
    public McPlayer Player { get; set; } = null!;
    /// <summary>Damage amount. Set to 0 to nullify (e.g. no fall damage).</summary>
    public float Amount { get; set; }
    public DamageCause Cause { get; set; }
    /// <summary>Cancel the damage entirely.</summary>
    public void Cancel() { }
}

/// <summary>The cause of a damage instance.</summary>
public enum DamageCause
{
    Generic, Fall, Fire, Lava, Drowning, Explosion, Arrow, Melee,
    Magic, Void, Starve, Poison, Wither, Lightning, Cactus,
    Cramming, DragonBreath, OutOfWorld, Thorns, Sting
}
