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

    // ── Server lifecycle ──────────────────────────────────────────────────────

    [JavaEvent("ServerLifecycleEvents", "SERVER_STARTED")]
    public static event Action<McServer> ServerStart = null!;

    [JavaEvent("ServerLifecycleEvents", "SERVER_STOPPING")]
    public static event Action<McServer> ServerStop = null!;

    [JavaEvent("ServerTickEvents", "END_SERVER_TICK")]
    public static event Action<McServer> ServerTick = null!;

    [JavaEvent("ServerTickEvents", "END_WORLD_TICK")]
    public static event Action<McWorld> WorldTick = null!;

    // ── Entity events ─────────────────────────────────────────────────────────

    [JavaEvent("ServerEntityEvents", "ENTITY_LOAD")]
    public static event Action<McEntity> EntitySpawn = null!;

    [JavaEvent("ServerLivingEntityEvents", "AFTER_DEATH")]
    public static event Action<McEntity> EntityDeath = null!;

    // ── Item events ───────────────────────────────────────────────────────────

    [JavaEvent("UseItemCallback", "EVENT")]
    public static event Action<McPlayer, McItemStack> ItemUse = null!;

    // ── Chunk events ──────────────────────────────────────────────────────────

    [JavaEvent("ServerChunkEvents", "CHUNK_LOAD")]
    public static event Action<McWorld> ChunkLoad = null!;

    [JavaEvent("ServerChunkEvents", "CHUNK_UNLOAD")]
    public static event Action<McWorld> ChunkUnload = null!;
}
