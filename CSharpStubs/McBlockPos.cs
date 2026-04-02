namespace CSCraft;

/// <summary>
/// Facade for a Minecraft block position (BlockPos).
/// Transpiles to Java's BlockPos.
/// </summary>
[JavaClass("net.minecraft.util.math.BlockPos")]
public class McBlockPos
{
    public McBlockPos(int x, int y, int z) { }

    [JavaMethod("{target}.getX()")]
    public int X { get; }

    [JavaMethod("{target}.getY()")]
    public int Y { get; }

    [JavaMethod("{target}.getZ()")]
    public int Z { get; }

    [JavaMethod("{target}.up()")]
    public McBlockPos Up() => null!;

    [JavaMethod("{target}.down()")]
    public McBlockPos Down() => null!;

    [JavaMethod("{target}.north()")]
    public McBlockPos North() => null!;

    [JavaMethod("{target}.south()")]
    public McBlockPos South() => null!;

    [JavaMethod("{target}.east()")]
    public McBlockPos East() => null!;

    [JavaMethod("{target}.west()")]
    public McBlockPos West() => null!;

    [JavaMethod("{target}.add({0},{1},{2})")]
    public McBlockPos Add(int x, int y, int z) => null!;

    [JavaMethod("{target}.getSquaredDistance(new Vec3d({0},{1},{2}))")]
    public double Distance(double x, double y, double z) => 0;
}
