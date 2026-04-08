using System.Text.RegularExpressions;

namespace CSCraft.Detranspiler.Maps;

/// <summary>
/// Maps Java method call patterns back to CSCraft C# API calls.
/// Patterns use {T} for the target object and {0},{1},{2} for arguments.
/// </summary>
public static class ReverseMethodMapper
{
    public record MethodPattern(Regex JavaRegex, string CsTemplate);

    // Order matters — more specific patterns first
    public static readonly MethodPattern[] Patterns =
    [
        // ── Text.literal unwrapping ──────────────────────────────────────────
        // Text.literal(X) → X  (used inline in many patterns below)

        // ── Player messaging ──────────────────────────────────────────────────
        new(Rx(@"(\w+)\.sendMessage\(Text\.literal\((.+?)\),\s*true\)"),             "$1.SendMessage($2, true)"),
        new(Rx(@"(\w+)\.sendMessage\(Text\.literal\((.+?)\)\)"),                     "$1.SendMessage($2)"),
        new(Rx(@"(\w+)\.sendFeedback\(\(\)\s*->\s*Text\.literal\((.+?)\),\s*\w+\)"), "$1.Respond($2)"),
        new(Rx(@"(\w+)\.sendError\(Text\.literal\((.+?)\)\)"),                       "$1.RespondError($2)"),

        // ── Player stats ──────────────────────────────────────────────────────
        new(Rx(@"(\w+)\.heal\((\w+)\.getMaxHealth\(\)\)"),   "$1.Heal($1.MaxHealth)"),
        new(Rx(@"(\w+)\.heal\((.+?)\)"),                     "$1.Heal($2)"),
        new(Rx(@"(\w+)\.damage\((.+?),\s*(.+?)\)"),         "$1.Damage($3)"),
        new(Rx(@"(\w+)\.getHealth\(\)"),                     "$1.Health"),
        new(Rx(@"(\w+)\.getMaxHealth\(\)"),                  "$1.MaxHealth"),
        new(Rx(@"(\w+)\.setHealth\((.+?)\)"),                "$1.Health = $2"),
        new(Rx(@"(\w+)\.getHungerManager\(\)\.getFoodLevel\(\)"), "$1.FoodLevel"),
        new(Rx(@"(\w+)\.addExperience\((.+?)\)"),            "$1.GiveXp($2)"),
        new(Rx(@"(\w+)\.experienceLevel"),                   "$1.XpLevel"),
        new(Rx(@"(\w+)\.clearStatusEffects\(\)"),            "$1.ClearEffects()"),
        new(Rx(@"(\w+)\.isSneaking\(\)"),                    "$1.IsSneaking"),
        new(Rx(@"(\w+)\.isBlocking\(\)"),                    "$1.IsBlocking"),
        new(Rx(@"(\w+)\.getAbilities\(\)\.flying"),          "$1.IsFlying"),
        new(Rx(@"(\w+)\.isSpectator\(\)"),                   "$1.IsSpectator"),
        new(Rx(@"(\w+)\.isSleeping\(\)"),                    "$1.IsSleeping"),
        new(Rx(@"(\w+)\.networkHandler\.getLatency\(\)"),    "$1.Ping"),
        new(Rx(@"(\w+)\.interactionManager\.getGameMode\(\)\.getName\(\)"), "$1.GameMode"),
        new(Rx(@"(\w+)\.changeGameMode\(GameMode\.byName\((.+?)\)\)"),       "$1.SetGameMode($2)"),

        // ── Player name / uuid ────────────────────────────────────────────────
        new(Rx(@"(\w+)\.getName\(\)\.getString\(\)"),   "$1.Name"),
        new(Rx(@"(\w+)\.getUuidAsString\(\)"),          "$1.Uuid"),

        // ── Player position ───────────────────────────────────────────────────
        new(Rx(@"(\w+)\.getX\(\)"),     "$1.X"),
        new(Rx(@"(\w+)\.getY\(\)"),     "$1.Y"),
        new(Rx(@"(\w+)\.getZ\(\)"),     "$1.Z"),
        new(Rx(@"(\w+)\.getBlockPos\(\)"), "$1.BlockPos"),
        new(Rx(@"(\w+)\.teleport\([\w().\s,]+,\s*(.+?),\s*(.+?),\s*(.+?),\s*.+?,\s*.+?\)"),
            "$1.Teleport($2, $3, $4)"),

        // ── Player effects ────────────────────────────────────────────────────
        new(Rx(@"(\w+)\.addStatusEffect\(new StatusEffectInstance\(Registries\.STATUS_EFFECT\.getEntry\(Identifier\.of\((.+?)\)\)\.get\(\),\s*(\d+),\s*(\d+)\)\)"),
            "$1.AddEffect($2, $3, $4)"),

        // ── Player inventory / items ──────────────────────────────────────────
        new(Rx(@"(\w+)\.getInventory\(\)\.insertStack\(new ItemStack\(Registries\.ITEM\.get\(Identifier\.of\((.+?)\)\),\s*(.+?)\)\)"),
            "McInventory.Give($1, new McItemStack($2, $3))"),
        new(Rx(@"(\w+)\.getMainHandStack\(\)"), "$1.MainHandItem"),
        new(Rx(@"(\w+)\.getOffHandStack\(\)"),  "$1.OffHandItem"),
        new(Rx(@"(\w+)\.getEquippedStack\(net\.minecraft\.entity\.EquipmentSlot\.HEAD\)"),   "$1.Helmet"),
        new(Rx(@"(\w+)\.getEquippedStack\(net\.minecraft\.entity\.EquipmentSlot\.CHEST\)"),  "$1.Chestplate"),
        new(Rx(@"(\w+)\.getEquippedStack\(net\.minecraft\.entity\.EquipmentSlot\.LEGS\)"),   "$1.Leggings"),
        new(Rx(@"(\w+)\.getEquippedStack\(net\.minecraft\.entity\.EquipmentSlot\.FEET\)"),   "$1.Boots"),

        // ── ItemStack ─────────────────────────────────────────────────────────
        new(Rx(@"Registries\.ITEM\.getId\((\w+)\.getItem\(\)\)\.toString\(\)"), "$1.Id"),
        new(Rx(@"(\w+)\.getCount\(\)"),  "$1.Count"),
        new(Rx(@"(\w+)\.isEmpty\(\)"),   "$1.IsEmpty"),

        // ── Sound ─────────────────────────────────────────────────────────────
        new(Rx(@"(\w+)\.playSoundToPlayer\(Registries\.SOUND_EVENT\.get\(Identifier\.of\((.+?)\)\),\s*\w+\.PLAYERS,\s*(.+?),\s*(.+?)\)"),
            "$1.PlaySound($2, $3, $4)"),
        new(Rx(@"(\w+)\.playSound\(.+?Identifier\.of\((.+?)\)\),\s*\w+,\s*(.+?),\s*(.+?)\)"),
            "$1.PlaySound($2, $3, $4)"),

        // ── Server ────────────────────────────────────────────────────────────
        new(Rx(@"(\w+)\.getPlayerManager\(\)\.getPlayerList\(\)"),        "$1.OnlinePlayers"),
        new(Rx(@"(\w+)\.getPlayerManager\(\)\.getCurrentPlayerCount\(\)"),"$1.PlayerCount"),
        new(Rx(@"(\w+)\.getPlayerManager\(\)\.getMaxPlayerCount\(\)"),    "$1.MaxPlayers"),
        new(Rx(@"(\w+)\.getAverageTickTime\(\)"),  "$1.Tps"),
        new(Rx(@"(\w+)\.getVersion\(\)"),          "$1.Version"),
        new(Rx(@"(\w+)\.isRunning\(\)"),           "$1.IsRunning"),
        new(Rx(@"(\w+)\.isHardcore\(\)"),          "$1.IsHardcore"),
        new(Rx(@"(\w+)\.getServerMotd\(\)"),       "$1.Motd"),
        new(Rx(@"(\w+)\.getPlayerManager\(\)\.broadcastMessageToOperators\(Text\.literal\((.+?)\)\)"),
            "$1.BroadcastOps($2)"),
        new(Rx(@"(\w+)\.getPlayerManager\(\)\.broadcast\(Text\.literal\((.+?)\),\s*false\)"),
            "$1.Broadcast($2)"),

        // ── World ─────────────────────────────────────────────────────────────
        new(Rx(@"(\w+)\.setBlockState\((.+?),\s*Registries\.BLOCK\.get\(Identifier\.of\((.+?)\)\)\.getDefaultState\(\)\)"),
            "$1.SetBlock($2, $3)"),
        new(Rx(@"(\w+)\.getBlockState\((.+?)\)"),         "$1.GetBlockState($2)"),
        new(Rx(@"(\w+)\.setTimeOfDay\((.+?)\)"),          "$1.SetTime($2)"),
        new(Rx(@"(\w+)\.setWeather\(\d+,\s*\d+,\s*true"),  "$1.SetRaining(true)"),
        new(Rx(@"(\w+)\.setWeather\(\d+,\s*\d+,\s*false"), "$1.SetRaining(false)"),
        new(Rx(@"(\w+)\.spawnEntity\((.+?)\)"),            "$1.SpawnEntity($2)"),
        new(Rx(@"(\w+)\.getSpawnPos\(\)"),                 "$1.SpawnPos"),
        new(Rx(@"(\w+)\.isDay\(\)"),                       "$1.IsDay"),
        new(Rx(@"(\w+)\.isRaining\(\)"),                   "$1.IsRaining"),
        new(Rx(@"(\w+)\.isThundering\(\)"),                "$1.IsThundering"),
        new(Rx(@"\(\(ServerWorld\)(\w+)\.getWorld\(\)\)"), "$1.World"),
        new(Rx(@"(\w+)\.getWorld\(\)"),                    "$1.World"),

        // ── Entity ────────────────────────────────────────────────────────────
        new(Rx(@"EntityType\.getId\((\w+)\.getType\(\)\)\.toString\(\)"), "$1.TypeId"),
        new(Rx(@"(\w+)\.isAlive\(\)"),      "$1.IsAlive"),
        new(Rx(@"(\w+)\.isOnGround\(\)"),   "$1.IsOnGround"),
        new(Rx(@"(\w+)\.isInvisible\(\)"),  "$1.IsInvisible"),
        new(Rx(@"(\w+)\.remove\(\w+\)"),    "$1.Remove()"),

        // ── Registry → McRegistry calls ───────────────────────────────────────
        new(Rx(@"Registry\.register\(Registries\.BLOCK,\s*Identifier\.of\((.+?)\),\s*new Block\((.+?)\)\)"),
            "McRegistry.RegisterBlock($1, $2)"),
        new(Rx(@"Registry\.register\(Registries\.ITEM,\s*Identifier\.of\((.+?)\),\s*new BlockItem\((.+?),\s*new Item\.Settings\(\)\)\)"),
            "McRegistry.RegisterBlockItem($2, $1)"),
        new(Rx(@"Registry\.register\(Registries\.ITEM,\s*Identifier\.of\((.+?)\),\s*new SwordItem\(ToolMaterials\.(\w+),.+?\)\);?"),
            "McRegistry.RegisterSword($1, McToolMaterial.$2);"),
        new(Rx(@"Registry\.register\(Registries\.ITEM,\s*Identifier\.of\((.+?)\),\s*new PickaxeItem\(ToolMaterials\.(\w+),.+?\)\);?"),
            "McRegistry.RegisterPickaxe($1, McToolMaterial.$2);"),
        new(Rx(@"Registry\.register\(Registries\.ITEM,\s*Identifier\.of\((.+?)\),\s*new AxeItem\(ToolMaterials\.(\w+),.+?\)\);?"),
            "McRegistry.RegisterAxe($1, McToolMaterial.$2);"),
        new(Rx(@"Registry\.register\(Registries\.ITEM,\s*Identifier\.of\((.+?)\),\s*new ShovelItem\(ToolMaterials\.(\w+),.+?\)\);?"),
            "McRegistry.RegisterShovel($1, McToolMaterial.$2);"),
        new(Rx(@"Registry\.register\(Registries\.ITEM,\s*Identifier\.of\((.+?)\),\s*new HoeItem\(ToolMaterials\.(\w+),.+?\)\);?"),
            "McRegistry.RegisterHoe($1, McToolMaterial.$2);"),
        new(Rx(@"Registry\.register\(Registries\.ITEM,\s*Identifier\.of\((.+?)\),\s*new ArmorItem\(ArmorMaterials\.(\w+),\s*ArmorItem\.Type\.HELMET.+?\)\);?"),
            "McRegistry.RegisterHelmet($1, McArmorMaterial.$2);"),
        new(Rx(@"Registry\.register\(Registries\.ITEM,\s*Identifier\.of\((.+?)\),\s*new ArmorItem\(ArmorMaterials\.(\w+),\s*ArmorItem\.Type\.CHESTPLATE.+?\)\);?"),
            "McRegistry.RegisterChestplate($1, McArmorMaterial.$2);"),
        new(Rx(@"Registry\.register\(Registries\.ITEM,\s*Identifier\.of\((.+?)\),\s*new ArmorItem\(ArmorMaterials\.(\w+),\s*ArmorItem\.Type\.LEGGINGS.+?\)\);?"),
            "McRegistry.RegisterLeggings($1, McArmorMaterial.$2);"),
        new(Rx(@"Registry\.register\(Registries\.ITEM,\s*Identifier\.of\((.+?)\),\s*new ArmorItem\(ArmorMaterials\.(\w+),\s*ArmorItem\.Type\.BOOTS.+?\)\);?"),
            "McRegistry.RegisterBoots($1, McArmorMaterial.$2);"),
        new(Rx(@"Registry\.register\(Registries\.ITEM,\s*Identifier\.of\((.+?)\),\s*new Item\(new Item\.Settings\(\)\.food\(.+?\)\)\);?"),
            "McRegistry.RegisterFood($1, /* nutrition, saturation */);"),
        new(Rx(@"Registry\.register\(Registries\.ITEM,\s*Identifier\.of\((.+?)\),\s*new Item\(new Item\.Settings\(\)\)\);?"),
            "McRegistry.RegisterItem($1);"),
        new(Rx(@"Registry\.register\(Registries\.SOUND_EVENT,\s*Identifier\.of\((.+?)\),\s*SoundEvent\.of\(.+?\)\);?"),
            "McRegistry.RegisterSound($1);"),

        // ── Creative tabs ─────────────────────────────────────────────────────
        new(Rx(@"ItemGroupEvents\.modifyEntriesEvent\(ItemGroups\.COMBAT\)\.register\(e\s*->\s*e\.add\((.+?)\)\)"),       "McCreativeTab.AddToCombat($1)"),
        new(Rx(@"ItemGroupEvents\.modifyEntriesEvent\(ItemGroups\.TOOLS\)\.register\(e\s*->\s*e\.add\((.+?)\)\)"),        "McCreativeTab.AddToTools($1)"),
        new(Rx(@"ItemGroupEvents\.modifyEntriesEvent\(ItemGroups\.INGREDIENTS\)\.register\(e\s*->\s*e\.add\((.+?)\)\)"),  "McCreativeTab.AddToIngredients($1)"),
        new(Rx(@"ItemGroupEvents\.modifyEntriesEvent\(ItemGroups\.NATURAL\)\.register\(e\s*->\s*e\.add\((.+?)\)\)"),      "McCreativeTab.AddToNaturalBlocks($1)"),
        new(Rx(@"ItemGroupEvents\.modifyEntriesEvent\(ItemGroups\.BUILDING_BLOCKS\)\.register\(e\s*->\s*e\.add\((.+?)\)\)"), "McCreativeTab.AddToBuildingBlocks($1)"),
        new(Rx(@"ItemGroupEvents\.modifyEntriesEvent\(ItemGroups\.FOOD_AND_DRINK\)\.register\(e\s*->\s*e\.add\((.+?)\)\)"), "McCreativeTab.AddToFood($1)"),

        // ── Boss bar ──────────────────────────────────────────────────────────
        new(Rx(@"new ServerBossBar\(Text\.literal\((.+?)\),\s*BossBar\.Color\.(\w+),\s*BossBar\.Style\.(\w+)\)"),
            "new McBossBar($1, McBossBarColor.$2, McBossBarStyle.$3)"),
        new(Rx(@"(\w+)\.addPlayer\((.+?)\)"),     "$1.AddPlayer($2)"),
        new(Rx(@"(\w+)\.removePlayer\((.+?)\)"),  "$1.RemovePlayer($2)"),
        new(Rx(@"(\w+)\.setName\(Text\.literal\((.+?)\)\)"),  "$1.SetTitle($2)"),
        new(Rx(@"(\w+)\.setPercent\((.+?)\)"),    "$1.SetProgress($2)"),
        new(Rx(@"(\w+)\.setVisible\((.+?)\)"),    "$1.SetVisible($2)"),

        // ── Scoreboard ────────────────────────────────────────────────────────
        new(Rx(@"(\w+)\.getScoreboard\(\)\.getOrCreateScore\((.+?),\s*.+?getNullableObjective\((.+?)\)\)\.setScore\((.+?)\)"),
            "McScoreboard.SetScore($1, $2, $3, $4)"),
        new(Rx(@"(\w+)\.getScoreboard\(\)\.getOrCreateScore\((.+?),\s*.+?getNullableObjective\((.+?)\)\)\.getScore\(\)"),
            "McScoreboard.GetScore($1, $2, $3)"),

        // ── Particles ─────────────────────────────────────────────────────────
        new(Rx(@"(\w+)\.spawnParticles\(.+?Identifier\.of\((.+?)\)\),\s*(.+?),\s*(.+?),\s*(.+?),\s*(\d+),"),
            "$1.SpawnParticle($2, $3, $4, $5, $6)"),

        // ── Title / subtitle ──────────────────────────────────────────────────
        new(Rx(@"(\w+)\.networkHandler\.sendPacket\(new net\.minecraft\.network\.packet\.s2c\.play\.TitleS2CPacket\(Text\.literal\((.+?)\)\)\)"),
            "$1.ShowTitle($2)"),
        new(Rx(@"(\w+)\.networkHandler\.sendPacket\(new net\.minecraft\.network\.packet\.s2c\.play\.SubtitleS2CPacket\(Text\.literal\((.+?)\)\)\)"),
            "$1.ShowSubtitle($2)"),

        // ── ModPlayerData ─────────────────────────────────────────────────────
        new(Rx(@"ModPlayerData\.getPlayerNbt\((\w+)\)\.getInt\((.+?)\)"),    "PlayerData.GetInt($1, $2, 0)"),
        new(Rx(@"ModPlayerData\.getPlayerNbt\((\w+)\)\.getString\((.+?)\)"), "PlayerData.GetString($1, $2, \"\")"),
        new(Rx(@"ModPlayerData\.getPlayerNbt\((\w+)\)\.getBoolean\((.+?)\)"), "PlayerData.GetBool($1, $2, false)"),
        new(Rx(@"ModPlayerData\.getPlayerNbt\((\w+)\)\.putInt\((.+?),\s*(.+?)\)"), "PlayerData.Set($1, $2, $3)"),

        // ── Console / logging ─────────────────────────────────────────────────
        new(Rx(@"LOGGER\.info\((.+?)\)"),   "Console.WriteLine($1)"),
        new(Rx(@"LOGGER\.error\((.+?)\)"),  "Console.Error($1)"),

        // ── Misc ──────────────────────────────────────────────────────────────
        new(Rx(@"(\w+)\.getServer\(\)"),    "$1.getServer() /* $1.Server */"),
        new(Rx(@"String\.valueOf\((.+?)\)"), "$1.ToString()"),
    ];

    private static Regex Rx(string pattern) =>
        new(pattern, RegexOptions.Compiled | RegexOptions.Singleline);

    /// <summary>
    /// Applies all patterns to a Java expression string, returning the best C# translation.
    /// Returns null if no pattern matched.
    /// </summary>
    public static string? TryTranslate(string javaExpr)
    {
        foreach (var (regex, template) in Patterns)
        {
            var m = regex.Match(javaExpr);
            if (m.Success)
                return regex.Replace(javaExpr, template);
        }
        return null;
    }

    /// <summary>
    /// Translates a Java expression, returning a TODO comment if nothing matched.
    /// </summary>
    public static string Translate(string javaExpr)
    {
        return TryTranslate(javaExpr) ?? javaExpr;
    }

    /// <summary>Strips Text.literal(...) wrappers so inner value is exposed.</summary>
    public static string UnwrapTextLiteral(string expr)
    {
        var m = Regex.Match(expr, @"^Text\.literal\((.+)\)$", RegexOptions.Singleline);
        return m.Success ? m.Groups[1].Value : expr;
    }
}
