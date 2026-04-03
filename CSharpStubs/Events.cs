namespace CSCraft;

/// <summary>
/// Static event declarations that mod authors subscribe to.
/// These map to Fabric API event registrations when transpiled.
/// </summary>
public static class Events
{
    // ── Player lifecycle ──────────────────────────────────────────────────────

    [JavaEvent("ServerPlayConnectionEvents", "JOIN")]
    public static event Action<McPlayer> PlayerJoin = null!;

    [JavaEvent("ServerPlayConnectionEvents", "DISCONNECT")]
    public static event Action<McPlayer> PlayerLeave = null!;

    [JavaEvent("ServerLivingEntityEvents", "AFTER_DEATH")]
    public static event Action<McPlayer> PlayerDeath = null!;

    [JavaEvent("ServerEntityEvents", "ENTITY_LOAD")]
    public static event Action<McPlayer> PlayerRespawn = null!;

    // ── Player combat ─────────────────────────────────────────────────────────

    /// <summary>Fired when a player takes damage. Return false to cancel.</summary>
    [JavaEvent("ServerLivingEntityEvents", "ALLOW_DAMAGE")]
    public static event Action<McPlayer, float> PlayerHurt = null!;

    /// <summary>Fired when a player attacks another entity.</summary>
    [JavaEvent("AttackEntityCallback", "EVENT")]
    public static event Action<McPlayer, McEntity> PlayerAttack = null!;

    // ── Player interaction ────────────────────────────────────────────────────

    /// <summary>Fired when a player right-clicks an entity.</summary>
    [JavaEvent("UseEntityCallback", "EVENT")]
    public static event Action<McPlayer, McEntity> PlayerUseEntity = null!;

    /// <summary>Fired when a player swings their arm (attack animation).</summary>
    [JavaEvent("ServerPlayNetworkHandler", "SWING")]
    public static event Action<McPlayer> PlayerSwing = null!;

    // ── Block events ──────────────────────────────────────────────────────────

    [JavaEvent("PlayerBlockBreakEvents", "AFTER")]
    public static event Action<McPlayer, McBlockPos> BlockBreak = null!;

    [JavaEvent("PlayerBlockBreakEvents", "BEFORE")]
    public static event Action<McPlayer, McBlockPos> BlockPlace = null!;

    [JavaEvent("UseBlockCallback", "EVENT")]
    public static event Action<McPlayer, McBlockPos> BlockInteract = null!;

    // ── Chat ──────────────────────────────────────────────────────────────────

    [JavaEvent("ServerMessageEvents", "CHAT_MESSAGE")]
    public static event Action<McPlayer, string> ChatMessage = null!;

    [JavaEvent("ServerMessageEvents", "COMMAND_MESSAGE")]
    public static event Action<McPlayer, string> CommandMessage = null!;

    // ── Item events ───────────────────────────────────────────────────────────

    [JavaEvent("UseItemCallback", "EVENT")]
    public static event Action<McPlayer, McItemStack> ItemUse = null!;

    /// <summary>Fired when a player picks up an item from the ground.</summary>
    [JavaEvent("EntityPickupItemEvents", "ALLOW_ENTITY_PICKUP")]
    public static event Action<McPlayer, McItemStack> ItemPickup = null!;

    /// <summary>Fired when an item finishes being used (eating, drinking, etc.).</summary>
    [JavaEvent("ServerPlayerEvents", "AFTER_RESPAWN")]
    public static event Action<McPlayer, McItemStack> ItemFinishUsing = null!;

    // ── Inventory ─────────────────────────────────────────────────────────────

    /// <summary>Fired when a player crafts an item.</summary>
    [JavaEvent("ServerPlayerEvents", "AFTER_RESPAWN")]
    public static event Action<McPlayer, McItemStack> ItemCraft = null!;

    // ── Server lifecycle ──────────────────────────────────────────────────────

    [JavaEvent("ServerLifecycleEvents", "SERVER_STARTED")]
    public static event Action<McServer> ServerStart = null!;

    [JavaEvent("ServerLifecycleEvents", "SERVER_STOPPING")]
    public static event Action<McServer> ServerStop = null!;

    [JavaEvent("ServerLifecycleEvents", "SERVER_LOADING")]
    public static event Action<McServer> ServerLoading = null!;

    [JavaEvent("ServerTickEvents", "END_SERVER_TICK")]
    public static event Action<McServer> ServerTick = null!;

    [JavaEvent("ServerTickEvents", "START_SERVER_TICK")]
    public static event Action<McServer> ServerTickStart = null!;

    [JavaEvent("ServerTickEvents", "END_WORLD_TICK")]
    public static event Action<McWorld> WorldTick = null!;

    // ── Entity events ─────────────────────────────────────────────────────────

    [JavaEvent("ServerEntityEvents", "ENTITY_LOAD")]
    public static event Action<McEntity> EntitySpawn = null!;

    [JavaEvent("ServerLivingEntityEvents", "AFTER_DEATH")]
    public static event Action<McEntity> EntityDeath = null!;

    /// <summary>Fired when a living entity takes damage. Return false to cancel.</summary>
    [JavaEvent("ServerLivingEntityEvents", "ALLOW_DAMAGE")]
    public static event Action<McEntity, float> EntityHurt = null!;

    // ── Chunk events ──────────────────────────────────────────────────────────

    [JavaEvent("ServerChunkEvents", "CHUNK_LOAD")]
    public static event Action<McWorld> ChunkLoad = null!;

    [JavaEvent("ServerChunkEvents", "CHUNK_UNLOAD")]
    public static event Action<McWorld> ChunkUnload = null!;

    // ── Command registration ──────────────────────────────────────────────────

    /// <summary>
    /// Fired during command registration. Use McCommand.Register() instead
    /// for a simpler API — this event is for advanced Brigadier usage.
    /// </summary>
    [JavaEvent("CommandRegistrationCallback", "EVENT")]
    public static event Action<McServer> CommandRegister = null!;

    // ── World generation ──────────────────────────────────────────────────────

    /// <summary>Fired after a chunk is generated (before it is saved).</summary>
    [JavaEvent("ChunkGeneratorEvents", "UNSUPPORTED_FEATURE")]
    public static event Action<McWorld> ChunkGenerate = null!;
}
