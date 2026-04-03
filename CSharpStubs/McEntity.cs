namespace CSCraft;

/// <summary>
/// Facade for a Minecraft entity (Entity).
/// Transpiles to Java's Entity.
/// </summary>
[JavaClass("net.minecraft.entity.Entity")]
public class McEntity
{
    [JavaMethod("{target}.getName().getString()")]
    public string Name { get; } = null!;

    [JavaMethod("{target}.getUuidAsString()")]
    public string Uuid { get; } = null!;

    [JavaMethod("{target}.getX()")]
    public double X { get; }

    [JavaMethod("{target}.getY()")]
    public double Y { get; }

    [JavaMethod("{target}.getZ()")]
    public double Z { get; }

    [JavaMethod("{target}.getBlockPos()")]
    public McBlockPos BlockPos { get; } = null!;

    [JavaMethod("{target}.isAlive()")]
    public bool IsAlive { get; }

    [JavaMethod("{target}.isOnGround()")]
    public bool IsOnGround { get; }

    [JavaMethod("{target}.getWorld()")]
    public McWorld World { get; } = null!;

    [JavaMethod("{target}.kill()")]
    public void Kill() { }

    [JavaMethod("{target}.discard()")]
    public void Remove() { }

    // ── Velocity ──────────────────────────────────────────────────────────────

    [JavaMethod("{target}.setVelocity({0}, {1}, {2})")]
    public void SetVelocity(double x, double y, double z) { }

    [JavaMethod("{target}.getVelocity().getX()")]
    public double VelocityX { get; }

    [JavaMethod("{target}.getVelocity().getY()")]
    public double VelocityY { get; }

    [JavaMethod("{target}.getVelocity().getZ()")]
    public double VelocityZ { get; }

    // ── Type ──────────────────────────────────────────────────────────────────

    [JavaMethod("EntityType.getId({target}.getType()).toString()")]
    public string TypeId { get; } = null!;

    [JavaMethod("{target} instanceof net.minecraft.entity.player.PlayerEntity")]
    public bool IsPlayer { get; }

    [JavaMethod("{target} instanceof net.minecraft.mob.MobEntity")]
    public bool IsMob { get; }

    // ── NBT ───────────────────────────────────────────────────────────────────

    [JavaMethod("{target}.getCustomData().getString({0})")]
    public string GetNbtString(string key) => null!;

    [JavaMethod("{target}.getCustomData().putString({0}, {1})")]
    public void SetNbtString(string key, string value) { }

    [JavaMethod("{target}.getCustomData().getInt({0})")]
    public int GetNbtInt(string key) => 0;

    [JavaMethod("{target}.getCustomData().putInt({0}, {1})")]
    public void SetNbtInt(string key, int value) { }
}
