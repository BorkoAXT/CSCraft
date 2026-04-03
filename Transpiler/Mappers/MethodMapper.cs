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
        ["Heal"]            = new("{target}.heal({0})"),
        ["Damage"]          = new("{target}.damage(server.getDamageSources().generic(), {0})"),

        // Movement & position
        ["Teleport"]        = new("{target}.teleport((ServerWorld)server.getWorld(World.OVERWORLD), {0}, {1}, {2}, 0f, 0f)",
                                  Imports: ["net.minecraft.server.world.ServerWorld", "net.minecraft.world.World"]),
        ["GetX"]            = new("{target}.getX()"),
        ["GetY"]            = new("{target}.getY()"),
        ["GetZ"]            = new("{target}.getZ()"),
        ["GetBlockPos"]     = new("{target}.getBlockPos()"),

        // Items & inventory
        ["GiveItem"]        = new("{target}.getInventory().insertStack(new ItemStack(Registries.ITEM.get(new Identifier({0})), {1}))",
                                  Imports: ["net.minecraft.item.ItemStack", "net.minecraft.registry.Registries", "net.minecraft.util.Identifier"]),
        ["ClearInventory"]  = new("{target}.getInventory().clear()"),

        // Effects
        ["GiveEffect"]      = new("{target}.addStatusEffect(new StatusEffectInstance(Registries.STATUS_EFFECT.get(new Identifier({0})), {1}, {2}))",
                                  Imports: ["net.minecraft.entity.effect.StatusEffectInstance", "net.minecraft.registry.Registries", "net.minecraft.util.Identifier"]),
        ["RemoveEffect"]    = new("{target}.removeStatusEffect(Registries.STATUS_EFFECT.get(new Identifier({0})))"),
        ["ClearEffects"]    = new("{target}.clearStatusEffects()"),

        // XP
        ["GiveXp"]          = new("{target}.addExperience({0})"),
        ["GetXpLevel"]      = new("{target}.experienceLevel"),
        ["SetXpLevel"]      = new("{target}.setExperienceLevel({0})"),

        // Game mode
        ["SetGameMode"]     = new("{target}.changeGameMode(GameMode.byName({0}))",
                                  Imports: ["net.minecraft.world.GameMode"]),
        ["GetGameMode"]     = new("{target}.interactionManager.getGameMode().getName()"),

        // Kick / permissions
        ["Kick"]            = new("{target}.networkHandler.disconnect(Text.literal({0}))"),
        ["IsOp"]            = new("{target}.hasPermissionLevel(2)"),

        // Misc
        ["PlaySound"]       = new("{target}.playSoundToPlayer(Registries.SOUND_EVENT.get(new Identifier({0})), SoundCategory.PLAYERS, 1.0f, 1.0f)",
                                  Imports: ["net.minecraft.sound.SoundCategory"]),
        ["SetSpawn"]        = new("{target}.setSpawnPoint(World.OVERWORLD, new BlockPos({0},{1},{2}), 0f, true, false)"),
    };

    // ── McWorld (ServerWorld) ────────────────────────────────────────────────

    private static readonly Dictionary<string, MethodMapping> WorldMethods = new()
    {
        // Blocks
        ["SetBlock"]        = new("{target}.setBlockState(new BlockPos({0},{1},{2}), Registries.BLOCK.get(new Identifier({3})).getDefaultState())",
                                  Imports: ["net.minecraft.registry.Registries", "net.minecraft.util.Identifier", "net.minecraft.util.math.BlockPos"]),
        ["GetBlock"]        = new("Registries.BLOCK.getId({target}.getBlockState(new BlockPos({0},{1},{2})).getBlock()).toString()"),
        ["BreakBlock"]      = new("{target}.breakBlock(new BlockPos({0},{1},{2}), true)"),
        ["FillBlocks"]      = new("/* fill {0},{1},{2} to {3},{4},{5} with {6} */"), // complex — left as comment

        // Entities
        ["SpawnEntity"]     = new("/* spawnEntity {0} at {1},{2},{3} */"),           // requires EntityType registry lookup
        ["GetEntities"]     = new("{target}.getEntitiesByType(TypeFilter.instanceOf(Entity.class), e -> true)"),
        ["GetNearbyPlayers"]= new("{target}.getPlayers()"),

        // World info
        ["GetTime"]         = new("{target}.getTime()"),
        ["SetTime"]         = new("{target}.setTimeOfDay({0})"),
        ["IsRaining"]       = new("{target}.isRaining()"),
        ["SetWeather"]      = new("{target}.setWeather({0}, {1}, {2}, {3})"),
        ["GetDifficulty"]   = new("{target}.getDifficulty().getName()"),

        // Explosions
        ["CreateExplosion"] = new("{target}.createExplosion(null, {0}, {1}, {2}, {3}, World.ExplosionSourceType.NONE)"),

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
        ["GetTps"]              = new("{target}.getTickTime()"),
        ["IsRunning"]           = new("{target}.isRunning()"),
        ["GetVersion"]          = new("{target}.getVersion()"),
        ["Shutdown"]            = new("{target}.stop(false)"),
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
        ["GetCount"]    = new("{target}.getCount()"),
        ["SetCount"]    = new("{target}.setCount({0})"),
        ["GetItem"]     = new("Registries.ITEM.getId({target}.getItem()).toString()"),
        ["IsEmpty"]     = new("{target}.isEmpty()"),
        ["HasNbt"]      = new("{target}.hasNbt()"),
        ["GetNbt"]      = new("{target}.getNbt()"),
        ["Copy"]        = new("{target}.copy()"),
    };

    // ── Static constructors (new XYZ(...) in C# → Java factory/constructor) ──

    public static readonly Dictionary<string, string> Constructors = new()
    {
        // new BlockPos(x, y, z) → new BlockPos(x, y, z)  (same!)
        ["BlockPos"]        = "new BlockPos({0}, {1}, {2})",
        // new ItemStack("minecraft:diamond", 1) → new ItemStack(Registries.ITEM.get(...), 1)
        ["ItemStack"]       = "new ItemStack(Registries.ITEM.get(new Identifier({0})), {1})",
        // new McText("hello") → Text.literal("hello")
        ["McText"]          = "Text.literal({0})",
        ["ChatMessage"]     = "Text.literal({0})",
        // new McIdentifier("minecraft:stone") → new Identifier("minecraft:stone")
        ["McIdentifier"]    = "new Identifier({0})",
        ["ResourceLocation"]= "new Identifier({0})",
    };

    // ── McPlayer extended methods ─────────────────────────────────────────────

    private static readonly Dictionary<string, MethodMapping> PlayerMethodsExtra = new()
    {
        ["GetBiome"]        = new("{target}.getWorld().getBiome({target}.getBlockPos()).getKey().map(k -> k.getValue().toString()).orElse(\"unknown\")"),
        ["GetDimension"]    = new("{target}.getWorld().getRegistryKey().getValue().toString()"),
        ["GetNbtString"]    = new("{target}.getCustomData().getString({0})"),
        ["SetNbtString"]    = new("{target}.getCustomData().putString({0}, {1})"),
        ["GetNbtInt"]       = new("{target}.getCustomData().getInt({0})"),
        ["SetNbtInt"]       = new("{target}.getCustomData().putInt({0}, {1})"),
        ["HasNbt"]          = new("{target}.getCustomData().contains({0})"),
    };

    // ── McWorld extended methods ──────────────────────────────────────────────

    private static readonly Dictionary<string, MethodMapping> WorldMethodsExtra = new()
    {
        ["GetBlockState"]   = new("{target}.getBlockState(new BlockPos({0},{1},{2}))",
                                  Imports: ["net.minecraft.util.math.BlockPos"]),
        ["GetTopY"]         = new("{target}.getTopY(Heightmap.Type.WORLD_SURFACE, {0}, {1})",
                                  Imports: ["net.minecraft.world.Heightmap"]),
        ["IsInBorder"]      = new("{target}.getWorldBorder().contains(new BlockPos({0},{1},{2}))"),
        ["PlaySound"]       = new("{target}.playSound(null, new BlockPos((int){1}, (int){2}, (int){3}), Registries.SOUND_EVENT.get(new Identifier({0})), SoundCategory.BLOCKS, 1.0f, 1.0f)",
                                  Imports: ["net.minecraft.sound.SoundCategory", "net.minecraft.registry.Registries", "net.minecraft.util.Identifier"]),
        ["SpawnParticle"]   = new("{target}.spawnParticles(Registries.PARTICLE_TYPE.get(new Identifier({0})), {1}, {2}, {3}, {4}, 0, 0, 0, 0)",
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
        ["McRegistry.RegisterBlock"]    = "Registry.register(Registries.BLOCK, new Identifier({0}), new Block(AbstractBlock.Settings.create().strength({1})))",
        ["McRegistry.RegisterBlockWithSettings"] = "Registry.register(Registries.BLOCK, new Identifier({0}), new Block({1}))",

        // Item registration
        ["McRegistry.RegisterItem"]         = "Registry.register(Registries.ITEM, new Identifier({0}), new Item(new Item.Settings()))",
        ["McRegistry.RegisterItemWithSettings"] = "Registry.register(Registries.ITEM, new Identifier({0}), new Item({1}))",
        ["McRegistry.RegisterBlockItem"]    = "Registry.register(Registries.ITEM, new Identifier({0}), new BlockItem({1}, new Item.Settings()))",

        // Tool/weapon registration
        ["McRegistry.RegisterSword"]    = "Registry.register(Registries.ITEM, new Identifier({0}), new SwordItem(ToolMaterials.{1}, {2}, {3}f, new Item.Settings()))",
        ["McRegistry.RegisterPickaxe"]  = "Registry.register(Registries.ITEM, new Identifier({0}), new PickaxeItem(ToolMaterials.{1}, {2}, {3}f, new Item.Settings()))",
        ["McRegistry.RegisterAxe"]      = "Registry.register(Registries.ITEM, new Identifier({0}), new AxeItem(ToolMaterials.{1}, {2}f, {3}f, new Item.Settings()))",
        ["McRegistry.RegisterShovel"]   = "Registry.register(Registries.ITEM, new Identifier({0}), new ShovelItem(ToolMaterials.{1}, {2}f, {3}f, new Item.Settings()))",
        ["McRegistry.RegisterHoe"]      = "Registry.register(Registries.ITEM, new Identifier({0}), new HoeItem(ToolMaterials.{1}, {2}, {3}f, new Item.Settings()))",

        // Food registration
        ["McRegistry.RegisterFood"]     = "Registry.register(Registries.ITEM, new Identifier({0}), new Item(new Item.Settings().food(new FoodComponent.Builder().nutrition({1}).saturationModifier({2}).build())))",

        // Armor registration
        ["McRegistry.RegisterHelmet"]       = "Registry.register(Registries.ITEM, new Identifier({0}), new ArmorItem(ArmorMaterials.{1}, EquipmentSlot.HEAD, new Item.Settings()))",
        ["McRegistry.RegisterChestplate"]   = "Registry.register(Registries.ITEM, new Identifier({0}), new ArmorItem(ArmorMaterials.{1}, EquipmentSlot.CHEST, new Item.Settings()))",
        ["McRegistry.RegisterLeggings"]     = "Registry.register(Registries.ITEM, new Identifier({0}), new ArmorItem(ArmorMaterials.{1}, EquipmentSlot.LEGS, new Item.Settings()))",
        ["McRegistry.RegisterBoots"]        = "Registry.register(Registries.ITEM, new Identifier({0}), new ArmorItem(ArmorMaterials.{1}, EquipmentSlot.FEET, new Item.Settings()))",

        // Sound registration
        ["McRegistry.RegisterSound"]    = "Registry.register(Registries.SOUND_EVENT, new Identifier({0}), SoundEvent.of(new Identifier({0})))",

        // Attribute registration
        ["McRegistry.RegisterAttribute"]= "Registry.register(Registries.ATTRIBUTE, new Identifier({0}), new ClampedEntityAttribute({0}, {1}, {2}, {3}))",

        // Enchantment helpers
        ["McEnchantment.GetLevel"]      = "EnchantmentHelper.getLevel(Registries.ENCHANTMENT.get(new Identifier({1})), {0})",
        ["McEnchantment.HasEnchantment"]= "EnchantmentHelper.getLevel(Registries.ENCHANTMENT.get(new Identifier({1})), {0}) > 0",

        // Block settings factory
        ["McBlockSettings.Create"]      = "AbstractBlock.Settings.create()",
        ["McBlockSettings.CopyOf"]      = "AbstractBlock.Settings.copyOf({0})",

        // Item settings factory
        ["McItemSettings.Create"]       = "new Item.Settings()",

        // Command registration (simplified wrappers)
        ["McCommand.Register"]          = "CommandRegistrationCallback.EVENT.register((dispatcher, registryAccess, environment) -> dispatcher.register(CommandManager.literal({0}).executes(ctx -> { {1}(new McCommandSourceWrapper(ctx.getSource())); return 1; })))",
        ["McCommand.RegisterOp"]        = "CommandRegistrationCallback.EVENT.register((dispatcher, registryAccess, environment) -> dispatcher.register(CommandManager.literal({0}).requires(src -> src.hasPermissionLevel(2)).executes(ctx -> { {1}(new McCommandSourceWrapper(ctx.getSource())); return 1; })))",

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
        ["McTag.BlockIsIn"]             = "{0}.getBlockState(new BlockPos({1},{2},{3})).isIn(net.minecraft.registry.tag.BlockTags.create(new Identifier({4})))",
        ["McTag.IsInTag"]               = "{0}.getDefaultState().isIn(net.minecraft.registry.tag.BlockTags.create(new Identifier({1})))",
        ["McTag.IsLog"]                 = "{0}.getBlockState(new BlockPos({1},{2},{3})).isIn(net.minecraft.registry.tag.BlockTags.LOGS)",
        ["McTag.IsLeaves"]              = "{0}.getBlockState(new BlockPos({1},{2},{3})).isIn(net.minecraft.registry.tag.BlockTags.LEAVES)",
        ["McTag.IsDirt"]                = "{0}.getBlockState(new BlockPos({1},{2},{3})).isIn(net.minecraft.registry.tag.BlockTags.DIRT)",
        ["McTag.IsStone"]               = "{0}.getBlockState(new BlockPos({1},{2},{3})).isIn(net.minecraft.registry.tag.BlockTags.STONE_ORE_REPLACEABLES)",
        ["McTag.ItemIsIn"]              = "{0}.isIn(net.minecraft.registry.tag.ItemTags.create(new Identifier({1})))",
        ["McTag.IsSword"]               = "{0}.isIn(net.minecraft.registry.tag.ItemTags.SWORDS)",
        ["McTag.IsPickaxe"]             = "{0}.isIn(net.minecraft.registry.tag.ItemTags.PICKAXES)",
        ["McTag.IsAxe"]                 = "{0}.isIn(net.minecraft.registry.tag.ItemTags.AXES)",
        ["McTag.IsFish"]                = "{0}.isIn(net.minecraft.registry.tag.ItemTags.FISHES)",
        ["McTag.EntityIsIn"]            = "{0}.getType().isIn(net.minecraft.registry.tag.EntityTypeTags.create(new Identifier({1})))",
        ["McTag.IsUndead"]              = "{0}.getType().isIn(net.minecraft.registry.tag.EntityTypeTags.UNDEAD)",
        ["McTag.CanBreatheUnderwater"]  = "{0}.getType().isIn(net.minecraft.registry.tag.EntityTypeTags.CAN_BREATHE_UNDER_WATER)",

        // ── McFluid ──────────────────────────────────────────────────────────
        ["McFluid.IsFluid"]             = "!{0}.getFluidState(new BlockPos({1},{2},{3})).isEmpty()",
        ["McFluid.IsWater"]             = "{0}.getFluidState(new BlockPos({1},{2},{3})).isIn(net.minecraft.registry.tag.FluidTags.WATER)",
        ["McFluid.IsLava"]              = "{0}.getFluidState(new BlockPos({1},{2},{3})).isIn(net.minecraft.registry.tag.FluidTags.LAVA)",
        ["McFluid.IsSource"]            = "{0}.getFluidState(new BlockPos({1},{2},{3})).isStill()",
        ["McFluid.GetLevel"]            = "{0}.getFluidState(new BlockPos({1},{2},{3})).getLevel()",
        ["McFluid.IsPlayerSubmerged"]   = "{0}.isSubmergedInWater()",
        ["McFluid.IsPlayerInLava"]      = "{0}.isInLava()",

        // ── McStructure ───────────────────────────────────────────────────────
        ["McStructure.IsInsideStructure"] = "{0}.hasStructure(new BlockPos({1},{2},{3}), Registries.STRUCTURE.getOrThrow(net.minecraft.registry.RegistryKey.of(net.minecraft.registry.RegistryKeys.STRUCTURE, new Identifier({4}))))",
        ["McStructure.FindNearest"]       = "{0}.locateStructure(Registries.STRUCTURE.getOrThrow(net.minecraft.registry.RegistryKey.of(net.minecraft.registry.RegistryKeys.STRUCTURE, new Identifier({1}))), new BlockPos((int){0}.getLevelProperties().getSpawnX(), 64, (int){0}.getLevelProperties().getSpawnZ()), 100, false)",
        ["McStructure.Place"]             = "{ var _structManager = {0}.getServer().getStructureTemplateManager(); var _template = _structManager.getTemplateOrBlank(new Identifier({4})); var _placement = new net.minecraft.structure.StructurePlacementData(); _template.place((ServerWorld){0}, new BlockPos({1},{2},{3}), new BlockPos({1},{2},{3}), _placement, {0}.getRandom(), 2); }",

        // ── McAdvancement ─────────────────────────────────────────────────────
        ["McAdvancement.Grant"]          = "{ var _adv = {0}.getServer().getAdvancementLoader().get(new Identifier({1})); if (_adv != null) { var _prog = {0}.getAdvancementTracker().getProgress(_adv); for (var _crit : _prog.getUnobtainedCriteria()) {0}.getAdvancementTracker().grantCriterion(_adv, _crit); } }",
        ["McAdvancement.Revoke"]         = "{ var _adv2 = {0}.getServer().getAdvancementLoader().get(new Identifier({1})); if (_adv2 != null) { var _prog2 = {0}.getAdvancementTracker().getProgress(_adv2); for (var _crit2 : _prog2.getObtainedCriteria()) {0}.getAdvancementTracker().revokeCriterion(_adv2, _crit2); } }",
        ["McAdvancement.HasCompleted"]   = "{ var _adv3 = {0}.getServer().getAdvancementLoader().get(new Identifier({1})); _adv3 != null && {0}.getAdvancementTracker().getProgress(_adv3).isDone(); }",
        ["McAdvancement.GrantCriterion"] = "{ var _adv4 = {0}.getServer().getAdvancementLoader().get(new Identifier({1})); if (_adv4 != null) {0}.getAdvancementTracker().grantCriterion(_adv4, {2}); }",

        // ── McLootTable ───────────────────────────────────────────────────────
        ["McLootTable.DropLoot"]         = "{ var _lootCtx = new net.minecraft.loot.context.LootContextParameterSet.Builder((ServerWorld){0}).add(net.minecraft.loot.context.LootContextParameters.ORIGIN, new net.minecraft.util.math.Vec3d({2},{3},{4})).build(net.minecraft.loot.context.LootContextTypes.CHEST); var _table = {0}.getServer().getReloadableRegistries().getLootTable(net.minecraft.registry.RegistryKey.of(net.minecraft.loot.LootTable.REGISTRY_KEY, new Identifier({1}))); _table.generateLoot(_lootCtx).forEach(s -> net.minecraft.entity.ItemEntity.spawn((ServerWorld){0}, new net.minecraft.util.math.BlockPos((int){2},(int){3},(int){4}), s)); }",
        ["McLootTable.GiveLootToPlayer"] = "{ var _lootCtx2 = new net.minecraft.loot.context.LootContextParameterSet.Builder((ServerWorld){0}.getWorld()).add(net.minecraft.loot.context.LootContextParameters.THIS_ENTITY, {0}).build(net.minecraft.loot.context.LootContextTypes.ENTITY); var _table2 = {0}.getServer().getReloadableRegistries().getLootTable(net.minecraft.registry.RegistryKey.of(net.minecraft.loot.LootTable.REGISTRY_KEY, new Identifier({1}))); _table2.generateLoot(_lootCtx2).forEach(s -> {0}.getInventory().insertStack(s)); }",

        // ── McPotion ──────────────────────────────────────────────────────────
        ["McPotion.GetPotionId"]             = "PotionUtil.getPotion({0}).getId(Registries.POTION)",
        ["McPotion.HasEffect"]               = "PotionUtil.getPotionEffects({0}).stream().anyMatch(e -> e.getEffectType() == Registries.STATUS_EFFECT.get(new Identifier({1})))",
        ["McPotion.GetEffects"]              = "PotionUtil.getPotionEffects({0})",
        ["McPotion.RegisterBrewingRecipe"]   = "BrewingRecipeRegistry.registerPotionRecipe(Registries.POTION.get(new Identifier({0})), {1}.asItem(), Registries.POTION.get(new Identifier({2})))",

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
        ["McRecipe.PlayerKnowsRecipe"]      = "{0}.getRecipeBook().contains(new Identifier({1}))",
        ["McRecipe.UnlockForPlayer"]        = "{0}.unlockRecipes(new net.minecraft.util.Identifier[] {{ new Identifier({1}) }})",
        ["McRecipe.LockForPlayer"]          = "{0}.lockRecipes(new net.minecraft.util.Identifier[] {{ new Identifier({1}) }})",

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
        ["McCreativeTab.AddToTab"]              = "ItemGroupEvents.modifyEntriesEvent(RegistryKey.of(RegistryKeys.ITEM_GROUP, new Identifier({0}))).register(e -> e.add({1}))",
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
        ["McPlayer.World"]          = "(ServerWorld){target}.getWorld()",
        ["McPlayer.Inventory"]      = "{target}.getInventory()",
        ["McPlayer.IsAlive"]        = "{target}.isAlive()",
        ["McPlayer.IsSneaking"]     = "{target}.isSneaking()",
        ["McPlayer.IsSprinting"]    = "{target}.isSprinting()",
        ["McPlayer.IsOnGround"]     = "{target}.isOnGround()",
        ["McPlayer.IsCreative"]     = "{target}.isCreative()",
        ["McPlayer.GameMode"]       = "{target}.interactionManager.getGameMode().getName()",

        // McWorld properties
        ["McWorld.Time"]            = "{target}.getTime()",
        ["McWorld.IsDay"]           = "{target}.isDay()",
        ["McWorld.IsNight"]         = "!{target}.isDay()",
        ["McWorld.IsRaining"]       = "{target}.isRaining()",
        ["McWorld.IsThundering"]    = "{target}.isThundering()",
        ["McWorld.Difficulty"]      = "{target}.getDifficulty().getName()",
        ["McWorld.SpawnPos"]        = "{target}.getSpawnPos()",

        // McServer properties
        ["McServer.OnlinePlayers"]  = "{target}.getPlayerManager().getPlayerList()",
        ["McServer.PlayerCount"]    = "{target}.getPlayerManager().getCurrentPlayerCount()",
        ["McServer.MaxPlayers"]     = "{target}.getPlayerManager().getMaxPlayerCount()",
        ["McServer.Tps"]            = "{target}.getTickTime()",
        ["McServer.Version"]        = "{target}.getVersion()",
        ["McServer.IsRunning"]      = "{target}.isRunning()",
        ["McServer.Motd"]           = "{target}.getServerMotd()",

        // BlockPos properties
        ["BlockPos.X"]              = "{target}.getX()",
        ["BlockPos.Y"]              = "{target}.getY()",
        ["BlockPos.Z"]              = "{target}.getZ()",

        // ItemStack properties
        ["ItemStack.Count"]         = "{target}.getCount()",
        ["ItemStack.IsEmpty"]       = "{target}.isEmpty()",
        ["ItemStack.HasNbt"]        = "{target}.hasNbt()",
    };

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
    /// </summary>
    public static string? GetStatic(string fullName)
        => StaticMethods.TryGetValue(fullName, out var v) ? v : null;

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
        "BlockPos"                                          => BlockPosMethods,
        "ItemStack"                                         => ItemStackMethods,
        _ => null
    };

    private static Dictionary<string, MethodMapping> MergeTables(
        Dictionary<string, MethodMapping> primary,
        Dictionary<string, MethodMapping> extra)
    {
        var merged = new Dictionary<string, MethodMapping>(primary);
        foreach (var (k, v) in extra) merged.TryAdd(k, v);
        return merged;
    }
}

/// <summary>
/// A method mapping: the Java template string and any extra imports needed.
/// </summary>
public record MethodMapping(string Template, string[]? Imports = null);