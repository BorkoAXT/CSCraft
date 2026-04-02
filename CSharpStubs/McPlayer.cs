namespace CSCraft;

/// <summary>
/// Facade for a Minecraft server player (ServerPlayerEntity).
/// Transpiles to Java's ServerPlayerEntity.
/// </summary>
[JavaClass("net.minecraft.server.network.ServerPlayerEntity")]
public class McPlayer
{
    // ── Identity ──────────────────────────────────────────────────────────────

    [JavaMethod("{target}.getName().getString()")]
    public string Name { get; } = null!;

    [JavaMethod("{target}.getUuidAsString()")]
    public string Uuid { get; } = null!;

    // ── Health & food ─────────────────────────────────────────────────────────

    [JavaMethod("{target}.getHealth()")]
    public float Health { get; set; }

    [JavaMethod("{target}.getMaxHealth()")]
    public float MaxHealth { get; }

    [JavaMethod("{target}.getHungerManager().getFoodLevel()")]
    public int FoodLevel { get; set; }

    [JavaMethod("{target}.isAlive()")]
    public bool IsAlive { get; }

    // ── Position ──────────────────────────────────────────────────────────────

    [JavaMethod("{target}.getX()")]
    public double X { get; }

    [JavaMethod("{target}.getY()")]
    public double Y { get; }

    [JavaMethod("{target}.getZ()")]
    public double Z { get; }

    [JavaMethod("{target}.getBlockPos()")]
    public McBlockPos BlockPos { get; } = null!;

    [JavaMethod("(ServerWorld){target}.getWorld()")]
    public McWorld World { get; } = null!;

    // ── State ─────────────────────────────────────────────────────────────────

    [JavaMethod("{target}.isSneaking()")]
    public bool IsSneaking { get; }

    [JavaMethod("{target}.isSprinting()")]
    public bool IsSprinting { get; }

    [JavaMethod("{target}.isOnGround()")]
    public bool IsOnGround { get; }

    [JavaMethod("{target}.isCreative()")]
    public bool IsCreative { get; }

    [JavaMethod("{target}.interactionManager.getGameMode().getName()")]
    public string GameMode { get; } = null!;

    [JavaMethod("{target}.experienceLevel")]
    public int XpLevel { get; set; }

    [JavaMethod("{target}.getInventory()")]
    public object Inventory { get; } = null!;

    // ── Chat & messages ───────────────────────────────────────────────────────

    [JavaMethod("{target}.sendMessage(Text.literal({0}))")]
    public void SendMessage(string message) { }

    [JavaMethod("{target}.sendMessage(Text.literal({0}), true)")]
    public void SendActionBar(string message) { }

    // ── Health ────────────────────────────────────────────────────────────────

    [JavaMethod("{target}.heal({0})")]
    public void Heal(float amount) { }

    [JavaMethod("{target}.damage(server.getDamageSources().generic(), {0})")]
    public void Damage(float amount) { }

    // ── Movement ──────────────────────────────────────────────────────────────

    [JavaMethod("{target}.teleport((ServerWorld)server.getWorld(World.OVERWORLD), {0}, {1}, {2}, 0f, 0f)")]
    public void Teleport(double x, double y, double z) { }

    // ── Items ─────────────────────────────────────────────────────────────────

    [JavaMethod("{target}.getInventory().insertStack(new ItemStack(Registries.ITEM.get(new Identifier({0})), {1}))")]
    public void GiveItem(string itemId, int count = 1) { }

    [JavaMethod("{target}.getInventory().clear()")]
    public void ClearInventory() { }

    // ── Effects ───────────────────────────────────────────────────────────────

    [JavaMethod("{target}.addStatusEffect(new StatusEffectInstance(Registries.STATUS_EFFECT.get(new Identifier({0})), {1}, {2}))")]
    public void GiveEffect(string effectId, int duration, int amplifier = 0) { }

    [JavaMethod("{target}.removeStatusEffect(Registries.STATUS_EFFECT.get(new Identifier({0})))")]
    public void RemoveEffect(string effectId) { }

    [JavaMethod("{target}.clearStatusEffects()")]
    public void ClearEffects() { }

    // ── XP ────────────────────────────────────────────────────────────────────

    [JavaMethod("{target}.addExperience({0})")]
    public void GiveXp(int amount) { }

    [JavaMethod("{target}.setExperienceLevel({0})")]
    public void SetXpLevel(int level) { }

    // ── Game mode ─────────────────────────────────────────────────────────────

    [JavaMethod("{target}.changeGameMode(GameMode.byName({0}))")]
    public void SetGameMode(string mode) { }

    // ── Kick / permissions ────────────────────────────────────────────────────

    [JavaMethod("{target}.networkHandler.disconnect(Text.literal({0}))")]
    public void Kick(string reason) { }

    [JavaMethod("{target}.hasPermissionLevel(2)")]
    public bool IsOp() => false;

    // ── Misc ──────────────────────────────────────────────────────────────────

    [JavaMethod("{target}.playSoundToPlayer(Registries.SOUND_EVENT.get(new Identifier({0})), SoundCategory.PLAYERS, 1.0f, 1.0f)")]
    public void PlaySound(string soundId) { }

    [JavaMethod("{target}.setSpawnPoint(World.OVERWORLD, new BlockPos({0},{1},{2}), 0f, true, false)")]
    public void SetSpawn(int x, int y, int z) { }
}
