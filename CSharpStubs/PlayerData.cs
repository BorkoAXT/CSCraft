namespace CSCraft;

/// <summary>
/// Persistent per-player data storage.
/// Data is saved to the player's NBT automatically and survives restarts.
/// Supported types: int, long, float, double, bool, string, McBlockPos.
/// </summary>
public static class PlayerData
{
    // ── Write ─────────────────────────────────────────────────────────────────

    /// <summary>Store an integer value on the player.</summary>
    [JavaMethod("{0}.getCustomData().putInt({1}, {2})")]
    public static void Set(McPlayer player, string key, int value) { }

    /// <summary>Store a long value on the player.</summary>
    [JavaMethod("{0}.getCustomData().putLong({1}, {2})")]
    public static void Set(McPlayer player, string key, long value) { }

    /// <summary>Store a float value on the player.</summary>
    [JavaMethod("{0}.getCustomData().putFloat({1}, {2})")]
    public static void Set(McPlayer player, string key, float value) { }

    /// <summary>Store a double value on the player.</summary>
    [JavaMethod("{0}.getCustomData().putDouble({1}, {2})")]
    public static void Set(McPlayer player, string key, double value) { }

    /// <summary>Store a boolean value on the player.</summary>
    [JavaMethod("{0}.getCustomData().putBoolean({1}, {2})")]
    public static void Set(McPlayer player, string key, bool value) { }

    /// <summary>Store a string value on the player.</summary>
    [JavaMethod("{0}.getCustomData().putString({1}, {2})")]
    public static void Set(McPlayer player, string key, string value) { }

    /// <summary>Store a BlockPos on the player (packed as three ints).</summary>
    [JavaMethod("{ net.minecraft.nbt.NbtCompound _pd = {0}.getCustomData(); _pd.putInt({1} + \"_x\", {2}.getX()); _pd.putInt({1} + \"_y\", {2}.getY()); _pd.putInt({1} + \"_z\", {2}.getZ()); }")]
    public static void Set(McPlayer player, string key, McBlockPos pos) { }

    // ── Read ──────────────────────────────────────────────────────────────────

    /// <summary>Get an integer value; returns defaultValue if the key is absent.</summary>
    [JavaMethod("{0}.getCustomData().contains({1}) ? {0}.getCustomData().getInt({1}) : {2}")]
    public static int GetInt(McPlayer player, string key, int defaultValue = 0) => defaultValue;

    /// <summary>Get a long value.</summary>
    [JavaMethod("{0}.getCustomData().contains({1}) ? {0}.getCustomData().getLong({1}) : {2}")]
    public static long GetLong(McPlayer player, string key, long defaultValue = 0) => defaultValue;

    /// <summary>Get a float value.</summary>
    [JavaMethod("{0}.getCustomData().contains({1}) ? {0}.getCustomData().getFloat({1}) : {2}")]
    public static float GetFloat(McPlayer player, string key, float defaultValue = 0f) => defaultValue;

    /// <summary>Get a double value.</summary>
    [JavaMethod("{0}.getCustomData().contains({1}) ? {0}.getCustomData().getDouble({1}) : {2}")]
    public static double GetDouble(McPlayer player, string key, double defaultValue = 0.0) => defaultValue;

    /// <summary>Get a boolean value.</summary>
    [JavaMethod("{0}.getCustomData().contains({1}) ? {0}.getCustomData().getBoolean({1}) : {2}")]
    public static bool GetBool(McPlayer player, string key, bool defaultValue = false) => defaultValue;

    /// <summary>Get a string value.</summary>
    [JavaMethod("{0}.getCustomData().contains({1}) ? {0}.getCustomData().getString({1}) : {2}")]
    public static string GetString(McPlayer player, string key, string defaultValue = "") => defaultValue;

    /// <summary>Get a BlockPos stored with Set(player, key, pos).</summary>
    [JavaMethod("new net.minecraft.util.math.BlockPos({0}.getCustomData().getInt({1} + \"_x\"), {0}.getCustomData().getInt({1} + \"_y\"), {0}.getCustomData().getInt({1} + \"_z\"))")]
    public static McBlockPos GetBlockPos(McPlayer player, string key) => null!;

    // ── Existence / removal ───────────────────────────────────────────────────

    /// <summary>Whether the player has a stored value for this key.</summary>
    [JavaMethod("{0}.getCustomData().contains({1})")]
    public static bool Has(McPlayer player, string key) => false;

    /// <summary>Delete a stored value.</summary>
    [JavaMethod("{0}.getCustomData().remove({1})")]
    public static void Remove(McPlayer player, string key) { }
}
