namespace CSCraft;

/// <summary>
/// Facade for the Minecraft server (MinecraftServer).
/// Transpiles to Java's MinecraftServer.
/// </summary>
[JavaClass("net.minecraft.server.MinecraftServer")]
public class McServer
{
    // ── Players ───────────────────────────────────────────────────────────────

    [JavaMethod("{target}.getPlayerManager().getPlayerList()")]
    public List<McPlayer> OnlinePlayers { get; } = null!;

    [JavaMethod("{target}.getPlayerManager().getCurrentPlayerCount()")]
    public int PlayerCount { get; }

    [JavaMethod("{target}.getPlayerManager().getMaxPlayerCount()")]
    public int MaxPlayers { get; }

    // ── Server info ───────────────────────────────────────────────────────────

    [JavaMethod("{target}.getTickTime()")]
    public float Tps { get; }

    [JavaMethod("{target}.getVersion()")]
    public string Version { get; } = null!;

    [JavaMethod("{target}.isRunning()")]
    public bool IsRunning { get; }

    [JavaMethod("{target}.getServerMotd()")]
    public string Motd { get; } = null!;

    // ── Actions ───────────────────────────────────────────────────────────────

    [JavaMethod("{target}.getPlayerManager().broadcast(Text.literal({0}), false)")]
    public void Broadcast(string message) { }

    [JavaMethod("{target}.getPlayerManager().getPlayer({0})")]
    public McPlayer? GetPlayer(string name) => null;

    [JavaMethod("{target}.getCommandManager().executeWithPrefix({target}.getCommandSource(), {0})")]
    public void RunCommand(string command) { }

    [JavaMethod("{target}.stop(false)")]
    public void Shutdown() { }
}
