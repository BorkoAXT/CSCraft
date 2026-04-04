namespace CSCraft;

/// <summary>
/// Static event declarations that mod authors subscribe to.
/// These map to Fabric API event registrations when transpiled.
/// </summary>
public static class Events
{
    // ── Player lifecycle ──────────────────────────────────────────────────────

    /// <summary>Fired when a connection is initialized, before the player fully joins.</summary>
    [JavaEvent("ServerPlayConnectionEvents", "INIT")]
    public static event Action<McPlayer> PlayerConnect = null!;

    [JavaEvent("ServerPlayConnectionEvents", "JOIN")]
    public static event Action<McPlayer> PlayerJoin = null!;

    [JavaEvent("ServerPlayConnectionEvents", "DISCONNECT")]
    public static event Action<McPlayer> PlayerLeave = null!;

    [JavaEvent("ServerLivingEntityEvents", "AFTER_DEATH")]
    public static event Action<McPlayer> PlayerDeath = null!;

    [JavaEvent("ServerPlayerEvents", "AFTER_RESPAWN")]
    public static event Action<McPlayer> PlayerRespawn = null!;

    /// <summary>Fired when a player is about to die. Return false to cancel death.</summary>
    [JavaEvent("ServerPlayerEvents", "ALLOW_DEATH")]
    public static event Action<McPlayer> PlayerAllowDeath = null!;

    /// <summary>Fired when copying player data on respawn or dimension transfer.</summary>
    [JavaEvent("ServerPlayerEvents", "COPY_FROM")]
    public static event Action<McPlayer> PlayerCopyFrom = null!;

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

    // ── Block events ──────────────────────────────────────────────────────────

    /// <summary>Fired when a player left-clicks (attacks) a block.</summary>
    [JavaEvent("AttackBlockCallback", "EVENT")]
    public static event Action<McPlayer, McBlockPos> BlockAttack = null!;

    [JavaEvent("PlayerBlockBreakEvents", "AFTER")]
    public static event Action<McPlayer, McBlockPos> BlockBreak = null!;

    /// <summary>Fired when a block break is canceled (e.g. blocked by another event handler).</summary>
    [JavaEvent("PlayerBlockBreakEvents", "CANCELED")]
    public static event Action<McPlayer, McBlockPos> BlockBreakCanceled = null!;

    [JavaEvent("UseBlockCallback", "EVENT")]
    public static event Action<McPlayer, McBlockPos> BlockInteract = null!;

    // ── Chat ──────────────────────────────────────────────────────────────────

    /// <summary>Fired before a chat message is sent. Return false to block it.</summary>
    [JavaEvent("ServerMessageEvents", "ALLOW_CHAT_MESSAGE")]
    public static event Action<McPlayer, string> ChatAllowed = null!;

    [JavaEvent("ServerMessageEvents", "CHAT_MESSAGE")]
    public static event Action<McPlayer, string> ChatMessage = null!;

    /// <summary>Fired before a command output message is sent. Return false to block it.</summary>
    [JavaEvent("ServerMessageEvents", "ALLOW_COMMAND_MESSAGES")]
    public static event Action<McPlayer, string> CommandMessageAllowed = null!;

    [JavaEvent("ServerMessageEvents", "COMMAND_MESSAGE")]
    public static event Action<McPlayer, string> CommandMessage = null!;

    // ── Item events ───────────────────────────────────────────────────────────

    [JavaEvent("UseItemCallback", "EVENT")]
    public static event Action<McPlayer, McItemStack> ItemUse = null!;

    /// <summary>Fired when a player picks up an item from the ground.</summary>
    [JavaEvent("EntityPickupItemEvents", "ALLOW_ENTITY_PICKUP")]
    public static event Action<McPlayer, McItemStack> ItemPickup = null!;

    /// <summary>Fired after an entity has picked up an item.</summary>
    [JavaEvent("EntityPickupItemEvents", "AFTER_PICKUP")]
    public static event Action<McPlayer, McItemStack> ItemAfterPickup = null!;

    // ── Server lifecycle ──────────────────────────────────────────────────────

    [JavaEvent("ServerLifecycleEvents", "SERVER_STARTED")]
    public static event Action<McServer> ServerStart = null!;

    [JavaEvent("ServerLifecycleEvents", "SERVER_STOPPING")]
    public static event Action<McServer> ServerStop = null!;

    [JavaEvent("ServerLifecycleEvents", "SERVER_STOPPED")]
    public static event Action<McServer> ServerStopped = null!;

    /// <summary>Fired when data packs are reloaded (e.g. /reload command).</summary>
    [JavaEvent("ServerLifecycleEvents", "SYNC_DATA_PACK_CONTENTS")]
    public static event Action<McServer> DataPacksReload = null!;

    [JavaEvent("ServerLifecycleEvents", "SERVER_STARTING")]
    public static event Action<McServer> ServerLoading = null!;

    [JavaEvent("ServerTickEvents", "END_SERVER_TICK")]
    public static event Action<McServer> ServerTick = null!;

    [JavaEvent("ServerTickEvents", "START_SERVER_TICK")]
    public static event Action<McServer> ServerTickStart = null!;

    [JavaEvent("ServerTickEvents", "START_WORLD_TICK")]
    public static event Action<McWorld> WorldTickStart = null!;

    [JavaEvent("ServerTickEvents", "END_WORLD_TICK")]
    public static event Action<McWorld> WorldTick = null!;

    [JavaEvent("ServerWorldEvents", "LOAD")]
    public static event Action<McWorld> WorldLoad = null!;

    [JavaEvent("ServerWorldEvents", "UNLOAD")]
    public static event Action<McWorld> WorldUnload = null!;

    // ── Entity events ─────────────────────────────────────────────────────────

    [JavaEvent("ServerEntityEvents", "ENTITY_LOAD")]
    public static event Action<McEntity> EntitySpawn = null!;

    [JavaEvent("ServerLivingEntityEvents", "AFTER_DEATH")]
    public static event Action<McEntity> EntityDeath = null!;

    /// <summary>Fired when a living entity takes damage. Return false to cancel.</summary>
    [JavaEvent("ServerLivingEntityEvents", "ALLOW_DAMAGE")]
    public static event Action<McEntity, float> EntityHurt = null!;

    /// <summary>Fired after a living entity has taken damage (damage already applied).</summary>
    [JavaEvent("ServerLivingEntityEvents", "AFTER_DAMAGE")]
    public static event Action<McEntity, float> EntityAfterHurt = null!;

    /// <summary>Fired when an entity is about to die. Return false to cancel death.</summary>
    [JavaEvent("ServerLivingEntityEvents", "ALLOW_DEATH")]
    public static event Action<McEntity> EntityAllowDeath = null!;

    [JavaEvent("ServerEntityEvents", "ENTITY_UNLOAD")]
    public static event Action<McEntity> EntityUnload = null!;

    // ── Sleep events ─────────────────────────────────────────────────────────

    /// <summary>Fired when a player starts sleeping in a bed.</summary>
    [JavaEvent("PlayerSleepInBedCallback", "EVENT")]
    public static event Action<McPlayer, McBlockPos> PlayerSleep = null!;

    /// <summary>Fired when a player wakes up from sleeping.</summary>
    [JavaEvent("ServerPlayerEvents", "AFTER_RESPAWN")]
    public static event Action<McPlayer> PlayerWakeUp = null!;

    // ── Chunk events ──────────────────────────────────────────────────────────

    [JavaEvent("ServerChunkEvents", "CHUNK_LOAD")]
    public static event Action<McWorld> ChunkLoad = null!;

    [JavaEvent("ServerChunkEvents", "CHUNK_UNLOAD")]
    public static event Action<McWorld> ChunkUnload = null!;

    [JavaEvent("ServerBlockEntityEvents", "BLOCK_ENTITY_LOAD")]
    public static event Action<McWorld> BlockEntityLoad = null!;

    [JavaEvent("ServerBlockEntityEvents", "BLOCK_ENTITY_UNLOAD")]
    public static event Action<McWorld> BlockEntityUnload = null!;

}
