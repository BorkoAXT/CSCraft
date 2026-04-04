namespace CSCraft;

/// <summary>
/// Full NBT compound read/write for entities, block entities, and item stacks.
/// Wraps NbtCompound.
/// </summary>
[JavaClass("net.minecraft.nbt.NbtCompound")]
public class McNbt
{
    public McNbt() { }

    // ── Strings ───────────────────────────────────────────────────────────────

    [JavaMethod("{target}.getString({0})")]
    public string GetString(string key) => null!;

    [JavaMethod("{target}.putString({0}, {1})")]
    public void SetString(string key, string value) { }

    [JavaMethod("{target}.contains({0}) && {target}.getType({0}) == 8")]
    public bool HasString(string key) => false;

    // ── Integers ──────────────────────────────────────────────────────────────

    [JavaMethod("{target}.getInt({0})")]
    public int GetInt(string key) => 0;

    [JavaMethod("{target}.putInt({0}, {1})")]
    public void SetInt(string key, int value) { }

    // ── Longs ─────────────────────────────────────────────────────────────────

    [JavaMethod("{target}.getLong({0})")]
    public long GetLong(string key) => 0;

    [JavaMethod("{target}.putLong({0}, {1})")]
    public void SetLong(string key, long value) { }

    // ── Floats / doubles ──────────────────────────────────────────────────────

    [JavaMethod("{target}.getFloat({0})")]
    public float GetFloat(string key) => 0;

    [JavaMethod("{target}.putFloat({0}, {1})")]
    public void SetFloat(string key, float value) { }

    [JavaMethod("{target}.getDouble({0})")]
    public double GetDouble(string key) => 0;

    [JavaMethod("{target}.putDouble({0}, {1})")]
    public void SetDouble(string key, double value) { }

    // ── Booleans ──────────────────────────────────────────────────────────────

    [JavaMethod("{target}.getBoolean({0})")]
    public bool GetBool(string key) => false;

    [JavaMethod("{target}.putBoolean({0}, {1})")]
    public void SetBool(string key, bool value) { }

    // ── Nested compounds ──────────────────────────────────────────────────────

    [JavaMethod("{target}.getCompound({0})")]
    public McNbt GetCompound(string key) => null!;

    [JavaMethod("{target}.put({0}, {1})")]
    public void SetCompound(string key, McNbt value) { }

    // ── Existence ─────────────────────────────────────────────────────────────

    [JavaMethod("{target}.contains({0})")]
    public bool Has(string key) => false;

    [JavaMethod("{target}.remove({0})")]
    public void Remove(string key) { }

    [JavaMethod("{target}.isEmpty()")]
    public bool IsEmpty { get; }

    [JavaMethod("{target}.getKeys()")]
    public List<string> GetKeys() => null!;

    // ── Entity helpers ────────────────────────────────────────────────────────

    /// <summary>Read the persistent custom NBT data from an entity.</summary>
    [JavaMethod("{0}.getCustomData().copy()")]
    public static McNbt FromEntity(McEntity entity) => null!;

    /// <summary>Write an NBT compound into an entity's custom data.</summary>
    [JavaMethod("{1}.getKeys().forEach(k -> {0}.getCustomData().put(k, {1}.get(k)))")]
    public static void ToEntity(McEntity entity, McNbt nbt) { }

    /// <summary>Read NBT from an item stack.</summary>
    [JavaMethod("{0}.hasNbt() ? {0}.getNbt().copy() : new NbtCompound()")]
    public static McNbt FromItem(McItemStack stack) => null!;

    /// <summary>Write NBT into an item stack.</summary>
    [JavaMethod("{1}.getKeys().forEach(k -> { if ({0}.getNbt() == null) {0}.setNbt(new NbtCompound()); {0}.getNbt().put(k, {1}.get(k)); })")]
    public static void ToItem(McItemStack stack, McNbt nbt) { }
}
