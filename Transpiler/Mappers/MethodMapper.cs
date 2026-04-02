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

    // ── Static method calls (Math.X, Console.X, etc.) ────────────────────────

    public static readonly Dictionary<string, string> StaticMethods = new()
    {
        ["Console.WriteLine"]   = "LOGGER.info({0})",
        ["Console.Write"]       = "LOGGER.info({0})",
        ["Console.Error"]       = "LOGGER.error({0})",
        ["Math.Abs"]            = "Math.abs({0})",
        ["Math.Floor"]          = "(int)Math.floor({0})",
        ["Math.Ceiling"]        = "(int)Math.ceil({0})",
        ["Math.Round"]          = "Math.round({0})",
        ["Math.Max"]            = "Math.max({0}, {1})",
        ["Math.Min"]            = "Math.min({0}, {1})",
        ["Math.Sqrt"]           = "Math.sqrt({0})",
        ["Math.Pow"]            = "Math.pow({0}, {1})",
        ["Math.Clamp"]          = "Math.max({2}, Math.min({1}, {0}))",
        ["string.IsNullOrEmpty"]= "({0} == null || {0}.isEmpty())",
        ["string.Format"]       = "String.format({0}, {1})",
        ["string.Join"]         = "String.join({0}, {1})",
        ["Guid.NewGuid"]        = "UUID.randomUUID()",
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
        "McPlayer" or "PlayerEntity" or "McPlayerEntity"    => PlayerMethods,
        "McWorld"  or "ServerWorld"  or "World"             => WorldMethods,
        "McServer" or "MinecraftServer"                     => ServerMethods,
        "BlockPos"                                          => BlockPosMethods,
        "ItemStack"                                         => ItemStackMethods,
        _ => null
    };
}

/// <summary>
/// A method mapping: the Java template string and any extra imports needed.
/// </summary>
public record MethodMapping(string Template, string[]? Imports = null);