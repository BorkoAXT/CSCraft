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
}
