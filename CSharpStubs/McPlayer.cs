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

    [JavaMethod("{target}.getYaw()")]
    public float Yaw { get; }

    [JavaMethod("{target}.getPitch()")]
    public float Pitch { get; }

    [JavaMethod("{target}.isSneaking()")]
    public bool IsSneaking { get; }

    [JavaMethod("{target}.isSprinting()")]
    public bool IsSprinting { get; }

    [JavaMethod("{target}.isOnGround()")]
    public bool IsOnGround { get; }

    [JavaMethod("{target}.isSwimming()")]
    public bool IsSwimming { get; }

    [JavaMethod("{target}.isGliding()")]
    public bool IsGliding { get; }

    [JavaMethod("{target}.isCreative()")]
    public bool IsCreative { get; }

    [JavaMethod("{target}.getAbilities().flying")]
    public bool IsFlying { get; }

    [JavaMethod("{target}.interactionManager.getGameMode().getName()")]
    public string GameMode { get; } = null!;

    [JavaMethod("{target}.experienceLevel")]
    public int XpLevel { get; set; }

    [JavaMethod("{target}.getInventory()")]
    public object Inventory { get; } = null!;

    [JavaMethod("{target}.getMainHandStack()")]
    public McItemStack MainHandItem { get; } = null!;

    [JavaMethod("{target}.getOffHandStack()")]
    public McItemStack OffHandItem { get; } = null!;

    [JavaMethod("{target}.getEquippedStack(net.minecraft.entity.EquipmentSlot.HEAD)")]
    public McItemStack Helmet { get; } = null!;

    [JavaMethod("{target}.getEquippedStack(net.minecraft.entity.EquipmentSlot.CHEST)")]
    public McItemStack Chestplate { get; } = null!;

    [JavaMethod("{target}.getEquippedStack(net.minecraft.entity.EquipmentSlot.LEGS)")]
    public McItemStack Leggings { get; } = null!;

    [JavaMethod("{target}.getEquippedStack(net.minecraft.entity.EquipmentSlot.FEET)")]
    public McItemStack Boots { get; } = null!;

    [JavaMethod("{target}.equipStack(net.minecraft.entity.EquipmentSlot.HEAD, {0})")]
    public void SetHelmet(McItemStack item) { }

    [JavaMethod("{target}.equipStack(net.minecraft.entity.EquipmentSlot.CHEST, {0})")]
    public void SetChestplate(McItemStack item) { }

    [JavaMethod("{target}.equipStack(net.minecraft.entity.EquipmentSlot.LEGS, {0})")]
    public void SetLeggings(McItemStack item) { }

    [JavaMethod("{target}.equipStack(net.minecraft.entity.EquipmentSlot.FEET, {0})")]
    public void SetBoots(McItemStack item) { }

    [JavaMethod("new java.util.ArrayList<>({target}.getStatusEffects())")]
    public List<McStatusEffectInstance> GetActiveEffects() => null!;

    // ── Chat & messages ───────────────────────────────────────────────────────

    [JavaMethod("{target}.sendMessage(Text.literal({0}))")]
    public void SendMessage(string message) { }

    [JavaMethod("{target}.sendMessage(Text.literal({0}), true)")]
    public void SendActionBar(string message) { }

    [JavaMethod("{target}.networkHandler.sendPacket(new TitleS2CPacket(Text.literal({0}))) ; {target}.networkHandler.sendPacket(new SubtitleS2CPacket(Text.literal({1})))")]
    public void SendTitle(string title, string subtitle = "") { }

    // ── Health ────────────────────────────────────────────────────────────────

    [JavaMethod("{target}.heal({0})")]
    public void Heal(float amount) { }

    [JavaMethod("{target}.damage(server.getDamageSources().generic(), {0})")]
    public void Damage(float amount) { }

    // ── Movement ──────────────────────────────────────────────────────────────

    [JavaMethod("{target}.teleport((ServerWorld)server.getWorld(World.OVERWORLD), {0}, {1}, {2}, 0f, 0f)")]
    public void Teleport(double x, double y, double z) { }

    [JavaMethod("{target}.lookAt(EntityAnchor.EYES, new Vec3d({0},{1},{2}))")]
    public void LookAt(double x, double y, double z) { }

    // ── Items ─────────────────────────────────────────────────────────────────

    [JavaMethod("{target}.getInventory().insertStack(new ItemStack(Registries.ITEM.get(Identifier.of({0})), {1}))")]
    public void GiveItem(string itemId, int count = 1) { }

    [JavaMethod("{target}.getInventory().clear()")]
    public void ClearInventory() { }

    // ── Effects ───────────────────────────────────────────────────────────────

    [JavaMethod("{target}.addStatusEffect(new StatusEffectInstance(Registries.STATUS_EFFECT.get(Identifier.of({0})), {1}, {2}))")]
    public void GiveEffect(string effectId, int duration, int amplifier = 0) { }

    [JavaMethod("{target}.removeStatusEffect(Registries.STATUS_EFFECT.get(Identifier.of({0})))")]
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

    // ── Shield / item use ─────────────────────────────────────────────────────

    [JavaMethod("{target}.isBlocking()")]
    public bool IsBlocking { get; }

    [JavaMethod("{target}.isUsingItem()")]
    public bool IsUsingItem { get; }

    [JavaMethod("{target}.getActiveItem()")]
    public McItemStack GetActiveItem() => null!;

    [JavaMethod("{target}.getItemUseTimeLeft()")]
    public int GetItemUseTimeLeft() => 0;

    // ── Cooldowns ─────────────────────────────────────────────────────────────

    [JavaMethod("{target}.getItemCooldownManager().isCoolingDown(Registries.ITEM.get(Identifier.of({0})))")]
    public bool IsOnCooldown(string itemId) => false;

    [JavaMethod("{target}.getItemCooldownManager().set(Registries.ITEM.get(Identifier.of({0})), {1})")]
    public void SetCooldown(string itemId, int ticks) { }

    // ── Connection / locale ───────────────────────────────────────────────────

    [JavaMethod("{target}.networkHandler.getLatency()")]
    public int GetPing() => 0;

    [JavaMethod("{target}.networkHandler.getConnectionAddress().toString()")]
    public string GetIp() => null!;

    [JavaMethod("{target}.clientSettingsPacket != null ? {target}.clientSettingsPacket.language() : \"en_us\"")]
    public string GetLocale() => null!;

    // ── Sleep ─────────────────────────────────────────────────────────────────

    [JavaMethod("{target}.isSleeping()")]
    public bool IsSleeping { get; }

    [JavaMethod("{target}.isSpectator()")]
    public bool IsSpectator { get; }

    [JavaMethod("{target}.wakeUp(false, false)")]
    public void WakeUp() { }

    // ── UI ────────────────────────────────────────────────────────────────────

    [JavaMethod("{target}.networkHandler.sendPacket(new net.minecraft.network.packet.s2c.play.OpenScreenS2CPacket(0, net.minecraft.screen.ScreenHandlerType.GENERIC_9X3, Text.literal(\"\"), 3))")]
    public void OpenInventory() { }

    [JavaMethod("{target}.closeHandledScreen()")]
    public void CloseInventory() { }

    /// <summary>Show an advancement-style toast notification.</summary>
    [JavaMethod("{target}.networkHandler.sendPacket(new net.minecraft.network.packet.s2c.play.AdvancementUpdateS2CPacket(false, java.util.List.of(), java.util.Set.of(), java.util.Map.of()))")]
    public void SendToast(string title, string description) { }

    /// <summary>Send a resource pack to the player.</summary>
    [JavaMethod("{target}.sendResourcePackUrl({0}, \"\", false, null)")]
    public void SendResourcePack(string url) { }

    // ── Ender chest ───────────────────────────────────────────────────────────

    [JavaMethod("{target}.getEnderChestInventory()")]
    public McInventory GetEnderChest() => null!;

    // ── Kick / permissions ────────────────────────────────────────────────────

    [JavaMethod("{target}.networkHandler.disconnect(Text.literal({0}))")]
    public void Kick(string reason) { }

    [JavaMethod("{target}.hasPermissionLevel(2)")]
    public bool IsOp() => false;

    [JavaMethod("{target}.hasPermissionLevel({0})")]
    public bool HasPermissionLevel(int level) => false;

    [JavaMethod("{target}.hasStatusEffect(Registries.STATUS_EFFECT.get(Identifier.of({0})))")]
    public bool HasEffect(string effectId) => false;

    // ── Misc ──────────────────────────────────────────────────────────────────

    [JavaMethod("{target}.playSoundToPlayer(Registries.SOUND_EVENT.get(Identifier.of({0})), SoundCategory.PLAYERS, 1.0f, 1.0f)")]
    public void PlaySound(string soundId) { }

    [JavaMethod("{target}.setSpawnPoint(World.OVERWORLD, new BlockPos({0},{1},{2}), 0f, true, false)")]
    public void SetSpawn(int x, int y, int z) { }

    // ── Particles ─────────────────────────────────────────────────────────────

    /// <summary>Spawn particles visible only to this player. particleId: e.g. McParticles.Flame</summary>
    [JavaMethod("{target}.networkHandler.sendPacket(new ParticleS2CPacket(ParticleTypes.byId(Registry.PARTICLE_TYPE.getRawId(Registries.PARTICLE_TYPE.get(Identifier.of({0})))), true, {1}, {2}, {3}, 0, 0, 0, 0, {4}))")]
    public void SpawnParticle(string particleId, double x, double y, double z, int count = 1) { }

    // ── Biome & dimension ─────────────────────────────────────────────────────

    /// <summary>Get the biome ID at the player's current position.</summary>
    [JavaMethod("{target}.getWorld().getBiome({target}.getBlockPos()).getKey().map(k -> k.getValue().toString()).orElse(\"unknown\")")]
    public string GetBiome() => null!;

    /// <summary>Get the dimension key (e.g. "minecraft:overworld").</summary>
    [JavaMethod("{target}.getWorld().getRegistryKey().getValue().toString()")]
    public string GetDimension() => null!;

    // ── NBT / persistent data ─────────────────────────────────────────────────

    /// <summary>Get a string value stored in the player's NBT data.</summary>
    [JavaMethod("{target}.getCustomData().getString({0})")]
    public string GetNbtString(string key) => null!;

    /// <summary>Set a string value in the player's NBT data.</summary>
    [JavaMethod("{target}.getCustomData().putString({0}, {1})")]
    public void SetNbtString(string key, string value) { }

    /// <summary>Get an integer value stored in the player's NBT data.</summary>
    [JavaMethod("{target}.getCustomData().getInt({0})")]
    public int GetNbtInt(string key) => 0;

    /// <summary>Set an integer value in the player's NBT data.</summary>
    [JavaMethod("{target}.getCustomData().putInt({0}, {1})")]
    public void SetNbtInt(string key, int value) { }

    /// <summary>Check whether a key exists in the player's NBT data.</summary>
    [JavaMethod("{target}.getCustomData().contains({0})")]
    public bool HasNbt(string key) => false;

    // ── Server reference ──────────────────────────────────────────────────────

    [JavaMethod("{target}.getServer()")]
    public McServer Server { get; } = null!;
}
