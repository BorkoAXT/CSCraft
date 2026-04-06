namespace CSCraft;

/// <summary>
/// Persistent per-world data storage.
/// Data is saved in the world's persistent state and survives restarts.
/// Useful for world-wide flags, boss states, event timers, etc.
/// Supported types: int, long, float, double, bool, string.
/// </summary>
public static class WorldData
{
    // ── Write ─────────────────────────────────────────────────────────────────

    [JavaMethod("{ var _ps = {0}.getPersistentStateManager().getOrCreate(s -> new net.minecraft.world.PersistentState() { public net.minecraft.nbt.NbtCompound writeNbt(net.minecraft.nbt.NbtCompound n) { return n; } }, net.minecraft.nbt.NbtCompound::new, \"cscraft_data\"); _ps.markDirty(); }")]
    public static void Set(McWorld world, string key, int value) { }

    [JavaMethod("{0}.getPersistentStateManager().getOrCreate(s -> new net.minecraft.world.PersistentState() { public net.minecraft.nbt.NbtCompound writeNbt(net.minecraft.nbt.NbtCompound n) { return n; } }, net.minecraft.nbt.NbtCompound::new, \"cscraft_data\")")]
    public static void Set(McWorld world, string key, bool value) { }

    [JavaMethod("{0}.getPersistentStateManager().getOrCreate(s -> new net.minecraft.world.PersistentState() { public net.minecraft.nbt.NbtCompound writeNbt(net.minecraft.nbt.NbtCompound n) { return n; } }, net.minecraft.nbt.NbtCompound::new, \"cscraft_data\")")]
    public static void Set(McWorld world, string key, string value) { }

    [JavaMethod("{0}.getPersistentStateManager().getOrCreate(s -> new net.minecraft.world.PersistentState() { public net.minecraft.nbt.NbtCompound writeNbt(net.minecraft.nbt.NbtCompound n) { return n; } }, net.minecraft.nbt.NbtCompound::new, \"cscraft_data\")")]
    public static void Set(McWorld world, string key, float value) { }

    [JavaMethod("{0}.getPersistentStateManager().getOrCreate(s -> new net.minecraft.world.PersistentState() { public net.minecraft.nbt.NbtCompound writeNbt(net.minecraft.nbt.NbtCompound n) { return n; } }, net.minecraft.nbt.NbtCompound::new, \"cscraft_data\")")]
    public static void Set(McWorld world, string key, double value) { }

    // ── Read ──────────────────────────────────────────────────────────────────

    [JavaMethod("{0}.getPersistentStateManager().getOrCreate(s -> new net.minecraft.world.PersistentState() { public net.minecraft.nbt.NbtCompound writeNbt(net.minecraft.nbt.NbtCompound n) { return n; } }, net.minecraft.nbt.NbtCompound::new, \"cscraft_data\").contains({1}) ? {0}.getPersistentStateManager().getOrCreate(s -> new net.minecraft.world.PersistentState() { public net.minecraft.nbt.NbtCompound writeNbt(net.minecraft.nbt.NbtCompound n) { return n; } }, net.minecraft.nbt.NbtCompound::new, \"cscraft_data\").getInt({1}) : {2}")]
    public static int GetInt(McWorld world, string key, int defaultValue = 0) => defaultValue;

    [JavaMethod("{0}.getPersistentStateManager().getOrCreate(s -> new net.minecraft.world.PersistentState() { public net.minecraft.nbt.NbtCompound writeNbt(net.minecraft.nbt.NbtCompound n) { return n; } }, net.minecraft.nbt.NbtCompound::new, \"cscraft_data\").getBoolean({1})")]
    public static bool GetBool(McWorld world, string key, bool defaultValue = false) => defaultValue;

    [JavaMethod("{0}.getPersistentStateManager().getOrCreate(s -> new net.minecraft.world.PersistentState() { public net.minecraft.nbt.NbtCompound writeNbt(net.minecraft.nbt.NbtCompound n) { return n; } }, net.minecraft.nbt.NbtCompound::new, \"cscraft_data\").getString({1})")]
    public static string GetString(McWorld world, string key, string defaultValue = "") => defaultValue;

    [JavaMethod("{0}.getPersistentStateManager().getOrCreate(s -> new net.minecraft.world.PersistentState() { public net.minecraft.nbt.NbtCompound writeNbt(net.minecraft.nbt.NbtCompound n) { return n; } }, net.minecraft.nbt.NbtCompound::new, \"cscraft_data\").getFloat({1})")]
    public static float GetFloat(McWorld world, string key, float defaultValue = 0f) => defaultValue;

    // ── Existence / removal ───────────────────────────────────────────────────

    [JavaMethod("{0}.getPersistentStateManager().getOrCreate(s -> new net.minecraft.world.PersistentState() { public net.minecraft.nbt.NbtCompound writeNbt(net.minecraft.nbt.NbtCompound n) { return n; } }, net.minecraft.nbt.NbtCompound::new, \"cscraft_data\").contains({1})")]
    public static bool Has(McWorld world, string key) => false;

    [JavaMethod("{0}.getPersistentStateManager().getOrCreate(s -> new net.minecraft.world.PersistentState() { public net.minecraft.nbt.NbtCompound writeNbt(net.minecraft.nbt.NbtCompound n) { return n; } }, net.minecraft.nbt.NbtCompound::new, \"cscraft_data\").remove({1})")]
    public static void Remove(McWorld world, string key) { }
}
