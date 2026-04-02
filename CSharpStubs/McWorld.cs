namespace CSCraft;

/// <summary>
/// Facade for a Minecraft server world (ServerWorld).
/// Transpiles to Java's ServerWorld.
/// </summary>
[JavaClass("net.minecraft.server.world.ServerWorld")]
public class McWorld
{
    // ── World state ───────────────────────────────────────────────────────────

    [JavaMethod("{target}.getTime()")]
    public long Time { get; }

    [JavaMethod("{target}.isDay()")]
    public bool IsDay { get; }

    [JavaMethod("!{target}.isDay()")]
    public bool IsNight { get; }

    [JavaMethod("{target}.isRaining()")]
    public bool IsRaining { get; }

    [JavaMethod("{target}.isThundering()")]
    public bool IsThundering { get; }

    [JavaMethod("{target}.getDifficulty().getName()")]
    public string Difficulty { get; } = null!;

    [JavaMethod("{target}.getSpawnPos()")]
    public McBlockPos SpawnPos { get; } = null!;

    // ── Time ──────────────────────────────────────────────────────────────────

    [JavaMethod("{target}.setTimeOfDay({0})")]
    public void SetTime(long time) { }

    // ── Blocks ────────────────────────────────────────────────────────────────

    [JavaMethod("{target}.setBlockState(new BlockPos({0},{1},{2}), Registries.BLOCK.get(new Identifier({3})).getDefaultState())")]
    public void SetBlock(int x, int y, int z, string blockId) { }

    [JavaMethod("Registries.BLOCK.getId({target}.getBlockState(new BlockPos({0},{1},{2})).getBlock()).toString()")]
    public string GetBlock(int x, int y, int z) => null!;

    [JavaMethod("{target}.breakBlock(new BlockPos({0},{1},{2}), true)")]
    public void BreakBlock(int x, int y, int z) { }

    [JavaMethod("{target}.getLightLevel(new BlockPos({0},{1},{2}))")]
    public int GetLightLevel(int x, int y, int z) => 0;

    [JavaMethod("{target}.getBiome(new BlockPos({0},{1},{2})).getKey().map(k -> k.getValue().toString()).orElse(\"unknown\")")]
    public string GetBiome(int x, int y, int z) => null!;

    // ── Entities ──────────────────────────────────────────────────────────────

    [JavaMethod("{target}.getPlayers()")]
    public List<McPlayer> GetNearbyPlayers() => null!;

    [JavaMethod("{target}.getEntitiesByType(TypeFilter.instanceOf(Entity.class), e -> true)")]
    public List<McEntity> GetEntities() => null!;

    // ── Explosions ────────────────────────────────────────────────────────────

    [JavaMethod("{target}.createExplosion(null, {0}, {1}, {2}, {3}, World.ExplosionSourceType.NONE)")]
    public void CreateExplosion(double x, double y, double z, float power) { }

    // ── Weather ───────────────────────────────────────────────────────────────

    [JavaMethod("{target}.setWeather({0}, {1}, {2}, {3})")]
    public void SetWeather(int clearDuration, int rainDuration, bool raining, bool thundering) { }
}
