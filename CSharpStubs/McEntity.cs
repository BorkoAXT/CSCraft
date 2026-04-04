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

    [JavaMethod("{target}.getYaw()")]
    public float Yaw { get; }

    [JavaMethod("{target}.getPitch()")]
    public float Pitch { get; }

    [JavaMethod("{target}.isAlive()")]
    public bool IsAlive { get; }

    [JavaMethod("{target}.isOnGround()")]
    public bool IsOnGround { get; }

    [JavaMethod("{target}.isOnFire()")]
    public bool IsOnFire { get; }

    [JavaMethod("{target}.isInvisible()")]
    public bool IsInvisible { get; }

    [JavaMethod("{target}.isSwimming()")]
    public bool IsSwimming { get; }

    [JavaMethod("({target} instanceof LivingEntity _lge && _lge.isGliding())")]
    public bool IsGliding { get; }

    /// <summary>Health of the entity (0 for non-living entities).</summary>
    [JavaMethod("({target} instanceof LivingEntity ? ((LivingEntity){target}).getHealth() : 0f)")]
    public float Health { get; }

    /// <summary>Max health of the entity (0 for non-living entities).</summary>
    [JavaMethod("({target} instanceof LivingEntity ? ((LivingEntity){target}).getMaxHealth() : 0f)")]
    public float MaxHealth { get; }

    [JavaMethod("{target}.getWorld()")]
    public McWorld World { get; } = null!;

    [JavaMethod("{target}.kill()")]
    public void Kill() { }

    [JavaMethod("{target}.discard()")]
    public void Remove() { }

    [JavaMethod("{target}.setOnFireFor({0})")]
    public void SetOnFire(int seconds) { }

    [JavaMethod("{target}.setInvisible({0})")]
    public void SetInvisible(bool invisible) { }

    [JavaMethod("{target}.hasCustomName() ? {target}.getCustomName().getString() : null")]
    public string? CustomName { get; } = null;

    [JavaMethod("{target}.setCustomName(Text.literal({0}))")]
    public void SetCustomName(string name) { }

    [JavaMethod("{target}.setCustomNameVisible({0})")]
    public void SetCustomNameVisible(bool visible) { }

    [JavaMethod("{target}.getPassengerList()")]
    public List<McEntity> GetPassengers() => null!;

    [JavaMethod("{target}.getVehicle()")]
    public McEntity? GetVehicle() => null;

    [JavaMethod("{target}.startRiding({0}, true)")]
    public void StartRiding(McEntity vehicle) { }

    [JavaMethod("{target}.stopRiding()")]
    public void StopRiding() { }

    [JavaMethod("{target}.getCommandTags().contains({0})")]
    public bool HasTag(string tag) => false;

    [JavaMethod("{target}.addCommandTag({0})")]
    public void AddTag(string tag) { }

    [JavaMethod("{target}.removeScoreboardTag({0})")]
    public void RemoveTag(string tag) { }

    [JavaMethod("if ({target}.getWorld() instanceof ServerWorld _sw) { _sw.teleportTo(null, {0}, {1}, {2}, java.util.Set.of(), 0f, 0f); }")]
    public void Teleport(double x, double y, double z) { }

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

    // ── Age / lifecycle ───────────────────────────────────────────────────────

    /// <summary>Age in ticks. Negative = baby for mobs that support it.</summary>
    [JavaMethod("({target} instanceof net.minecraft.entity.passive.PassiveEntity _pe ? _pe.getBreedingAge() : 0)")]
    public int GetAge() => 0;

    [JavaMethod("if ({target} instanceof net.minecraft.entity.passive.PassiveEntity _pe) _pe.setBreedingAge({0})")]
    public void SetAge(int ticks) { }

    [JavaMethod("({target} instanceof net.minecraft.entity.passive.PassiveEntity _pe2 && _pe2.isBaby())")]
    public bool IsBaby() => false;

    [JavaMethod("if ({target} instanceof net.minecraft.entity.passive.PassiveEntity _pe3) _pe3.setBreedingAge({0} ? -24000 : 0)")]
    public void SetBaby(bool baby) { }

    // ── Gravity ───────────────────────────────────────────────────────────────

    [JavaMethod("{target}.hasNoGravity()")]
    public bool HasNoGravity { get; }

    [JavaMethod("{target}.setNoGravity({0})")]
    public void SetNoGravity(bool noGravity) { }

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
