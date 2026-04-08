namespace Transpiler;

/// <summary>
/// Maps C# method/property calls on Minecraft facade types to their
/// Java equivalents.
///
/// Format strings use positional placeholders:
///   {target} = the object the method is called on  e.g. "player"
///   {0},{1}.. = method arguments in order
///
/// Examples:
///   player.SendMessage("hi")  →  player.sendMessage(Text.literal("hi"))
///   player.Name               →  player.getName().getString()
/// </summary>
public static class MethodMapper
{
    // ── McPlayer (ServerPlayerEntity) ─────────────────────────────────────────

    private static readonly Dictionary<string, MethodMapping> PlayerMethods = new()
    {
        // Chat
        ["SendMessage"]     = new("{target}.sendMessage(Text.literal({0}))",
                                  Imports: ["net.minecraft.text.Text"]),
        ["SendActionBar"]   = new("{target}.sendMessage(Text.literal({0}), true)"),

        // Identity
        ["GetName"]         = new("{target}.getName().getString()"),
        ["GetUuid"]         = new("{target}.getUuidAsString()"),

        // Health & food
        ["GetHealth"]       = new("{target}.getHealth()"),
        ["SetHealth"]       = new("{target}.setHealth({0})"),
        ["GetMaxHealth"]    = new("{target}.getMaxHealth()"),
        ["GetFoodLevel"]    = new("{target}.getHungerManager().getFoodLevel()"),
        ["SetFoodLevel"]    = new("{target}.getHungerManager().setFoodLevel({0})"),
        ["SetHunger"]       = new("{target}.getHungerManager().setFoodLevel({0})"),
        ["SetSaturation"]   = new("{target}.getHungerManager().setSaturationLevel({0})"),
        ["Heal"]            = new("{target}.heal({0})"),
        ["Damage"]          = new("{target}.damage({target}.getServerWorld().getDamageSources().generic(), {0})"),

        // Movement & position
        ["Teleport"]        = new("{target}.teleport(((ServerWorld){target}.getWorld()), {0}, {1}, {2}, 0f, 0f)",
                                  Imports: ["net.minecraft.server.world.ServerWorld"]),
        ["GetX"]            = new("{target}.getX()"),
        ["GetY"]            = new("{target}.getY()"),
        ["GetZ"]            = new("{target}.getZ()"),
        ["GetBlockPos"]     = new("{target}.getBlockPos()"),

        // Items & inventory
        ["GiveItem"]        = new("{target}.getInventory().insertStack(new ItemStack(Registries.ITEM.get(Identifier.of({0})), {1}))",
                                  Imports: ["net.minecraft.item.ItemStack", "net.minecraft.registry.Registries", "net.minecraft.util.Identifier"]),
        ["ClearInventory"]  = new("{target}.getInventory().clear()"),

        // Effects
        ["GiveEffect"]      = new("{target}.addStatusEffect(new StatusEffectInstance(Registries.STATUS_EFFECT.getEntry(Identifier.of({0})).get(), {1}, {2}))",
                                  Imports: ["net.minecraft.entity.effect.StatusEffectInstance", "net.minecraft.registry.Registries", "net.minecraft.util.Identifier"]),
        ["AddEffect"]       = new("{target}.addStatusEffect(new StatusEffectInstance(Registries.STATUS_EFFECT.getEntry(Identifier.of({0})).get(), {1}, {2}))",
                                  Imports: ["net.minecraft.entity.effect.StatusEffectInstance", "net.minecraft.registry.Registries", "net.minecraft.util.Identifier"]),
        ["RemoveEffect"]    = new("{target}.removeStatusEffect(Registries.STATUS_EFFECT.getEntry(Identifier.of({0})).get())"),
        ["ClearEffects"]    = new("{target}.clearStatusEffects()"),

        // XP
        ["GiveXp"]          = new("{target}.addExperience({0})"),
        ["AddExperience"]   = new("{target}.addExperience({0})"),
        ["GetXpLevel"]      = new("{target}.experienceLevel"),
        ["SetXpLevel"]      = new("{target}.setExperienceLevel({0})"),

        // Game mode
        ["SetGameMode"]     = new("{target}.changeGameMode(GameMode.byName({0}))",
                                  Imports: ["net.minecraft.world.GameMode"]),
        ["GetGameMode"]     = new("{target}.interactionManager.getGameMode().getName()"),

        // Kick / permissions
        ["Kick"]            = new("{target}.networkHandler.disconnect(Text.literal({0}))"),
        ["IsOp"]            = new("{target}.hasPermissionLevel(2)"),

        // Flight
        ["SetFlying"]       = new("{ {target}.getAbilities().flying = {0}; {target}.getAbilities().allowFlying = {0}; {target}.sendAbilitiesUpdate(); }"),

        // Misc
        ["PlaySound"]       = new("{target}.playSoundToPlayer(Registries.SOUND_EVENT.get(Identifier.of({0})), SoundCategory.PLAYERS, 1.0f, 1.0f)",
                                  Imports: ["net.minecraft.sound.SoundCategory"]),
        ["SetSpawn"]        = new("{target}.setSpawnPoint(World.OVERWORLD, new BlockPos({0},{1},{2}), 0f, true, false)"),
    };

    // ── McWorld (ServerWorld) ────────────────────────────────────────────────

    private static readonly Dictionary<string, MethodMapping> WorldMethods = new()
    {
        // Blocks
        ["SetBlock"]        = new("{target}.setBlockState(new BlockPos({0},{1},{2}), Registries.BLOCK.get(Identifier.of({3})).getDefaultState())",
                                  Imports: ["net.minecraft.registry.Registries", "net.minecraft.util.Identifier", "net.minecraft.util.math.BlockPos"]),
        ["GetBlock"]        = new("Registries.BLOCK.getId({target}.getBlockState(new BlockPos({0},{1},{2})).getBlock()).toString()"),
        ["BreakBlock"]      = new("{target}.breakBlock(new BlockPos({0},{1},{2}), true)"),
        ["FillBlocks"]      = new("/* fill {0},{1},{2} to {3},{4},{5} with {6} */"), // complex — left as comment

        // Entities
        ["SpawnEntity"]         = new("{ var _et = net.minecraft.entity.EntityType.get({0}).orElse(null); if (_et != null) { var _ent = _et.create({target}); if (_ent != null) { _ent.setPosition({1},{2},{3}); {target}.spawnEntity(_ent); } } }",
                                       Imports: ["net.minecraft.entity.EntityType"]),
        ["GetEntities"]         = new("{target}.getEntitiesByType(TypeFilter.instanceOf(Entity.class), e -> true)"),
        ["GetNearbyEntities"]   = new("{target}.getEntitiesByType(TypeFilter.instanceOf(Entity.class), new net.minecraft.util.math.Box({0}-{3},{1}-{3},{2}-{3},{0}+{3},{1}+{3},{2}+{3}), e -> true)"),
        ["GetNearbyPlayers"]    = new("{target}.getPlayers()"),

        // Lightning
        ["SpawnLightning"]  = new("{ var _bolt = net.minecraft.entity.EntityType.LIGHTNING_BOLT.create({target}); if (_bolt != null) { _bolt.setPosition({0},{1},{2}); {target}.spawnEntity(_bolt); } }"),

        // Border
        ["GetBorderSize"]       = new("{target}.getWorldBorder().getSize()"),
        ["SetBorderSize"]       = new("{target}.getWorldBorder().setSize({0})"),
        ["AnimateBorderSize"]   = new("{target}.getWorldBorder().interpolateSize({target}.getWorldBorder().getSize(), {0}, (long)({1} * 1000L))"),
        ["GetBorderCenterX"]    = new("{target}.getWorldBorder().getCenterX()"),
        ["GetBorderCenterZ"]    = new("{target}.getWorldBorder().getCenterZ()"),
        ["SetBorderCenter"]     = new("{target}.getWorldBorder().setCenter({0}, {1})"),
        ["SetBorderWarningDistance"] = new("{target}.getWorldBorder().setWarningBlocks({0})"),

        // Block entities
        ["GetBlockEntity"]  = new("{target}.getBlockEntity(new BlockPos({0},{1},{2}))",
                                   Imports: ["net.minecraft.util.math.BlockPos"]),

        // Item drops
        ["DropItem"]        = new("{target}.spawnEntity(new net.minecraft.entity.ItemEntity({target}, {1}, {2}, {3}, new ItemStack(Registries.ITEM.get(Identifier.of({0})), {4})))"),

        // Checks
        ["IsAir"]           = new("{target}.getBlockState(new BlockPos({0},{1},{2})).isAir()"),
        ["IsChunkLoaded"]   = new("{target}.isChunkLoaded(new BlockPos({0}, 64, {1}))"),

        // World info
        ["GetTime"]         = new("{target}.getTime()"),
        ["SetTime"]         = new("{target}.setTimeOfDay({0})"),
        ["IsRaining"]       = new("{target}.isRaining()"),
        ["SetWeather"]      = new("{target}.setWeather({0}, {1}, {2}, {3})"),
        ["GetDifficulty"]   = new("{target}.getDifficulty().getName()"),

        // Explosions
        ["CreateExplosion"] = new("{target}.createExplosion(null, {0}, {1}, {2}, {3}, World.ExplosionSourceType.NONE)"),

        // Fill & random
        ["FillBlocks"]      = new("BlockPos.stream(new BlockPos({0},{1},{2}), new BlockPos({3},{4},{5})).forEach(_bp -> {target}.setBlockState(_bp, Registries.BLOCK.get(Identifier.of({6})).getDefaultState()))",
                                   Imports: ["net.minecraft.util.math.BlockPos", "net.minecraft.registry.Registries", "net.minecraft.util.Identifier"]),
        ["GetRandomInt"]    = new("{target}.getRandom().nextBetween({0}, {1})"),

        // Lighting
        ["GetLightLevel"]   = new("{target}.getLightLevel(new BlockPos({0},{1},{2}))"),

        // Biome
        ["GetBiome"]        = new("{target}.getBiome(new BlockPos({0},{1},{2})).getKey().map(k -> k.getValue().toString()).orElse(\"unknown\")"),
    };

    // ── McServer (MinecraftServer) ───────────────────────────────────────────

    private static readonly Dictionary<string, MethodMapping> ServerMethods = new()
    {
        ["Broadcast"]           = new("{target}.getPlayerManager().broadcast(Text.literal({0}), false)"),
        ["GetOnlinePlayers"]    = new("{target}.getPlayerManager().getPlayerList()"),
        ["GetPlayer"]           = new("{target}.getPlayerManager().getPlayer({0})"),
        ["GetPlayerCount"]      = new("{target}.getPlayerManager().getCurrentPlayerCount()"),
        ["GetMaxPlayers"]       = new("{target}.getPlayerManager().getMaxPlayerCount()"),
        ["RunCommand"]          = new("{target}.getCommandManager().executeWithPrefix({target}.getCommandSource(), {0})"),
        ["GetTps"]              = new("{target}.getAverageTickTime()"),
        ["GetTicks"]            = new("{target}.getTicks()"),
        ["IsRunning"]           = new("{target}.isRunning()"),
        ["GetVersion"]          = new("{target}.getVersion()"),
        ["Shutdown"]            = new("{target}.stop(false)"),
        ["GetPlayerByUuid"]     = new("{target}.getPlayerManager().getPlayer(java.util.UUID.fromString({0}))"),
        ["GetAllWorlds"]        = new("java.util.stream.StreamSupport.stream({target}.getWorlds().spliterator(), false).toList()"),
        ["GetSeed"]             = new("{target}.getOverworld().getSeed()"),
        ["GetDefaultGameMode"]  = new("{target}.getDefaultGameMode().getName()"),
        ["SetDefaultGameMode"]  = new("{target}.setDefaultGameMode(net.minecraft.world.GameMode.byName({0}))"),
        ["IsHardcore"]          = new("{target}.isHardcore()"),
    };

    // ── BlockPos ─────────────────────────────────────────────────────────────

    private static readonly Dictionary<string, MethodMapping> BlockPosMethods = new()
    {
        ["GetX"]    = new("{target}.getX()"),
        ["GetY"]    = new("{target}.getY()"),
        ["GetZ"]    = new("{target}.getZ()"),
        ["Up"]      = new("{target}.up()"),
        ["Down"]    = new("{target}.down()"),
        ["North"]   = new("{target}.north()"),
        ["South"]   = new("{target}.south()"),
        ["East"]    = new("{target}.east()"),
        ["West"]    = new("{target}.west()"),
        ["Add"]     = new("{target}.add({0},{1},{2})"),
        ["Distance"]= new("{target}.getSquaredDistance(new Vec3d({0},{1},{2}))"),
    };

    // ── ItemStack ─────────────────────────────────────────────────────────────

    private static readonly Dictionary<string, MethodMapping> ItemStackMethods = new()
    {
        ["GetCount"]        = new("{target}.getCount()"),
        ["SetCount"]        = new("{target}.setCount({0})"),
        ["GetItem"]         = new("Registries.ITEM.getId({target}.getItem()).toString()"),
        ["IsEmpty"]         = new("{target}.isEmpty()"),
        ["HasNbt"]          = new("{target}.hasNbt()"),
        ["GetNbt"]          = new("{target}.getNbt()"),
        ["Copy"]            = new("{target}.copy()"),
        ["IsOf"]            = new("Registries.ITEM.getId({target}.getItem()).toString().equals({0})"),
        ["GetCustomName"]   = new("{target}.getName().getString()"),
        ["SetCustomName"]   = new("{target}.setCustomName(Text.literal({0}))",
                                   Imports: ["net.minecraft.text.Text"]),
        ["GetDamage"]       = new("{target}.getDamage()"),
        ["SetDamage"]       = new("{target}.setDamage({0})"),
        ["AddEnchantment"]  = new("{ var _aeReg = server.getRegistryManager().get(net.minecraft.registry.RegistryKeys.ENCHANTMENT); var _aeKey = net.minecraft.registry.RegistryKey.of(net.minecraft.registry.RegistryKeys.ENCHANTMENT, net.minecraft.util.Identifier.of({0})); _aeReg.getEntry(_aeKey).ifPresent(_aeEnch -> {target}.addEnchantment(_aeEnch, {1})); }",
                                   Imports: ["net.minecraft.registry.RegistryKey", "net.minecraft.registry.RegistryKeys", "net.minecraft.util.Identifier"]),
        ["GetNbtString"]    = new("{target}.contains(net.minecraft.component.DataComponentTypes.CUSTOM_DATA) ? {target}.get(net.minecraft.component.DataComponentTypes.CUSTOM_DATA).getNbt().getString({0}) : \"\""),
        ["SetNbtString"]    = new("{ var _nbtS = {target}.contains(net.minecraft.component.DataComponentTypes.CUSTOM_DATA) ? {target}.get(net.minecraft.component.DataComponentTypes.CUSTOM_DATA).getNbt().copy() : new NbtCompound(); _nbtS.putString({0}, {1}); {target}.set(net.minecraft.component.DataComponentTypes.CUSTOM_DATA, net.minecraft.component.type.NbtComponent.of(_nbtS)); }",
                                   Imports: ["net.minecraft.nbt.NbtCompound"]),
        ["GetNbtInt"]       = new("{target}.contains(net.minecraft.component.DataComponentTypes.CUSTOM_DATA) ? {target}.get(net.minecraft.component.DataComponentTypes.CUSTOM_DATA).getNbt().getInt({0}) : 0"),
        ["SetNbtInt"]       = new("{ var _nbtI = {target}.contains(net.minecraft.component.DataComponentTypes.CUSTOM_DATA) ? {target}.get(net.minecraft.component.DataComponentTypes.CUSTOM_DATA).getNbt().copy() : new NbtCompound(); _nbtI.putInt({0}, {1}); {target}.set(net.minecraft.component.DataComponentTypes.CUSTOM_DATA, net.minecraft.component.type.NbtComponent.of(_nbtI)); }",
                                   Imports: ["net.minecraft.nbt.NbtCompound"]),
    };

    // ── McEntity (Entity / LivingEntity) ─────────────────────────────────────

    private static readonly Dictionary<string, MethodMapping> EntityMethods = new()
    {
        ["Kill"]                = new("{target}.kill()"),
        ["Remove"]              = new("{target}.discard()"),
        ["Teleport"]            = new("if ({target}.getWorld() instanceof ServerWorld _sw) { _sw.teleportTo(null, {0}, {1}, {2}, java.util.Set.of(), 0f, 0f); }"),
        ["SetOnFire"]           = new("{target}.setOnFireFor({0})"),
        ["SetInvisible"]        = new("{target}.setInvisible({0})"),
        ["SetCustomName"]       = new("{target}.setCustomName(Text.literal({0}))",
                                      Imports: ["net.minecraft.text.Text"]),
        ["SetCustomNameVisible"]= new("{target}.setCustomNameVisible({0})"),
        ["GetPassengers"]       = new("{target}.getPassengerList()"),
        ["GetVehicle"]          = new("{target}.getVehicle()"),
        ["StartRiding"]         = new("{target}.startRiding({0}, true)"),
        ["StopRiding"]          = new("{target}.stopRiding()"),
        ["HasTag"]              = new("{target}.getCommandTags().contains({0})"),
        ["AddTag"]              = new("{target}.addCommandTag({0})"),
        ["RemoveTag"]           = new("{target}.removeScoreboardTag({0})"),
        ["SetVelocity"]         = new("{target}.setVelocity({0}, {1}, {2})"),
        ["GetNbtString"]        = new("{ NbtCompound _eNbt = new NbtCompound(); {target}.writeNbt(_eNbt); _eNbt.getString({0}); }",
                                      Imports: ["net.minecraft.nbt.NbtCompound"]),
        ["SetNbtString"]        = new("/* TODO: entity.SetNbtString not supported in 1.21.1 — use command tags instead */"),
        ["GetNbtInt"]           = new("{ NbtCompound _eNbt = new NbtCompound(); {target}.writeNbt(_eNbt); _eNbt.getInt({0}); }",
                                      Imports: ["net.minecraft.nbt.NbtCompound"]),
        ["SetNbtInt"]           = new("/* TODO: entity.SetNbtInt not supported in 1.21.1 — use command tags instead */"),

        // Age / baby
        ["IsBaby"]  = new("({target} instanceof net.minecraft.entity.passive.PassiveEntity _pe2b && _pe2b.isBaby())"),
        ["GetAge"]  = new("({target} instanceof net.minecraft.entity.passive.PassiveEntity _pea ? _pea.getBreedingAge() : 0)"),
        ["SetAge"]  = new("if ({target} instanceof net.minecraft.entity.passive.PassiveEntity _pes) _pes.setBreedingAge({0})"),
        ["SetBaby"] = new("if ({target} instanceof net.minecraft.entity.passive.PassiveEntity _peb) _peb.setBreedingAge({0} ? -24000 : 0)"),
        ["HasNoGravity"] = new("{target}.hasNoGravity()"),
        ["SetNoGravity"] = new("{target}.setNoGravity({0})"),
    };

    // ── Static constructors (new XYZ(...) in C# → Java factory/constructor) ──

    public static readonly Dictionary<string, string> Constructors = new()
    {
        // new BlockPos(x, y, z) → new BlockPos(x, y, z)  (same!)
        ["BlockPos"]        = "new BlockPos({0}, {1}, {2})",
        // new ItemStack("minecraft:diamond", 1) → new ItemStack(Registries.ITEM.get(...), 1)
        ["ItemStack"]       = "new ItemStack(Registries.ITEM.get(Identifier.of({0})), {1})",
        // new McText("hello") → Text.literal("hello")
        ["McText"]          = "Text.literal({0})",
        ["ChatMessage"]     = "Text.literal({0})",
        // new McIdentifier("minecraft:stone") → Identifier.of("minecraft:stone")
        ["McIdentifier"]    = "Identifier.of({0})",
        ["ResourceLocation"]= "Identifier.of({0})",
        ["Identifier"]      = "Identifier.of({0})",
        // new McNbt() → new NbtCompound()
        ["McNbt"]           = "new NbtCompound()",
        // new McBossBar("title", color) → new ServerBossBar(Text.literal("title"), BossBar.Color.PURPLE, BossBar.Style.PROGRESS)
        ["McBossBar"]       = "new ServerBossBar(Text.literal({0}), BossBar.Color.PURPLE, BossBar.Style.PROGRESS)",
        // new McItemStack("id", count) → new ItemStack(Registries.ITEM.get(Identifier.of("id")), count)
        ["McItemStack"]     = "new ItemStack(Registries.ITEM.get(Identifier.of({0})), {1})",
    };

    // ── McPlayer extended methods ─────────────────────────────────────────────

    private static readonly Dictionary<string, MethodMapping> PlayerMethodsExtra = new()
    {
        ["GetBiome"]        = new("{target}.getWorld().getBiome({target}.getBlockPos()).getKey().map(k -> k.getValue().toString()).orElse(\"unknown\")"),
        ["GetDimension"]    = new("{target}.getWorld().getRegistryKey().getValue().toString()"),
        ["GetNbtString"]    = new("ModPlayerData.getPlayerNbt({target}).getString({0})"),
        ["SetNbtString"]    = new("{ NbtCompound _pNbt = ModPlayerData.getPlayerNbt({target}); _pNbt.putString({0}, {1}); }",
                                  Imports: ["net.minecraft.nbt.NbtCompound"]),
        ["GetNbtInt"]       = new("ModPlayerData.getPlayerNbt({target}).getInt({0})"),
        ["SetNbtInt"]       = new("{ NbtCompound _pNbt = ModPlayerData.getPlayerNbt({target}); _pNbt.putInt({0}, {1}); }",
                                  Imports: ["net.minecraft.nbt.NbtCompound"]),
        ["HasNbt"]          = new("ModPlayerData.getPlayerNbt({target}).contains({0})"),
        ["SetHelmet"]           = new("{target}.equipStack(net.minecraft.entity.EquipmentSlot.HEAD, {0})"),
        ["SetChestplate"]       = new("{target}.equipStack(net.minecraft.entity.EquipmentSlot.CHEST, {0})"),
        ["SetLeggings"]         = new("{target}.equipStack(net.minecraft.entity.EquipmentSlot.LEGS, {0})"),
        ["SetBoots"]            = new("{target}.equipStack(net.minecraft.entity.EquipmentSlot.FEET, {0})"),
        ["GetActiveEffects"]    = new("new java.util.ArrayList<>({target}.getStatusEffects())"),
        ["HasPermissionLevel"]  = new("{target}.hasPermissionLevel({0})"),
        ["HasEffect"]           = new("{target}.hasStatusEffect(Registries.STATUS_EFFECT.getEntry(Identifier.of({0})).get())"),
        ["SendTitle"]           = new("{target}.networkHandler.sendPacket(new net.minecraft.network.packet.s2c.play.TitleS2CPacket(Text.literal({0}))); {target}.networkHandler.sendPacket(new net.minecraft.network.packet.s2c.play.SubtitleS2CPacket(Text.literal({1})))"),
        ["LookAt"]              = new("{target}.lookAt(net.minecraft.command.argument.EntityAnchorArgumentType.EntityAnchor.EYES, new net.minecraft.util.math.Vec3d({0},{1},{2}))"),
        ["GetPing"]             = new("{target}.networkHandler.getLatency()"),
        ["GetIp"]               = new("{target}.networkHandler.getConnectionAddress().toString()"),
        ["Kick"]                = new("{target}.networkHandler.disconnect(Text.literal({0}))",
                                     Imports: ["net.minecraft.text.Text"]),
        ["GetEnderChest"]       = new("{target}.getEnderChestInventory()"),
    };

    // ── McWorld extended methods ──────────────────────────────────────────────

    private static readonly Dictionary<string, MethodMapping> WorldMethodsExtra = new()
    {
        ["GetBlockState"]   = new("{target}.getBlockState(new BlockPos({0},{1},{2}))",
                                  Imports: ["net.minecraft.util.math.BlockPos"]),
        ["GetTopY"]         = new("{target}.getTopY(Heightmap.Type.WORLD_SURFACE, {0}, {1})",
                                  Imports: ["net.minecraft.world.Heightmap"]),
        ["IsInBorder"]      = new("{target}.getWorldBorder().contains(new BlockPos({0},{1},{2}))"),
        ["PlaySound"]       = new("{target}.playSound(null, new BlockPos((int){1}, (int){2}, (int){3}), Registries.SOUND_EVENT.get(Identifier.of({0})), SoundCategory.BLOCKS, 1.0f, 1.0f)",
                                  Imports: ["net.minecraft.sound.SoundCategory", "net.minecraft.registry.Registries", "net.minecraft.util.Identifier"]),
        ["SpawnParticle"]   = new("{target}.spawnParticles((net.minecraft.particle.ParticleEffect)Registries.PARTICLE_TYPE.get(Identifier.of({0})), {1}, {2}, {3}, {4}, 0, 0, 0, 0)",
                                  Imports: ["net.minecraft.registry.Registries", "net.minecraft.util.Identifier"]),
    };

    // ── McServer extended methods ─────────────────────────────────────────────

    private static readonly Dictionary<string, MethodMapping> ServerMethodsExtra = new()
    {
        ["GetScore"]        = new("{target}.getScoreboard().getOrCreateScore({1}, {target}.getScoreboard().getNullableObjective({0})).getScore()"),
        ["SetScore"]        = new("{target}.getScoreboard().getOrCreateScore({1}, {target}.getScoreboard().getNullableObjective({0})).setScore({2})"),
    };

    // ── McCommandSource methods ───────────────────────────────────────────────

    private static readonly Dictionary<string, MethodMapping> CommandSourceMethods = new()
    {
        ["SendMessage"]     = new("{target}.sendFeedback(() -> Text.literal({0}), false)",
                                  Imports: ["net.minecraft.text.Text"]),
        ["SendError"]       = new("{target}.sendError(Text.literal({0}))",
                                  Imports: ["net.minecraft.text.Text"]),
        ["HasPermission"]   = new("{target}.hasPermissionLevel({0})"),
    };

    // ── McNbt (NbtCompound) ─────────────────────────────────────────────────

    private static readonly Dictionary<string, MethodMapping> NbtMethods = new()
    {
        ["GetString"]   = new("{target}.getString({0})"),
        ["SetString"]   = new("{target}.putString({0}, {1})"),
        ["HasString"]   = new("{target}.contains({0}) && {target}.getType({0}) == 8"),
        ["GetInt"]      = new("{target}.getInt({0})"),
        ["SetInt"]      = new("{target}.putInt({0}, {1})"),
        ["GetLong"]     = new("{target}.getLong({0})"),
        ["SetLong"]     = new("{target}.putLong({0}, {1})"),
        ["GetFloat"]    = new("{target}.getFloat({0})"),
        ["SetFloat"]    = new("{target}.putFloat({0}, {1})"),
        ["GetDouble"]   = new("{target}.getDouble({0})"),
        ["SetDouble"]   = new("{target}.putDouble({0}, {1})"),
        ["GetBool"]     = new("{target}.getBoolean({0})"),
        ["SetBool"]     = new("{target}.putBoolean({0}, {1})"),
        ["GetCompound"] = new("{target}.getCompound({0})"),
        ["SetCompound"] = new("{target}.put({0}, {1})"),
        ["Has"]         = new("{target}.contains({0})"),
        ["Remove"]      = new("{target}.remove({0})"),
        ["GetKeys"]     = new("new java.util.ArrayList<>({target}.getKeys())"),
    };

    // ── McBossBar (ServerBossBar) ────────────────────────────────────────────

    private static readonly Dictionary<string, MethodMapping> BossBarMethods = new()
    {
        ["SetTitle"]        = new("{target}.setName(Text.literal({0}))",
                                  Imports: ["net.minecraft.text.Text"]),
        ["SetProgress"]     = new("{target}.setPercent({0})"),
        ["SetColor"]        = new("{target}.setColor(BossBar.Color.valueOf({0}.toUpperCase()))"),
        ["SetStyle"]        = new("{target}.setStyle(BossBar.Style.valueOf({0}))"),
        ["SetVisible"]      = new("{target}.setVisible({0})"),
        ["AddPlayer"]       = new("{target}.addPlayer({0})"),
        ["RemovePlayer"]    = new("{target}.removePlayer({0})"),
        ["AddAllPlayers"]   = new("{0}.getPlayerManager().getPlayerList().forEach({target}::addPlayer)"),
        ["RemoveAllPlayers"]= new("{target}.clearPlayers()"),
        ["GetPlayers"]      = new("{target}.getPlayers()"),
        ["SetDarkenSky"]    = new("{target}.setDarkenSky({0})"),
        ["SetDragonMusic"]  = new("{target}.setDragonMusic({0})"),
        ["SetThickenFog"]   = new("{target}.setThickenFog({0})"),
    };

    // ── McInventory (Inventory) ──────────────────────────────────────────────

    private static readonly Dictionary<string, MethodMapping> InventoryMethods = new()
    {
        ["GetSlot"]     = new("{target}.getStack({0})"),
        ["SetSlot"]     = new("{target}.setStack({0}, {1})"),
        ["TakeSlot"]    = new("{target}.removeStack({0}, {1})"),
        ["ClearSlot"]   = new("{target}.removeStack({0})"),
        ["Clear"]       = new("{target}.clear()"),
        ["MarkDirty"]   = new("{target}.markDirty()"),
        ["Count"]       = new("java.util.stream.IntStream.range(0, {target}.size()).mapToObj({target}::getStack).filter(s -> !s.isEmpty() && Registries.ITEM.getId(s.getItem()).toString().equals({0})).mapToInt(net.minecraft.item.ItemStack::getCount).sum()"),
        ["Contains"]    = new("java.util.stream.IntStream.range(0, {target}.size()).anyMatch(i -> !{target}.getStack(i).isEmpty() && Registries.ITEM.getId({target}.getStack(i).getItem()).toString().equals({0}))"),
        ["FindSlot"]    = new("java.util.stream.IntStream.range(0, {target}.size()).filter(i -> !{target}.getStack(i).isEmpty() && Registries.ITEM.getId({target}.getStack(i).getItem()).toString().equals({0})).findFirst().orElse(-1)"),
    };

    // ── McBlockEntity (BlockEntity) ──────────────────────────────────────────

    private static readonly Dictionary<string, MethodMapping> BlockEntityMethods = new()
    {
        ["MarkDirty"]       = new("{target}.markDirty()"),
        ["GetNbt"]          = new("{target}.createNbt()"),
        ["SetNbt"]          = new("{target}.readNbt({0})"),
        ["GetInventory"]    = new("{target} instanceof net.minecraft.inventory.Inventory _inv ? _inv : null"),
        ["IsBurning"]       = new("{target} instanceof net.minecraft.block.entity.AbstractFurnaceBlockEntity _fbe2 && _fbe2.isBurning()"),
        ["GetFurnaceCookTime"] = new("{target} instanceof net.minecraft.block.entity.AbstractFurnaceBlockEntity _fbe ? _fbe.getPropertyDelegate().get(0) : 0"),
        ["GetSignLine"]     = new("{target} instanceof net.minecraft.block.entity.SignBlockEntity _sbe ? _sbe.getFrontText().getMessage({0}, false).getString() : \"\""),
    };

    // ── McGameRule<T> ─────────────────────────────────────────────────────────

    private static readonly Dictionary<string, MethodMapping> GameRuleMethods = new()
    {
        // GetValue(server) — returns the raw BooleanRule or IntRule object
        // For bool: call .get() on BooleanRule; for int: call .getValue() on IntRule
        // The emitted code lets Java infer via var — user should assign to var or specific type
        ["GetValue"] = new("{1}.getGameRules().get({target})"),
        ["SetValue"] = new("{1}.getGameRules().get({target}).set({2}, {1})"),
    };

    // ── Static method calls (Math.X, Console.X, McRegistry.X, etc.) ──────────

    public static readonly Dictionary<string, string> StaticMethods = new()
    {
        // Console → Logger
        ["Console.WriteLine"]   = "LOGGER.info({0})",
        ["Console.Write"]       = "LOGGER.info({0})",
        ["Console.Error"]       = "LOGGER.error({0})",

        // Math
        ["Math.Abs"]            = "Math.abs({0})",
        ["Math.Floor"]          = "(int)Math.floor({0})",
        ["Math.Ceiling"]        = "(int)Math.ceil({0})",
        ["Math.Round"]          = "Math.round({0})",
        ["Math.Max"]            = "Math.max({0}, {1})",
        ["Math.Min"]            = "Math.min({0}, {1})",
        ["Math.Sqrt"]           = "Math.sqrt({0})",
        ["Math.Pow"]            = "Math.pow({0}, {1})",
        ["Math.Clamp"]          = "Math.max({2}, Math.min({1}, {0}))",

        // String helpers
        ["string.IsNullOrEmpty"]= "({0} == null || {0}.isEmpty())",
        ["string.Format"]       = "String.format({0}, {1})",
        ["string.Join"]         = "String.join({0}, {1})",
        ["Guid.NewGuid"]        = "UUID.randomUUID()",

        // ── McRegistry ──────────────────────────────────────────────────────

        // Block registration
        ["McRegistry.RegisterBlock"]    = "Registry.register(Registries.BLOCK, Identifier.of({0}), new Block(AbstractBlock.Settings.create().strength({1})))",
        ["McRegistry.RegisterBlockWithSettings"] = "Registry.register(Registries.BLOCK, Identifier.of({0}), new Block({1}))",

        // Item registration
        ["McRegistry.RegisterItem"]         = "Registry.register(Registries.ITEM, Identifier.of({0}), new Item(new Item.Settings()))",
        ["McRegistry.RegisterItemWithSettings"] = "Registry.register(Registries.ITEM, Identifier.of({0}), new Item({1}))",
        ["McRegistry.RegisterBlockItem"]    = "Registry.register(Registries.ITEM, Identifier.of({0}), new BlockItem({1}, new Item.Settings()))",

        // Tool material enum values → Java uppercase constants
        ["McToolMaterial.Wood"]     = "WOOD",
        ["McToolMaterial.Stone"]    = "STONE",
        ["McToolMaterial.Iron"]     = "IRON",
        ["McToolMaterial.Gold"]     = "GOLD",
        ["McToolMaterial.Diamond"]  = "DIAMOND",
        ["McToolMaterial.Netherite"] = "NETHERITE",

        // Armor material enum values
        ["McArmorMaterial.Leather"]   = "LEATHER",
        ["McArmorMaterial.Chain"]     = "CHAIN",
        ["McArmorMaterial.Iron"]      = "IRON",
        ["McArmorMaterial.Gold"]      = "GOLD",
        ["McArmorMaterial.Diamond"]   = "DIAMOND",
        ["McArmorMaterial.Netherite"] = "NETHERITE",
        ["McArmorMaterial.Turtle"]    = "TURTLE",

        // Tool/weapon registration (full 4 args: id, material, attackDamage, attackSpeed)
        // MC 1.21.1: ToolItem(ToolMaterial, Settings.attributeModifiers(...))
        ["McRegistry.RegisterSword"]    = "Registry.register(Registries.ITEM, Identifier.of({0}), new SwordItem(ToolMaterials.{1}, new Item.Settings().attributeModifiers(SwordItem.createAttributeModifiers(ToolMaterials.{1}, {2}, {3}))))",
        ["McRegistry.RegisterPickaxe"]  = "Registry.register(Registries.ITEM, Identifier.of({0}), new PickaxeItem(ToolMaterials.{1}, new Item.Settings().attributeModifiers(PickaxeItem.createAttributeModifiers(ToolMaterials.{1}, {2}, {3}))))",
        ["McRegistry.RegisterAxe"]      = "Registry.register(Registries.ITEM, Identifier.of({0}), new AxeItem(ToolMaterials.{1}, new Item.Settings().attributeModifiers(AxeItem.createAttributeModifiers(ToolMaterials.{1}, {2}, {3}))))",
        ["McRegistry.RegisterShovel"]   = "Registry.register(Registries.ITEM, Identifier.of({0}), new ShovelItem(ToolMaterials.{1}, new Item.Settings().attributeModifiers(ShovelItem.createAttributeModifiers(ToolMaterials.{1}, {2}, {3}))))",
        ["McRegistry.RegisterHoe"]      = "Registry.register(Registries.ITEM, Identifier.of({0}), new HoeItem(ToolMaterials.{1}, new Item.Settings().attributeModifiers(HoeItem.createAttributeModifiers(ToolMaterials.{1}, {2}, {3}))))",

        // Tool/weapon registration (2 args — id + material, use defaults)
        ["McRegistry.RegisterSword/2"]   = "Registry.register(Registries.ITEM, Identifier.of({0}), new SwordItem(ToolMaterials.{1}, new Item.Settings().attributeModifiers(SwordItem.createAttributeModifiers(ToolMaterials.{1}, 3, -2.4f))))",
        ["McRegistry.RegisterPickaxe/2"] = "Registry.register(Registries.ITEM, Identifier.of({0}), new PickaxeItem(ToolMaterials.{1}, new Item.Settings().attributeModifiers(PickaxeItem.createAttributeModifiers(ToolMaterials.{1}, 1, -2.8f))))",
        ["McRegistry.RegisterAxe/2"]     = "Registry.register(Registries.ITEM, Identifier.of({0}), new AxeItem(ToolMaterials.{1}, new Item.Settings().attributeModifiers(AxeItem.createAttributeModifiers(ToolMaterials.{1}, 6.0f, -3.1f))))",
        ["McRegistry.RegisterShovel/2"]  = "Registry.register(Registries.ITEM, Identifier.of({0}), new ShovelItem(ToolMaterials.{1}, new Item.Settings().attributeModifiers(ShovelItem.createAttributeModifiers(ToolMaterials.{1}, 1.5f, -3.0f))))",
        ["McRegistry.RegisterHoe/2"]     = "Registry.register(Registries.ITEM, Identifier.of({0}), new HoeItem(ToolMaterials.{1}, new Item.Settings().attributeModifiers(HoeItem.createAttributeModifiers(ToolMaterials.{1}, 0, -3.0f))))",

        // Tool/weapon registration (3 args — id + material + bonusDamage, use default attackSpeed)
        ["McRegistry.RegisterSword/3"]   = "Registry.register(Registries.ITEM, Identifier.of({0}), new SwordItem(ToolMaterials.{1}, new Item.Settings().attributeModifiers(SwordItem.createAttributeModifiers(ToolMaterials.{1}, {2}, -2.4f))))",
        ["McRegistry.RegisterPickaxe/3"] = "Registry.register(Registries.ITEM, Identifier.of({0}), new PickaxeItem(ToolMaterials.{1}, new Item.Settings().attributeModifiers(PickaxeItem.createAttributeModifiers(ToolMaterials.{1}, {2}, -2.8f))))",
        ["McRegistry.RegisterAxe/3"]     = "Registry.register(Registries.ITEM, Identifier.of({0}), new AxeItem(ToolMaterials.{1}, new Item.Settings().attributeModifiers(AxeItem.createAttributeModifiers(ToolMaterials.{1}, {2}, -3.1f))))",
        ["McRegistry.RegisterShovel/3"]  = "Registry.register(Registries.ITEM, Identifier.of({0}), new ShovelItem(ToolMaterials.{1}, new Item.Settings().attributeModifiers(ShovelItem.createAttributeModifiers(ToolMaterials.{1}, {2}, -3.0f))))",
        ["McRegistry.RegisterHoe/3"]     = "Registry.register(Registries.ITEM, Identifier.of({0}), new HoeItem(ToolMaterials.{1}, new Item.Settings().attributeModifiers(HoeItem.createAttributeModifiers(ToolMaterials.{1}, {2}, -3.0f))))",

        // Food registration
        ["McRegistry.RegisterFood"]     = "Registry.register(Registries.ITEM, Identifier.of({0}), new Item(new Item.Settings().food(new FoodComponent.Builder().nutrition({1}).saturationModifier({2}).build())))",

        // Armor registration
        // MC 1.21.1: ArmorItem(ArmorMaterial, ArmorItem.Type, Settings)
        ["McRegistry.RegisterHelmet"]       = "Registry.register(Registries.ITEM, Identifier.of({0}), new ArmorItem(ArmorMaterials.{1}, ArmorItem.Type.HELMET, new Item.Settings()))",
        ["McRegistry.RegisterChestplate"]   = "Registry.register(Registries.ITEM, Identifier.of({0}), new ArmorItem(ArmorMaterials.{1}, ArmorItem.Type.CHESTPLATE, new Item.Settings()))",
        ["McRegistry.RegisterLeggings"]     = "Registry.register(Registries.ITEM, Identifier.of({0}), new ArmorItem(ArmorMaterials.{1}, ArmorItem.Type.LEGGINGS, new Item.Settings()))",
        ["McRegistry.RegisterBoots"]        = "Registry.register(Registries.ITEM, Identifier.of({0}), new ArmorItem(ArmorMaterials.{1}, ArmorItem.Type.BOOTS, new Item.Settings()))",

        // Sound registration
        ["McRegistry.RegisterSound"]    = "Registry.register(Registries.SOUND_EVENT, Identifier.of({0}), SoundEvent.of(Identifier.of({0})))",

        // Attribute registration
        ["McRegistry.RegisterAttribute"]= "Registry.register(Registries.ATTRIBUTE, Identifier.of({0}), new ClampedEntityAttribute({0}, {1}, {2}, {3}))",

        // Entity type registration
        ["McRegistry.RegisterEntity"]       = "Registry.register(Registries.ENTITY_TYPE, Identifier.of({0}), EntityType.Builder.create(net.minecraft.entity.mob.MobEntity::new, net.minecraft.entity.SpawnGroup.valueOf({1}.toUpperCase())).dimensions({2}f, {3}f).build(Identifier.of({0}).toString()))",

        // Block entity registration
        ["McRegistry.RegisterBlockEntity"]  = "Registry.register(Registries.BLOCK_ENTITY_TYPE, Identifier.of({0}), net.minecraft.block.entity.BlockEntityType.Builder.create(net.minecraft.block.entity.BlockEntity::new).build())",

        // Game rule registration
        ["McRegistry.RegisterBoolRule"]     = "GameRuleRegistry.register({0}, GameRules.Category.MISC, GameRuleFactory.createBooleanRule({1}))",
        ["McRegistry.RegisterIntRule"]      = "GameRuleRegistry.register({0}, GameRules.Category.MISC, GameRuleFactory.createIntRule({1}))",

        // Enchantment helpers (MC 1.21.1 — uses dynamic registry via server.getRegistryManager())
        ["McEnchantment.GetLevel"]      = "server.getRegistryManager().get(net.minecraft.registry.RegistryKeys.ENCHANTMENT).getEntry(net.minecraft.registry.RegistryKey.of(net.minecraft.registry.RegistryKeys.ENCHANTMENT, net.minecraft.util.Identifier.of({1}))).map(_glEnch -> net.minecraft.enchantment.EnchantmentHelper.getLevel(_glEnch, {0})).orElse(0)",
        ["McEnchantment.HasEnchantment"]= "server.getRegistryManager().get(net.minecraft.registry.RegistryKeys.ENCHANTMENT).getEntry(net.minecraft.registry.RegistryKey.of(net.minecraft.registry.RegistryKeys.ENCHANTMENT, net.minecraft.util.Identifier.of({1}))).map(_heEnch -> net.minecraft.enchantment.EnchantmentHelper.getLevel(_heEnch, {0}) > 0).orElse(false)",

        // Block settings factory
        ["McBlockSettings.Create"]      = "AbstractBlock.Settings.create()",
        ["McBlockSettings.CopyOf"]      = "AbstractBlock.Settings.copyOf({0})",

        // Item settings factory
        ["McItemSettings.Create"]       = "new Item.Settings()",

        // Command registration (simplified wrappers)
        ["McCommand.Register"]          = "CommandRegistrationCallback.EVENT.register((dispatcher, registryAccess, environment) -> dispatcher.register(CommandManager.literal({0}).executes(ctx -> { {1}(ctx.getSource()); return 1; })))",
        ["McCommand.RegisterOp"]        = "CommandRegistrationCallback.EVENT.register((dispatcher, registryAccess, environment) -> dispatcher.register(CommandManager.literal({0}).requires(src -> src.hasPermissionLevel(2)).executes(ctx -> { {1}(ctx.getSource()); return 1; })))",
        ["McCommand.RegisterSub"]       = "CommandRegistrationCallback.EVENT.register((dispatcher, registryAccess, environment) -> dispatcher.register(CommandManager.literal({0}).then(CommandManager.literal({1}).executes(ctx -> { {2}(ctx.getSource()); return 1; }))))",

        // Game rule queries
        ["McGameRules.DoMobSpawning"]       = "{0}.getGameRules().getBoolean(GameRules.DO_MOB_SPAWNING)",
        ["McGameRules.DoDaylightCycle"]     = "{0}.getGameRules().getBoolean(GameRules.DO_DAYLIGHT_CYCLE)",
        ["McGameRules.DoFireTick"]          = "{0}.getGameRules().getBoolean(GameRules.DO_FIRE_TICK)",
        ["McGameRules.DoMobLoot"]           = "{0}.getGameRules().getBoolean(GameRules.DO_MOB_LOOT)",
        ["McGameRules.KeepInventory"]       = "{0}.getGameRules().getBoolean(GameRules.KEEP_INVENTORY)",
        ["McGameRules.MobGriefing"]         = "{0}.getGameRules().getBoolean(GameRules.MOB_GRIEFING)",
        ["McGameRules.Pvp"]                 = "{0}.getGameRules().getBoolean(GameRules.PVP)",
        ["McGameRules.RandomTickSpeed"]     = "{0}.getGameRules().getInt(GameRules.RANDOM_TICK_SPEED)",
        ["McGameRules.MaxEntityCramming"]   = "{0}.getGameRules().getInt(GameRules.MAX_ENTITY_CRAMMING)",

        // Attribute helpers
        ["McAttribute.GetValue"]        = "{0}.getAttributeValue(EntityAttributes.GENERIC_{1})",
        ["McAttribute.GetBaseValue"]    = "{0}.getAttributeBaseValue(EntityAttributes.GENERIC_{1})",

        // ── McTag ────────────────────────────────────────────────────────────
        ["McTag.BlockIsIn"]             = "{0}.getBlockState(new BlockPos({1},{2},{3})).isIn(net.minecraft.registry.tag.BlockTags.create(Identifier.of({4})))",
        ["McTag.IsInTag"]               = "{0}.getDefaultState().isIn(net.minecraft.registry.tag.BlockTags.create(Identifier.of({1})))",
        ["McTag.IsLog"]                 = "{0}.getBlockState(new BlockPos({1},{2},{3})).isIn(net.minecraft.registry.tag.BlockTags.LOGS)",
        ["McTag.IsLeaves"]              = "{0}.getBlockState(new BlockPos({1},{2},{3})).isIn(net.minecraft.registry.tag.BlockTags.LEAVES)",
        ["McTag.IsDirt"]                = "{0}.getBlockState(new BlockPos({1},{2},{3})).isIn(net.minecraft.registry.tag.BlockTags.DIRT)",
        ["McTag.IsStone"]               = "{0}.getBlockState(new BlockPos({1},{2},{3})).isIn(net.minecraft.registry.tag.BlockTags.STONE_ORE_REPLACEABLES)",
        ["McTag.ItemIsIn"]              = "{0}.isIn(net.minecraft.registry.tag.ItemTags.create(Identifier.of({1})))",
        ["McTag.IsSword"]               = "{0}.isIn(net.minecraft.registry.tag.ItemTags.SWORDS)",
        ["McTag.IsPickaxe"]             = "{0}.isIn(net.minecraft.registry.tag.ItemTags.PICKAXES)",
        ["McTag.IsAxe"]                 = "{0}.isIn(net.minecraft.registry.tag.ItemTags.AXES)",
        ["McTag.IsFish"]                = "{0}.isIn(net.minecraft.registry.tag.ItemTags.FISHES)",
        ["McTag.EntityIsIn"]            = "{0}.getType().isIn(net.minecraft.registry.tag.EntityTypeTags.create(Identifier.of({1})))",
        ["McTag.IsUndead"]              = "{0}.getType().isIn(net.minecraft.registry.tag.EntityTypeTags.UNDEAD)",
        ["McTag.CanBreatheUnderwater"]  = "{0}.getType().isIn(net.minecraft.registry.tag.EntityTypeTags.CAN_BREATHE_UNDER_WATER)",
        ["McTag.IsBoss"]                = "{0} instanceof net.minecraft.entity.boss.WitherEntity || {0} instanceof net.minecraft.entity.boss.dragon.EnderDragonEntity",
        ["McTag.IsArmor"]               = "{0}.isIn(net.minecraft.registry.tag.ItemTags.ARMOR_MATERIALS)",
        ["McTag.IsHoe"]                 = "{0}.isIn(net.minecraft.registry.tag.ItemTags.HOES)",
        ["McTag.IsShovel"]              = "{0}.isIn(net.minecraft.registry.tag.ItemTags.SHOVELS)",
        ["McTag.IsRangedWeapon"]        = "{0}.isIn(net.minecraft.registry.tag.ItemTags.BOWS) || {0}.isIn(net.minecraft.registry.tag.ItemTags.CROSSBOWS)",
        ["McTag.IsWearable"]            = "{0}.isIn(net.minecraft.registry.tag.ItemTags.HEAD_ARMOR) || {0}.isIn(net.minecraft.registry.tag.ItemTags.CHEST_ARMOR) || {0}.isIn(net.minecraft.registry.tag.ItemTags.LEG_ARMOR) || {0}.isIn(net.minecraft.registry.tag.ItemTags.FOOT_ARMOR)",

        // ── McFluid ──────────────────────────────────────────────────────────
        ["McFluid.IsFluid"]             = "!{0}.getFluidState(new BlockPos({1},{2},{3})).isEmpty()",
        ["McFluid.IsWater"]             = "{0}.getFluidState(new BlockPos({1},{2},{3})).isIn(net.minecraft.registry.tag.FluidTags.WATER)",
        ["McFluid.IsLava"]              = "{0}.getFluidState(new BlockPos({1},{2},{3})).isIn(net.minecraft.registry.tag.FluidTags.LAVA)",
        ["McFluid.IsSource"]            = "{0}.getFluidState(new BlockPos({1},{2},{3})).isStill()",
        ["McFluid.GetLevel"]            = "{0}.getFluidState(new BlockPos({1},{2},{3})).getLevel()",
        ["McFluid.IsPlayerSubmerged"]   = "{0}.isSubmergedInWater()",
        ["McFluid.IsPlayerInLava"]      = "{0}.isInLava()",

        // ── McStructure ───────────────────────────────────────────────────────
        ["McStructure.IsInsideStructure"] = "{0}.hasStructure(new BlockPos({1},{2},{3}), Registries.STRUCTURE.getOrThrow(net.minecraft.registry.RegistryKey.of(net.minecraft.registry.RegistryKeys.STRUCTURE, Identifier.of({4}))))",
        ["McStructure.FindNearest"]       = "{0}.locateStructure(Registries.STRUCTURE.getOrThrow(net.minecraft.registry.RegistryKey.of(net.minecraft.registry.RegistryKeys.STRUCTURE, Identifier.of({1}))), new BlockPos((int){0}.getLevelProperties().getSpawnX(), 64, (int){0}.getLevelProperties().getSpawnZ()), 100, false)",
        ["McStructure.Place"]             = "{ var _structManager = {0}.getServer().getStructureTemplateManager(); var _template = _structManager.getTemplateOrBlank(Identifier.of({4})); var _placement = new net.minecraft.structure.StructurePlacementData(); _template.place((ServerWorld){0}, new BlockPos({1},{2},{3}), new BlockPos({1},{2},{3}), _placement, {0}.getRandom(), 2); }",

        // ── McAdvancement ─────────────────────────────────────────────────────
        ["McAdvancement.Grant"]          = "{ var _adv = {0}.getServer().getAdvancementLoader().get(Identifier.of({1})); if (_adv != null) { var _prog = {0}.getAdvancementTracker().getProgress(_adv); for (var _crit : _prog.getUnobtainedCriteria()) {0}.getAdvancementTracker().grantCriterion(_adv, _crit); } }",
        ["McAdvancement.Revoke"]         = "{ var _adv2 = {0}.getServer().getAdvancementLoader().get(Identifier.of({1})); if (_adv2 != null) { var _prog2 = {0}.getAdvancementTracker().getProgress(_adv2); for (var _crit2 : _prog2.getObtainedCriteria()) {0}.getAdvancementTracker().revokeCriterion(_adv2, _crit2); } }",
        ["McAdvancement.HasCompleted"]   = "{ var _adv3 = {0}.getServer().getAdvancementLoader().get(Identifier.of({1})); _adv3 != null && {0}.getAdvancementTracker().getProgress(_adv3).isDone(); }",
        ["McAdvancement.GrantCriterion"] = "{ var _adv4 = {0}.getServer().getAdvancementLoader().get(Identifier.of({1})); if (_adv4 != null) {0}.getAdvancementTracker().grantCriterion(_adv4, {2}); }",

        // ── McLootTable ───────────────────────────────────────────────────────
        ["McLootTable.DropLoot"]         = "{ var _lootCtx = new net.minecraft.loot.context.LootContextParameterSet.Builder((ServerWorld){0}).add(net.minecraft.loot.context.LootContextParameters.ORIGIN, new net.minecraft.util.math.Vec3d({2},{3},{4})).build(net.minecraft.loot.context.LootContextTypes.CHEST); var _table = {0}.getServer().getReloadableRegistries().getLootTable(net.minecraft.registry.RegistryKey.of(net.minecraft.loot.LootTable.REGISTRY_KEY, Identifier.of({1}))); _table.generateLoot(_lootCtx).forEach(s -> net.minecraft.entity.ItemEntity.spawn((ServerWorld){0}, new net.minecraft.util.math.BlockPos((int){2},(int){3},(int){4}), s)); }",
        ["McLootTable.GiveLootToPlayer"] = "{ var _lootCtx2 = new net.minecraft.loot.context.LootContextParameterSet.Builder((ServerWorld){0}.getWorld()).add(net.minecraft.loot.context.LootContextParameters.THIS_ENTITY, {0}).build(net.minecraft.loot.context.LootContextTypes.ENTITY); var _table2 = {0}.getServer().getReloadableRegistries().getLootTable(net.minecraft.registry.RegistryKey.of(net.minecraft.loot.LootTable.REGISTRY_KEY, Identifier.of({1}))); _table2.generateLoot(_lootCtx2).forEach(s -> {0}.getInventory().insertStack(s)); }",

        // ── McPotion ──────────────────────────────────────────────────────────
        ["McPotion.GetPotionId"]             = "PotionUtil.getPotion({0}).getId(Registries.POTION)",
        ["McPotion.HasEffect"]               = "PotionUtil.getPotionEffects({0}).stream().anyMatch(e -> e.getEffectType() == Registries.STATUS_EFFECT.get(Identifier.of({1})))",
        ["McPotion.GetEffects"]              = "PotionUtil.getPotionEffects({0})",
        ["McPotion.RegisterBrewingRecipe"]   = "BrewingRecipeRegistry.registerPotionRecipe(Registries.POTION.get(Identifier.of({0})), {1}.asItem(), Registries.POTION.get(Identifier.of({2})))",

        // ── McProjectile ──────────────────────────────────────────────────────
        ["McProjectile.ThrowSnowball"]  = "{ SnowballEntity _proj = new SnowballEntity({0}.getServerWorld(), {0}); _proj.setVelocity({0}, {0}.getPitch(), {0}.getYaw(), 0, 1.5f, 1.0f); {0}.getServerWorld().spawnEntity(_proj); }",
        ["McProjectile.ShootArrow"]     = "{ ArrowEntity _arr = new ArrowEntity({0}.getServerWorld(), {0}, {0}.getMainHandStack(), null); _arr.setVelocity({0}, {0}.getPitch(), {0}.getYaw(), 0, 3.0f, 1.0f); {0}.getServerWorld().spawnEntity(_arr); }",
        ["McProjectile.SpawnFireball"]  = "{ SmallFireballEntity _fb = new SmallFireballEntity({0}.getServer().getWorld(net.minecraft.world.World.OVERWORLD), null, {1}, {2}, {3}); {0}.getServer().getWorld(net.minecraft.world.World.OVERWORLD).spawnEntity(_fb); }",
        ["McProjectile.ThrowPotion"]    = "{ ThrownPotionEntity _pot = new ThrownPotionEntity({0}.getServerWorld(), {0}); _pot.setItem(PotionUtil.setPotion(new net.minecraft.item.ItemStack(net.minecraft.item.Items.SPLASH_POTION), Registries.POTION.get(new net.minecraft.util.Identifier({1})))); _pot.setVelocity({0}, {0}.getPitch(), {0}.getYaw(), -20.0f, 0.5f, 0.5f); {0}.getServerWorld().spawnEntity(_pot); }",

        // ── McVillager ────────────────────────────────────────────────────────
        ["McVillager.GetProfession"]    = "Registries.VILLAGER_PROFESSION.getId(((net.minecraft.entity.passive.VillagerEntity){0}).getVillagerData().getProfession()).toString()",
        ["McVillager.GetLevel"]         = "((net.minecraft.entity.passive.VillagerEntity){0}).getVillagerData().getLevel()",
        ["McVillager.GetType"]          = "Registries.VILLAGER_TYPE.getId(((net.minecraft.entity.passive.VillagerEntity){0}).getVillagerData().getType()).toString()",
        ["McVillager.AddSellTrade"]     = "/* TODO: register sell trade for profession {0} level {1} — see VillagerTrades.registerCustomTradeList */",
        ["McVillager.AddBuyTrade"]      = "/* TODO: register buy trade for profession {0} level {1} — see VillagerTrades.registerCustomTradeList */",

        // ── McRecipe ──────────────────────────────────────────────────────────
        ["McRecipe.RegisterShaped"]         = "/* TODO: register shaped recipe {0} as JSON in data/modid/recipes/ */",
        ["McRecipe.RegisterShapeless"]      = "/* TODO: register shapeless recipe {0} as JSON in data/modid/recipes/ */",
        ["McRecipe.RegisterSmelting"]       = "/* TODO: register smelting recipe {0} as JSON in data/modid/recipes/ */",
        ["McRecipe.RegisterBlasting"]       = "/* TODO: register blasting recipe {0} as JSON in data/modid/recipes/ */",
        ["McRecipe.RegisterSmoking"]        = "/* TODO: register smoking recipe {0} as JSON in data/modid/recipes/ */",
        ["McRecipe.RegisterCampfire"]       = "/* TODO: register campfire recipe {0} as JSON in data/modid/recipes/ */",
        ["McRecipe.RegisterStonecutting"]   = "/* TODO: register stonecutting recipe {0} as JSON in data/modid/recipes/ */",
        ["McRecipe.PlayerKnowsRecipe"]      = "{0}.getRecipeBook().contains(Identifier.of({1}))",
        ["McRecipe.UnlockForPlayer"]        = "{0}.unlockRecipes(new net.minecraft.util.Identifier[] {{ Identifier.of({1}) }})",
        ["McRecipe.LockForPlayer"]          = "{0}.lockRecipes(new net.minecraft.util.Identifier[] {{ Identifier.of({1}) }})",

        // ── McCreativeTab ─────────────────────────────────────────────────────
        ["McCreativeTab.AddToBuildingBlocks"]   = "ItemGroupEvents.modifyEntriesEvent(ItemGroups.BUILDING_BLOCKS).register(e -> e.add({0}))",
        ["McCreativeTab.AddToNaturalBlocks"]    = "ItemGroupEvents.modifyEntriesEvent(ItemGroups.NATURAL).register(e -> e.add({0}))",
        ["McCreativeTab.AddToFunctional"]       = "ItemGroupEvents.modifyEntriesEvent(ItemGroups.FUNCTIONAL).register(e -> e.add({0}))",
        ["McCreativeTab.AddToRedstone"]         = "ItemGroupEvents.modifyEntriesEvent(ItemGroups.REDSTONE).register(e -> e.add({0}))",
        ["McCreativeTab.AddToTools"]            = "ItemGroupEvents.modifyEntriesEvent(ItemGroups.TOOLS).register(e -> e.add({0}))",
        ["McCreativeTab.AddToCombat"]           = "ItemGroupEvents.modifyEntriesEvent(ItemGroups.COMBAT).register(e -> e.add({0}))",
        ["McCreativeTab.AddToFood"]             = "ItemGroupEvents.modifyEntriesEvent(ItemGroups.FOOD_AND_DRINK).register(e -> e.add({0}))",
        ["McCreativeTab.AddToIngredients"]      = "ItemGroupEvents.modifyEntriesEvent(ItemGroups.INGREDIENTS).register(e -> e.add({0}))",
        ["McCreativeTab.AddToSpawnEggs"]        = "ItemGroupEvents.modifyEntriesEvent(ItemGroups.SPAWN_EGGS).register(e -> e.add({0}))",
        ["McCreativeTab.AddToOperator"]         = "ItemGroupEvents.modifyEntriesEvent(ItemGroups.OPERATOR).register(e -> e.add({0}))",
        ["McCreativeTab.AddToTab"]              = "ItemGroupEvents.modifyEntriesEvent(RegistryKey.of(RegistryKeys.ITEM_GROUP, Identifier.of({0}))).register(e -> e.add({1}))",

        // ── PlayerData ────────────────────────────────────────────────────────
        // Overloads handled by suffix: Set(player, key, int) etc.
        ["PlayerData.Set"]          = "if ({0} instanceof ServerPlayerEntity _spd) { NbtCompound _pdN = ModPlayerData.getPlayerNbt(_spd); _pdN.putInt({1}, {2}); }",
        ["PlayerData.GetInt"]       = "({0} instanceof ServerPlayerEntity _spgi ? (ModPlayerData.getPlayerNbt(_spgi).contains({1}) ? ModPlayerData.getPlayerNbt(_spgi).getInt({1}) : {2}) : {2})",
        ["PlayerData.GetLong"]      = "({0} instanceof ServerPlayerEntity _spgl ? (ModPlayerData.getPlayerNbt(_spgl).contains({1}) ? ModPlayerData.getPlayerNbt(_spgl).getLong({1}) : {2}) : {2})",
        ["PlayerData.GetFloat"]     = "({0} instanceof ServerPlayerEntity _spgf ? (ModPlayerData.getPlayerNbt(_spgf).contains({1}) ? ModPlayerData.getPlayerNbt(_spgf).getFloat({1}) : {2}) : {2})",
        ["PlayerData.GetDouble"]    = "({0} instanceof ServerPlayerEntity _spgd ? (ModPlayerData.getPlayerNbt(_spgd).contains({1}) ? ModPlayerData.getPlayerNbt(_spgd).getDouble({1}) : {2}) : {2})",
        ["PlayerData.GetBool"]      = "({0} instanceof ServerPlayerEntity _spgb ? (ModPlayerData.getPlayerNbt(_spgb).contains({1}) ? ModPlayerData.getPlayerNbt(_spgb).getBoolean({1}) : {2}) : {2})",
        ["PlayerData.GetString"]    = "({0} instanceof ServerPlayerEntity _spgs ? (ModPlayerData.getPlayerNbt(_spgs).contains({1}) ? ModPlayerData.getPlayerNbt(_spgs).getString({1}) : {2}) : {2})",
        ["PlayerData.GetBlockPos"]  = "({0} instanceof ServerPlayerEntity _spgp ? new net.minecraft.util.math.BlockPos(ModPlayerData.getPlayerNbt(_spgp).getInt({1} + \"_x\"), ModPlayerData.getPlayerNbt(_spgp).getInt({1} + \"_y\"), ModPlayerData.getPlayerNbt(_spgp).getInt({1} + \"_z\")) : BlockPos.ORIGIN)",
        ["PlayerData.Has"]          = "({0} instanceof ServerPlayerEntity _sph ? ModPlayerData.getPlayerNbt(_sph).contains({1}) : false)",
        ["PlayerData.Remove"]       = "if ({0} instanceof ServerPlayerEntity _spr) { ModPlayerData.getPlayerNbt(_spr).remove({1}); }",

        // ── WorldData ─────────────────────────────────────────────────────────
        ["WorldData.GetInt"]        = "/* WorldData.GetInt — use WorldData-generated PersistentState */",
        ["WorldData.GetBool"]       = "/* WorldData.GetBool — use WorldData-generated PersistentState */",
        ["WorldData.GetString"]     = "/* WorldData.GetString — use WorldData-generated PersistentState */",
        ["WorldData.GetFloat"]      = "/* WorldData.GetFloat — use WorldData-generated PersistentState */",
        ["WorldData.Has"]           = "/* WorldData.Has — use WorldData-generated PersistentState */",
        ["WorldData.Remove"]        = "/* WorldData.Remove — use WorldData-generated PersistentState */",

        // ── Schedule ──────────────────────────────────────────────────────────
        // McScheduler already handles RunLater/RunRepeating — Schedule.After/Every map to those
        ["Schedule.After"]          = "/* Schedule.After({0}) — use McScheduler.RunLater(server, ticks, action) */",
        ["Schedule.Every"]          = "/* Schedule.Every({0}) — use McScheduler.RunRepeating(server, ticks, action) */",

        // ── LootTables ────────────────────────────────────────────────────────
        ["LootTables.AddMobDrop"]   = "/* TODO: register mob loot for {0}: add {1} with chance {2} in data/modid/loot_table/entities/ */",
        ["LootTables.RemoveMobDrop"]= "/* TODO: remove mob drop {1} from {0} — use Fabric LootTableEvents.MODIFY */",
        ["LootTables.AddChestLoot"] = "/* TODO: add {1} to chest loot table — use Fabric LootTableEvents.MODIFY */",
        ["LootTables.AddToTable"]   = "/* TODO: add {1} to loot table {0} — use Fabric LootTableEvents.MODIFY */",
        ["LootTables.RemoveFromTable"] = "/* TODO: remove {1} from loot table {0} — use Fabric LootTableEvents.MODIFY */",

        // ── Recipes fluent (build-time only — emit TODO) ──────────────────────
        ["Recipes.AddShaped"]       = "/* TODO: shaped recipe {0} — define in data/modid/recipes/ */",
        ["Recipes.AddShapeless"]    = "/* TODO: shapeless recipe {0} — define in data/modid/recipes/ */",
        ["Recipes.AddSmelting"]     = "/* TODO: smelting recipe {0} — define in data/modid/recipes/ */",
        ["Recipes.AddBlasting"]     = "/* TODO: blasting recipe {0} — define in data/modid/recipes/ */",
        ["Recipes.AddSmoking"]      = "/* TODO: smoking recipe {0} — define in data/modid/recipes/ */",
        ["Recipes.AddCampfire"]     = "/* TODO: campfire recipe {0} — define in data/modid/recipes/ */",
        ["Recipes.AddStonecutting"] = "/* TODO: stonecutting recipe {0} — define in data/modid/recipes/ */",
        ["Recipes.AddSmithingTransform"] = "/* TODO: smithing recipe {0} — define in data/modid/recipes/ */",

        // ── Config ────────────────────────────────────────────────────────────
        ["Config.Load"]             = "/* Config.Load<T>() — generated config class, use field directly */",
        ["Config.Reload"]           = "/* Config.Reload<T>() — re-read config/modid.toml */",
        ["Config.Save"]             = "/* Config.Save(cfg) — write config/modid.toml */",

        // ── McScoreboard static ────────────────────────────────────────────────
        ["McScoreboard.CreateObjective"] = "{ var _sb = {0}.getScoreboard(); if (_sb.getNullableObjective({1}) == null) _sb.addObjective({1}, ScoreboardCriterion.DUMMY, Text.literal({2}), ScoreboardCriterion.RenderType.INTEGER, false, null); }",
        ["McScoreboard.RemoveObjective"] = "{0}.getScoreboard().removeObjective({0}.getScoreboard().getNullableObjective({1}))",
        ["McScoreboard.ShowSidebar"]     = "{0}.getScoreboard().setObjectiveSlot(ScoreboardDisplaySlot.SIDEBAR, {0}.getScoreboard().getNullableObjective({1}))",
        ["McScoreboard.HideSidebar"]     = "{0}.getScoreboard().setObjectiveSlot(ScoreboardDisplaySlot.SIDEBAR, null)",
        ["McScoreboard.ShowTabList"]     = "{0}.getScoreboard().setObjectiveSlot(ScoreboardDisplaySlot.LIST, {0}.getScoreboard().getNullableObjective({1}))",
        ["McScoreboard.ShowBelowName"]   = "{0}.getScoreboard().setObjectiveSlot(ScoreboardDisplaySlot.BELOW_NAME, {0}.getScoreboard().getNullableObjective({1}))",
        ["McScoreboard.GetScore"]        = "{0}.getScoreboard().getOrCreateScore({1}, {0}.getScoreboard().getNullableObjective({2})).getScore()",
        ["McScoreboard.SetScore"]        = "{0}.getScoreboard().getOrCreateScore({1}, {0}.getScoreboard().getNullableObjective({2})).setScore({3})",
        ["McScoreboard.AddScore"]        = "{0}.getScoreboard().getOrCreateScore({1}, {0}.getScoreboard().getNullableObjective({2})).incrementScore({3})",
        ["McScoreboard.ResetScore"]      = "{0}.getScoreboard().resetPlayerScore({1}.getEntityName(), {0}.getScoreboard().getNullableObjective({2}))",
        ["McScoreboard.CreateTeam"]      = "{ if ({0}.getScoreboard().getTeam({1}) == null) {0}.getScoreboard().addTeam({1}); }",
        ["McScoreboard.RemoveTeam"]      = "if ({0}.getScoreboard().getTeam({1}) != null) {0}.getScoreboard().removeTeam({0}.getScoreboard().getTeam({1}))",
        ["McScoreboard.AddPlayerToTeam"] = "{ net.minecraft.scoreboard.Team _at2 = {0}.getScoreboard().getTeam({2}); if (_at2 != null && !_at2.getPlayerList().contains({1}.getName().getString())) _at2.getPlayerList().add({1}.getName().getString()); }",
        ["McScoreboard.RemovePlayerFromTeam"] = "{0}.getScoreboard().removePlayerFromTeam({1}.getEntityName(), {0}.getScoreboard().getPlayerTeam({1}.getEntityName()))",
        ["McScoreboard.GetPlayerTeam"]   = "({0}.getScoreboard().getPlayerTeam({1}.getEntityName()) != null ? {0}.getScoreboard().getPlayerTeam({1}.getEntityName()).getName() : null)",
        ["McScoreboard.SetTeamPrefix"]   = "{ var _t = {0}.getScoreboard().getTeam({1}); if (_t != null) _t.setPrefix(Text.literal({2})); }",
        ["McScoreboard.SetTeamSuffix"]   = "{ var _t2 = {0}.getScoreboard().getTeam({1}); if (_t2 != null) _t2.setSuffix(Text.literal({2})); }",
        ["McScoreboard.SetTeamColor"]    = "{ var _t3 = {0}.getScoreboard().getTeam({1}); if (_t3 != null) _t3.setColor(net.minecraft.util.Formatting.byName({2})); }",
        ["McScoreboard.SetFriendlyFire"] = "{ var _t4 = {0}.getScoreboard().getTeam({1}); if (_t4 != null) _t4.setFriendlyFireAllowed({2}); }",

        // ── McParticles constants ─────────────────────────────────────────────
        // C# const strings — emit as Java string literals so SpawnParticle gets proper "minecraft:xxx"
        ["McParticles.Flame"]         = "\"minecraft:flame\"",
        ["McParticles.Smoke"]         = "\"minecraft:smoke\"",
        ["McParticles.LargeSmoke"]    = "\"minecraft:large_smoke\"",
        ["McParticles.Explosion"]     = "\"minecraft:explosion\"",
        ["McParticles.HugeExplosion"] = "\"minecraft:explosion_emitter\"",
        ["McParticles.Heart"]         = "\"minecraft:heart\"",
        ["McParticles.AngryVillager"] = "\"minecraft:angry_villager\"",
        ["McParticles.HappyVillager"] = "\"minecraft:happy_villager\"",
        ["McParticles.Crit"]          = "\"minecraft:crit\"",
        ["McParticles.MagicCrit"]     = "\"minecraft:enchanted_hit\"",
        ["McParticles.Splash"]        = "\"minecraft:splash\"",
        ["McParticles.Portal"]        = "\"minecraft:portal\"",
        ["McParticles.EnchantTable"]  = "\"minecraft:enchant\"",
        ["McParticles.Witch"]         = "\"minecraft:witch\"",
        ["McParticles.Slime"]         = "\"minecraft:item_slime\"",
        ["McParticles.Snow"]          = "\"minecraft:snowflake\"",
        ["McParticles.Lava"]          = "\"minecraft:lava\"",
        ["McParticles.End"]           = "\"minecraft:end_rod\"",
        ["McParticles.Dragon"]        = "\"minecraft:dragon_breath\"",
        ["McParticles.Bubble"]        = "\"minecraft:bubble\"",
        ["McParticles.Squid"]         = "\"minecraft:squid_ink\"",
        ["McParticles.Nautilus"]      = "\"minecraft:nautilus\"",
        ["McParticles.Note"]          = "\"minecraft:note\"",
        ["McParticles.XP"]            = "\"minecraft:experience_orb\"",
        ["McParticles.Campfire"]      = "\"minecraft:campfire_cosy_smoke\"",
        ["McParticles.Dust"]          = "\"minecraft:dust\"",
        ["McParticles.Falling"]       = "\"minecraft:falling_dust\"",
        ["McParticles.Totem"]         = "\"minecraft:totem_of_undying\"",
        ["McParticles.Warped"]        = "\"minecraft:warped_spore\"",
        ["McParticles.Crimson"]       = "\"minecraft:crimson_spore\"",
        ["McParticles.Glow"]          = "\"minecraft:glow\"",
        ["McParticles.Wax"]           = "\"minecraft:wax_on\"",
        ["McParticles.Electric"]      = "\"minecraft:electric_spark\"",
        ["McParticles.Scrape"]        = "\"minecraft:scrape\"",
        ["McParticles.Sonic"]         = "\"minecraft:sonic_boom\"",
        ["McParticles.Cherry"]        = "\"minecraft:cherry_leaves\"",

        // ── McScheduler static ────────────────────────────────────────────────
        ["McScheduler.RunLater"]      = "{ int _delay = {1}; {0}.execute(() -> { try { Thread.sleep(_delay * 50L); } catch (Exception _e) {} }); }",
        ["McScheduler.RunAsync"]      = "java.util.concurrent.CompletableFuture.runAsync(() -> {1})",
        ["McScheduler.RunRepeating"]  = "/* McScheduler.RunRepeating({0}, {1}) — use ServerTickEvents for repeating tasks */",

        // ── McInventory static ────────────────────────────────────────────────
        ["McInventory.FromPlayer"]   = "{0}.getInventory()",
        ["McInventory.EnderChest"]   = "{0}.getEnderChestInventory()",
        ["McInventory.Give"]         = "{0}.getInventory().insertStack({1})",
        ["McInventory.Take"]         = "{ int _rem = {2}; for (int _i = 0; _i < {0}.getInventory().size() && _rem > 0; _i++) { var _s = {0}.getInventory().getStack(_i); if (!_s.isEmpty() && Registries.ITEM.getId(_s.getItem()).toString().equals({1})) { int _take = Math.min(_s.getCount(), _rem); _s.decrement(_take); _rem -= _take; } } }",
    };

    // ── Property mappings (C# properties → Java getter calls) ────────────────

    public static readonly Dictionary<string, string> Properties = new()
    {
        // McPlayer properties
        ["McPlayer.Name"]           = "{target}.getName().getString()",
        ["McPlayer.Uuid"]           = "{target}.getUuidAsString()",
        ["McPlayer.Health"]         = "{target}.getHealth()",
        ["McPlayer.MaxHealth"]      = "{target}.getMaxHealth()",
        ["McPlayer.FoodLevel"]      = "{target}.getHungerManager().getFoodLevel()",
        ["McPlayer.XpLevel"]        = "{target}.experienceLevel",
        ["McPlayer.X"]              = "{target}.getX()",
        ["McPlayer.Y"]              = "{target}.getY()",
        ["McPlayer.Z"]              = "{target}.getZ()",
        ["McPlayer.World"]          = "((ServerWorld){target}.getWorld())",
        ["McPlayer.Server"]         = "{target}.getServer()",
        ["McPlayer.Inventory"]      = "{target}.getInventory()",
        ["McPlayer.IsAlive"]        = "{target}.isAlive()",
        ["McPlayer.IsSneaking"]     = "{target}.isSneaking()",
        ["McPlayer.IsSprinting"]    = "{target}.isSprinting()",
        ["McPlayer.IsOnGround"]     = "{target}.isOnGround()",
        ["McPlayer.IsCreative"]     = "{target}.isCreative()",
        ["McPlayer.IsFlying"]       = "{target}.getAbilities().flying",
        ["McPlayer.IsSwimming"]     = "{target}.isSwimming()",
        ["McPlayer.IsGliding"]      = "{target}.isGliding()",
        ["McPlayer.Yaw"]            = "{target}.getYaw()",
        ["McPlayer.Pitch"]          = "{target}.getPitch()",
        ["McPlayer.GameMode"]       = "{target}.interactionManager.getGameMode().getName()",
        ["McPlayer.MainHandItem"]   = "{target}.getMainHandStack()",
        ["McPlayer.OffHandItem"]    = "{target}.getOffHandStack()",
        ["McPlayer.Helmet"]         = "{target}.getEquippedStack(net.minecraft.entity.EquipmentSlot.HEAD)",
        ["McPlayer.Chestplate"]     = "{target}.getEquippedStack(net.minecraft.entity.EquipmentSlot.CHEST)",
        ["McPlayer.Leggings"]       = "{target}.getEquippedStack(net.minecraft.entity.EquipmentSlot.LEGS)",
        ["McPlayer.Boots"]          = "{target}.getEquippedStack(net.minecraft.entity.EquipmentSlot.FEET)",
        ["McPlayer.IsBlocking"]     = "{target}.isBlocking()",
        ["McPlayer.IsUsingItem"]    = "{target}.isUsingItem()",
        ["McPlayer.IsSleeping"]     = "{target}.isSleeping()",
        ["McPlayer.IsSpectator"]    = "{target}.isSpectator()",

        // McCommandSource properties
        ["McCommandSource.Player"]  = "{target}.getPlayer()",
        ["McCommandSource.Server"]  = "{target}.getServer()",
        ["McCommandSource.Name"]    = "{target}.getName()",

        // McWorld properties
        ["McWorld.Time"]            = "{target}.getTime()",
        ["McWorld.IsDay"]           = "{target}.isDay()",
        ["McWorld.IsNight"]         = "!{target}.isDay()",
        ["McWorld.IsRaining"]       = "{target}.isRaining()",
        ["McWorld.IsThundering"]    = "{target}.isThundering()",
        ["McWorld.Difficulty"]      = "{target}.getDifficulty().getName()",
        ["McWorld.SpawnPos"]        = "{target}.getSpawnPos()",
        ["McWorld.Dimension"]       = "{target}.getRegistryKey().getValue().toString()",
        ["McWorld.Server"]          = "{target}.getServer()",

        // McServer properties
        ["McServer.OnlinePlayers"]  = "{target}.getPlayerManager().getPlayerList()",
        ["McServer.PlayerCount"]    = "{target}.getPlayerManager().getCurrentPlayerCount()",
        ["McServer.MaxPlayers"]     = "{target}.getPlayerManager().getMaxPlayerCount()",
        ["McServer.Tps"]            = "{target}.getAverageTickTime()",
        ["McServer.Version"]        = "{target}.getVersion()",
        ["McServer.IsRunning"]      = "{target}.isRunning()",
        ["McServer.Motd"]           = "{target}.getServerMotd()",
        ["McServer.IsHardcore"]     = "{target}.isHardcore()",
        ["McServer.Overworld"]      = "(ServerWorld){target}.getWorld(World.OVERWORLD)",
        ["McServer.Nether"]         = "(ServerWorld){target}.getWorld(World.NETHER)",
        ["McServer.End"]            = "(ServerWorld){target}.getWorld(World.END)",
        ["McServer.IsWhitelistEnabled"] = "{target}.getPlayerManager().isWhitelistEnabled()",

        // McEntity properties
        ["McEntity.Name"]           = "{target}.getName().getString()",
        ["McEntity.Uuid"]           = "{target}.getUuidAsString()",
        ["McEntity.X"]              = "{target}.getX()",
        ["McEntity.Y"]              = "{target}.getY()",
        ["McEntity.Z"]              = "{target}.getZ()",
        ["McEntity.BlockPos"]       = "{target}.getBlockPos()",
        ["McEntity.World"]          = "((ServerWorld){target}.getWorld())",
        ["McEntity.IsAlive"]        = "{target}.isAlive()",
        ["McEntity.IsOnGround"]     = "{target}.isOnGround()",
        ["McEntity.IsOnFire"]       = "{target}.isOnFire()",
        ["McEntity.IsInvisible"]    = "{target}.isInvisible()",
        ["McEntity.IsSwimming"]     = "{target}.isSwimming()",
        ["McEntity.IsGliding"]      = "({target} instanceof LivingEntity _lge && _lge.isGliding())",
        ["McEntity.IsPlayer"]       = "{target} instanceof net.minecraft.entity.player.PlayerEntity",
        ["McEntity.IsMob"]          = "{target} instanceof net.minecraft.entity.mob.MobEntity",
        ["McEntity.Health"]         = "({target} instanceof LivingEntity ? ((LivingEntity){target}).getHealth() : 0f)",
        ["McEntity.MaxHealth"]      = "({target} instanceof LivingEntity ? ((LivingEntity){target}).getMaxHealth() : 0f)",
        ["McEntity.TypeId"]         = "EntityType.getId({target}.getType()).toString()",
        ["McEntity.VelocityX"]      = "{target}.getVelocity().getX()",
        ["McEntity.VelocityY"]      = "{target}.getVelocity().getY()",
        ["McEntity.VelocityZ"]      = "{target}.getVelocity().getZ()",
        ["McEntity.Yaw"]            = "{target}.getYaw()",
        ["McEntity.Pitch"]          = "{target}.getPitch()",
        ["McEntity.CustomName"]     = "{target}.hasCustomName() ? {target}.getCustomName().getString() : null",

        // BlockPos properties
        ["BlockPos.X"]              = "{target}.getX()",
        ["BlockPos.Y"]              = "{target}.getY()",
        ["BlockPos.Z"]              = "{target}.getZ()",
        ["McBlockPos.X"]            = "{target}.getX()",
        ["McBlockPos.Y"]            = "{target}.getY()",
        ["McBlockPos.Z"]            = "{target}.getZ()",

        // McItemStack properties
        ["McItemStack.Count"]       = "{target}.getCount()",
        ["McItemStack.Id"]          = "Registries.ITEM.getId({target}.getItem()).toString()",
        ["McItemStack.IsEmpty"]     = "{target}.isEmpty()",
        ["McItemStack.HasNbt"]      = "{target}.hasNbt()",
        // ItemStack properties (alias)
        ["ItemStack.Count"]         = "{target}.getCount()",
        ["ItemStack.Id"]            = "Registries.ITEM.getId({target}.getItem()).toString()",
        ["ItemStack.IsEmpty"]       = "{target}.isEmpty()",
        ["ItemStack.HasNbt"]        = "{target}.hasNbt()",

        // McBlockState properties
        ["McBlockState.IsAir"]      = "{target}.isAir()",
        ["McBlockState.Hardness"]   = "{target}.getHardness(null, BlockPos.ORIGIN)",
        ["McBlockState.Block"]      = "Registries.BLOCK.getId({target}.getBlock()).toString()",

        // McNbt properties
        ["McNbt.IsEmpty"]           = "{target}.isEmpty()",

        // McBossBar properties
        ["McBossBar.Title"]         = "{target}.getName().getString()",
        ["McBossBar.Progress"]      = "{target}.getPercent()",
        ["McBossBar.Color"]         = "{target}.getColor().name().toLowerCase()",
        ["McBossBar.Style"]         = "{target}.getStyle().name()",
        ["McBossBar.IsVisible"]     = "{target}.isVisible()",

        // McInventory properties
        ["McInventory.Size"]        = "{target}.size()",
        ["McInventory.IsEmpty"]     = "{target}.isEmpty()",

        // McBlockEntity properties
        ["McBlockEntity.Pos"]       = "{target}.getPos()",
        ["McBlockEntity.World"]     = "{target}.getWorld()",
        ["McBlockEntity.TypeId"]    = "net.minecraft.registry.Registries.BLOCK_ENTITY_TYPE.getId({target}.getType()).toString()",
        ["McBlockEntity.IsRemoved"] = "{target}.isRemoved()",
        ["McBlockEntity.IsChest"]   = "{target} instanceof net.minecraft.block.entity.ChestBlockEntity",
        ["McBlockEntity.IsFurnace"] = "{target} instanceof net.minecraft.block.entity.AbstractFurnaceBlockEntity",
        ["McBlockEntity.IsHopper"]  = "{target} instanceof net.minecraft.block.entity.HopperBlockEntity",
    };

    // ── Property return-type table ────────────────────────────────────────────
    // Allows the emitter to resolve method calls on property expressions like player.Helmet.GetItem()

    private static readonly Dictionary<string, string> PropertyReturnTypes = new()
    {
        ["McPlayer.MainHandItem"]   = "McItemStack",
        ["McPlayer.OffHandItem"]    = "McItemStack",
        ["McPlayer.Helmet"]         = "McItemStack",
        ["McPlayer.Chestplate"]     = "McItemStack",
        ["McPlayer.Leggings"]       = "McItemStack",
        ["McPlayer.Boots"]          = "McItemStack",
        ["McEntity.MainHandItem"]   = "McItemStack",
        ["McPlayer.World"]          = "McWorld",
        ["McEntity.World"]          = "McWorld",
        ["McCommandSource.Player"]  = "McPlayer",
        ["McCommandSource.Server"]  = "McServer",
    };

    /// <summary>
    /// Returns the C# return type of a property access, or null if unknown.
    /// Used so the emitter can resolve method calls on the result of property accesses.
    /// </summary>
    public static string? GetPropertyReturnType(string csTypeName, string propertyName)
    {
        string key = $"{csTypeName}.{propertyName}";
        return PropertyReturnTypes.TryGetValue(key, out var t) ? t : null;
    }

    // ── Public lookup API ─────────────────────────────────────────────────────

    /// <summary>
    /// Looks up a method mapping given the C# type and method name.
    /// Returns null if no mapping exists (will be passed through as-is).
    /// </summary>
    public static MethodMapping? GetMethod(string csTypeName, string methodName)
    {
        var table = GetTableForType(csTypeName);
        return table?.TryGetValue(methodName, out var m) == true ? m : null;
    }

    /// <summary>
    /// Looks up a property mapping given the C# type and property name.
    /// Returns null if no mapping exists.
    /// </summary>
    public static string? GetProperty(string csTypeName, string propertyName)
    {
        string key = $"{csTypeName}.{propertyName}";
        return Properties.TryGetValue(key, out var v) ? v : null;
    }

    /// <summary>
    /// Looks up a static method call by its full C# name e.g. "Console.WriteLine".
    /// Tries arg-count-specific overload first (e.g. "McRegistry.RegisterPickaxe/2"),
    /// then falls back to the generic entry.
    /// </summary>
    public static string? GetStatic(string fullName, int argCount = -1)
    {
        if (argCount >= 0)
        {
            string key = $"{fullName}/{argCount}";
            if (StaticMethods.TryGetValue(key, out var specific))
                return specific;
        }
        return StaticMethods.TryGetValue(fullName, out var v) ? v : null;
    }

    /// <summary>
    /// Looks up a constructor mapping by C# type name e.g. "BlockPos".
    /// </summary>
    public static string? GetConstructor(string csTypeName)
        => Constructors.TryGetValue(csTypeName, out var v) ? v : null;

    /// <summary>
    /// Applies a mapping format string, substituting {target} and {0},{1}...
    /// </summary>
    public static string Apply(string template, string target, params string[] args)
    {
        string result = template.Replace("{target}", target);
        for (int i = 0; i < args.Length; i++)
            result = result.Replace($"{{{i}}}", args[i]);
        return result;
    }

    private static Dictionary<string, MethodMapping>? GetTableForType(string csType) => csType switch
    {
        "McPlayer" or "PlayerEntity" or "McPlayerEntity"    => MergeTables(PlayerMethods, PlayerMethodsExtra),
        "McWorld"  or "ServerWorld"  or "World"             => MergeTables(WorldMethods, WorldMethodsExtra),
        "McServer" or "MinecraftServer"                     => MergeTables(ServerMethods, ServerMethodsExtra),
        "McCommandSource" or "ServerCommandSource"          => CommandSourceMethods,
        "McEntity" or "Entity" or "LivingEntity"           => EntityMethods,
        "BlockPos" or "McBlockPos"                         => BlockPosMethods,
        "ItemStack" or "McItemStack"                       => ItemStackMethods,
        "McNbt" or "NbtCompound"                           => NbtMethods,
        "McBossBar" or "ServerBossBar"                     => BossBarMethods,
        "McInventory" or "Inventory"                       => InventoryMethods,
        "McBlockEntity" or "BlockEntity"                   => BlockEntityMethods,
        _ when csType.StartsWith("McGameRule")             => GameRuleMethods,
        _ => null
    };

    private static Dictionary<string, MethodMapping> MergeTables(
        Dictionary<string, MethodMapping> primary,
        Dictionary<string, MethodMapping> extra)
    {
        var merged = new Dictionary<string, MethodMapping>(primary);
        foreach (var kvp in extra) if (!merged.ContainsKey(kvp.Key)) merged[kvp.Key] = kvp.Value;
        return merged;
    }
}

/// <summary>
/// A method mapping: the Java template string and any extra imports needed.
/// </summary>
public record MethodMapping(string Template, string[]? Imports = null);