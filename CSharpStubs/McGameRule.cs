namespace CSCraft;

/// <summary>
/// Provides access to Minecraft game rules on the server.
/// Transpiles to Java's GameRules lookups.
/// </summary>
public static class McGameRules
{
    // ── Boolean rules ─────────────────────────────────────────────────────────

    [JavaMethod("{0}.getGameRules().getBoolean(GameRules.DO_MOB_SPAWNING)")]
    public static bool DoMobSpawning(McServer server) => true;

    [JavaMethod("{0}.getGameRules().getBoolean(GameRules.DO_DAYLIGHT_CYCLE)")]
    public static bool DoDaylightCycle(McServer server) => true;

    [JavaMethod("{0}.getGameRules().getBoolean(GameRules.DO_FIRE_TICK)")]
    public static bool DoFireTick(McServer server) => true;

    [JavaMethod("{0}.getGameRules().getBoolean(GameRules.DO_MOB_LOOT)")]
    public static bool DoMobLoot(McServer server) => true;

    [JavaMethod("{0}.getGameRules().getBoolean(GameRules.KEEP_INVENTORY)")]
    public static bool KeepInventory(McServer server) => false;

    [JavaMethod("{0}.getGameRules().getBoolean(GameRules.MOB_GRIEFING)")]
    public static bool MobGriefing(McServer server) => true;

    [JavaMethod("{0}.getGameRules().getBoolean(GameRules.PVP)")]
    public static bool Pvp(McServer server) => true;

    [JavaMethod("{0}.getGameRules().getBoolean(GameRules.SHOW_DEATH_MESSAGES)")]
    public static bool ShowDeathMessages(McServer server) => true;

    // ── Integer rules ─────────────────────────────────────────────────────────

    [JavaMethod("{0}.getGameRules().getInt(GameRules.PLAYERS_SLEEPING_PERCENTAGE)")]
    public static int PlayersSleepingPercentage(McServer server) => 100;

    [JavaMethod("{0}.getGameRules().getInt(GameRules.MAX_ENTITY_CRAMMING)")]
    public static int MaxEntityCramming(McServer server) => 24;

    [JavaMethod("{0}.getGameRules().getInt(GameRules.RANDOM_TICK_SPEED)")]
    public static int RandomTickSpeed(McServer server) => 3;
}
