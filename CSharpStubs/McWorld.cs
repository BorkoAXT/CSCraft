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

    [JavaMethod("{target}.setBlockState(new BlockPos({0},{1},{2}), Registries.BLOCK.get(Identifier.of({3})).getDefaultState())")]
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

    /// <summary>Get all entities within radius of a position.</summary>
    [JavaMethod("{target}.getEntitiesByType(TypeFilter.instanceOf(Entity.class), new Box({0}-{3},{1}-{3},{2}-{3},{0}+{3},{1}+{3},{2}+{3}), e -> true)")]
    public List<McEntity> GetNearbyEntities(double x, double y, double z, double radius) => null!;

    /// <summary>Spawn an entity at a position. typeId example: "minecraft:zombie"</summary>
    [JavaMethod("{ var _et = net.minecraft.entity.EntityType.get({0}).orElse(null); if (_et != null) { var _ent = _et.create({target}); if (_ent != null) { _ent.setPosition({1},{2},{3}); {target}.spawnEntity(_ent); } } }")]
    public McEntity SpawnEntity(string typeId, double x, double y, double z) => null!;

    // ── Explosions ────────────────────────────────────────────────────────────

    [JavaMethod("{target}.createExplosion(null, {0}, {1}, {2}, {3}, World.ExplosionSourceType.NONE)")]
    public void CreateExplosion(double x, double y, double z, float power) { }

    // ── Weather ───────────────────────────────────────────────────────────────

    [JavaMethod("{target}.setWeather({0}, {1}, {2}, {3})")]
    public void SetWeather(int clearDuration, int rainDuration, bool raining, bool thundering) { }

    // ── Particles ─────────────────────────────────────────────────────────────

    /// <summary>Spawn particles at a world position. particleId: e.g. McParticles.Flame</summary>
    [JavaMethod("{target}.spawnParticles(Registries.PARTICLE_TYPE.get(Identifier.of({0})), {1}, {2}, {3}, {4}, 0, 0, 0, 0)")]
    public void SpawnParticle(string particleId, double x, double y, double z, int count = 1) { }

    // ── Sounds ────────────────────────────────────────────────────────────────

    /// <summary>Play a sound at a position in the world.</summary>
    [JavaMethod("{target}.playSound(null, new BlockPos((int){1}, (int){2}, (int){3}), Registries.SOUND_EVENT.get(Identifier.of({0})), SoundCategory.BLOCKS, 1.0f, 1.0f)")]
    public void PlaySound(string soundId, double x, double y, double z) { }

    // ── Block info ────────────────────────────────────────────────────────────

    /// <summary>Get the block state at a position.</summary>
    [JavaMethod("{target}.getBlockState(new BlockPos({0},{1},{2}))")]
    public McBlockState GetBlockState(int x, int y, int z) => null!;

    /// <summary>Check whether a position is within the world border.</summary>
    [JavaMethod("{target}.getWorldBorder().contains(new BlockPos({0},{1},{2}))")]
    public bool IsInBorder(int x, int y, int z) => true;

    /// <summary>Get the highest Y at an (x, z) using world surface type.</summary>
    [JavaMethod("{target}.getTopY(Heightmap.Type.WORLD_SURFACE, {0}, {1})")]
    public int GetTopY(int x, int z) => 0;

    /// <summary>Fill a rectangular region with a block. Same as the /fill command.</summary>
    [JavaMethod("BlockPos.stream(new BlockPos({0},{1},{2}), new BlockPos({3},{4},{5})).forEach(p -> {target}.setBlockState(p, Registries.BLOCK.get(Identifier.of({6})).getDefaultState()))")]
    public void FillBlocks(int x1, int y1, int z1, int x2, int y2, int z2, string blockId) { }

    /// <summary>Get a seeded random integer between min and max (inclusive).</summary>
    [JavaMethod("{target}.getRandom().nextBetween({0}, {1})")]
    public int GetRandomInt(int min, int max) => 0;

    // ── Item drops ────────────────────────────────────────────────────────────

    /// <summary>Drop an item stack at a world position.</summary>
    [JavaMethod("{target}.spawnEntity(new net.minecraft.entity.ItemEntity({target}, {1}, {2}, {3}, new ItemStack(Registries.ITEM.get(Identifier.of({0})), {4})))")]
    public void DropItem(string itemId, double x, double y, double z, int count = 1) { }

    // ── Lightning ─────────────────────────────────────────────────────────────

    [JavaMethod("{ var _bolt = net.minecraft.entity.EntityType.LIGHTNING_BOLT.create({target}); if (_bolt != null) { _bolt.setPosition({0},{1},{2}); {target}.spawnEntity(_bolt); } }")]
    public void SpawnLightning(double x, double y, double z) { }

    // ── Block entities ────────────────────────────────────────────────────────

    /// <summary>Get the block entity (tile entity) at a position, or null if none.</summary>
    [JavaMethod("{target}.getBlockEntity(new BlockPos({0},{1},{2}))")]
    public McBlockEntity? GetBlockEntity(int x, int y, int z) => null;

    // ── Chunk ─────────────────────────────────────────────────────────────────

    [JavaMethod("{target}.isChunkLoaded(new BlockPos({0}, 64, {1}))")]
    public bool IsChunkLoaded(int x, int z) => false;

    [JavaMethod("{target}.getBlockState(new BlockPos({0},{1},{2})).isAir()")]
    public bool IsAir(int x, int y, int z) => false;

    // ── World border ──────────────────────────────────────────────────────────

    [JavaMethod("{target}.getWorldBorder().getSize()")]
    public double GetBorderSize() => 0;

    [JavaMethod("{target}.getWorldBorder().setSize({0})")]
    public void SetBorderSize(double size) { }

    /// <summary>Shrink or expand the border to targetSize over durationSeconds.</summary>
    [JavaMethod("{target}.getWorldBorder().interpolateSize({target}.getWorldBorder().getSize(), {0}, (long)({1} * 1000L))")]
    public void AnimateBorderSize(double targetSize, double durationSeconds) { }

    [JavaMethod("{target}.getWorldBorder().getCenterX()")]
    public double GetBorderCenterX() => 0;

    [JavaMethod("{target}.getWorldBorder().getCenterZ()")]
    public double GetBorderCenterZ() => 0;

    [JavaMethod("{target}.getWorldBorder().setCenter({0}, {1})")]
    public void SetBorderCenter(double x, double z) { }

    [JavaMethod("{target}.getWorldBorder().setWarningBlocks({0})")]
    public void SetBorderWarningDistance(int blocks) { }

    // ── Dimension ─────────────────────────────────────────────────────────────

    /// <summary>Get the dimension key string (e.g. "minecraft:overworld").</summary>
    [JavaMethod("{target}.getRegistryKey().getValue().toString()")]
    public string Dimension { get; } = null!;

    // ── Server reference ──────────────────────────────────────────────────────

    [JavaMethod("{target}.getServer()")]
    public McServer Server { get; } = null!;
}
