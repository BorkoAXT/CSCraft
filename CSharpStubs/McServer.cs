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

    [JavaMethod("{target}.getPlayerManager().getPlayer(java.util.UUID.fromString({0}))")]
    public McPlayer? GetPlayerByUuid(string uuid) => null;

    [JavaMethod("java.util.stream.StreamSupport.stream({target}.getWorlds().spliterator(), false).toList()")]
    public List<McWorld> GetAllWorlds() => null!;

    [JavaMethod("{target}.getCommandManager().executeWithPrefix({target}.getCommandSource(), {0})")]
    public void RunCommand(string command) { }

    [JavaMethod("{target}.stop(false)")]
    public void Shutdown() { }

    // ── Worlds ────────────────────────────────────────────────────────────────

    /// <summary>Get the Overworld.</summary>
    [JavaMethod("(ServerWorld){target}.getWorld(World.OVERWORLD)")]
    public McWorld Overworld { get; } = null!;

    /// <summary>Get the Nether.</summary>
    [JavaMethod("(ServerWorld){target}.getWorld(World.NETHER)")]
    public McWorld Nether { get; } = null!;

    /// <summary>Get the End.</summary>
    [JavaMethod("(ServerWorld){target}.getWorld(World.END)")]
    public McWorld End { get; } = null!;

    // ── Server properties ─────────────────────────────────────────────────────

    [JavaMethod("{target}.isHardcore()")]
    public bool IsHardcore { get; }

    [JavaMethod("{target}.getOverworld().getSeed()")]
    public long GetSeed() => 0;

    [JavaMethod("{target}.getDefaultGameMode().getName()")]
    public string GetDefaultGameMode() => null!;

    [JavaMethod("{target}.setDefaultGameMode(net.minecraft.world.GameMode.byName({0}))")]
    public void SetDefaultGameMode(string mode) { }

    // ── Ban / whitelist ───────────────────────────────────────────────────────

    [JavaMethod("{target}.getPlayerManager().getUserBanList().contains(new net.minecraft.server.BannedPlayerEntry(new com.mojang.authlib.GameProfile(null, {0})))")]
    public bool IsPlayerBanned(string name) => false;

    [JavaMethod("{ var _profile = new com.mojang.authlib.GameProfile(null, {0}); var _entry = new net.minecraft.server.BannedPlayerEntry(_profile, null, null, null, {1}); {target}.getPlayerManager().getUserBanList().add(_entry); var _p = {target}.getPlayerManager().getPlayer({0}); if (_p != null) _p.networkHandler.disconnect(Text.literal({1})); }")]
    public void BanPlayer(string name, string reason) { }

    [JavaMethod("{target}.getPlayerManager().getUserBanList().remove(new com.mojang.authlib.GameProfile(null, {0}))")]
    public void PardonPlayer(string name) { }

    [JavaMethod("{target}.getPlayerManager().isWhitelistEnabled()")]
    public bool IsWhitelistEnabled { get; }

    [JavaMethod("{target}.getPlayerManager().setWhitelistEnabled({0})")]
    public void SetWhitelistEnabled(bool enabled) { }

    [JavaMethod("{target}.getPlayerManager().getWhitelist().isAllowed(new com.mojang.authlib.GameProfile(null, {0}))")]
    public bool IsWhitelisted(string name) => false;

    [JavaMethod("{target}.getPlayerManager().getWhitelist().add(new net.minecraft.server.WhitelistEntry(new com.mojang.authlib.GameProfile(null, {0})))")]
    public void AddToWhitelist(string name) { }

    [JavaMethod("{target}.getPlayerManager().getWhitelist().remove(new net.minecraft.server.WhitelistEntry(new com.mojang.authlib.GameProfile(null, {0})))")]
    public void RemoveFromWhitelist(string name) { }

    // ── Scoreboard ────────────────────────────────────────────────────────────

    /// <summary>Get or create a scoreboard objective.</summary>
    [JavaMethod("{target}.getScoreboard().getNullableObjective({0}) != null ? {target}.getScoreboard().getNullableObjective({0}) : {target}.getScoreboard().addObjective({0}, ScoreboardCriterion.DUMMY, Text.literal({0}), ScoreboardCriterion.RenderType.INTEGER)")]
    public object GetScoreboard(string name) => null!;

    /// <summary>Get a player's score on an objective.</summary>
    [JavaMethod("{target}.getScoreboard().getOrCreateScore({1}, {target}.getScoreboard().getNullableObjective({0})).getScore()")]
    public int GetScore(string objective, McPlayer player) => 0;

    /// <summary>Set a player's score on an objective.</summary>
    [JavaMethod("{target}.getScoreboard().getOrCreateScore({1}, {target}.getScoreboard().getNullableObjective({0})).setScore({2})")]
    public void SetScore(string objective, McPlayer player, int score) { }
}
