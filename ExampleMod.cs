using CSCraft;

[ModInfo(
    Id          = "testmod",
    Name        = "Test Mod",
    Version     = "1.0.0",
    Author      = "CSCraftTester",
    Description = "Comprehensive test mod for CSCraft transpiler",
    MinecraftVersion = "1.21.1"
)]
public class TestMod : IMod
{
    // ── Blocks ───────────────────────────────────────────────────────────────
    static McBlock rubyOre      = McRegistry.RegisterBlock("testmod:ruby_ore", 3.0f, McMineTool.Pickaxe, McMineLevel.Iron);
    static McBlock sapphireOre  = McRegistry.RegisterBlock("testmod:sapphire_ore", 4.0f, McMineTool.Pickaxe, McMineLevel.Diamond);
    static McBlock magicLog     = McRegistry.RegisterBlock("testmod:magic_log", 2.0f, McMineTool.Axe, McMineLevel.Wood);
    static McBlock softSand     = McRegistry.RegisterBlock("testmod:soft_sand", 0.5f, McMineTool.Shovel, McMineLevel.Stone);
    static McBlock basicBlock   = McRegistry.RegisterBlock("testmod:basic_block", 1.5f);

    // ── Block Items ──────────────────────────────────────────────────────────
    static McItem rubyOreItem     = McRegistry.RegisterBlockItem("testmod:ruby_ore", rubyOre);
    static McItem sapphireOreItem = McRegistry.RegisterBlockItem("testmod:sapphire_ore", sapphireOre);
    static McItem magicLogItem    = McRegistry.RegisterBlockItem("testmod:magic_log", magicLog);
    static McItem softSandItem    = McRegistry.RegisterBlockItem("testmod:soft_sand", softSand);
    static McItem basicBlockItem  = McRegistry.RegisterBlockItem("testmod:basic_block", basicBlock);

    // ── Items ────────────────────────────────────────────────────────────────
    static McItem ruby          = McRegistry.RegisterItem("testmod:ruby");
    static McItem sapphire      = McRegistry.RegisterItem("testmod:sapphire");
    static McItem magicDust     = McRegistry.RegisterItem("testmod:magic_dust");

    // ── Tools ────────────────────────────────────────────────────────────────
    static McItem rubySword     = McRegistry.RegisterSword("testmod:ruby_sword", McToolMaterial.Diamond, 5, -2.4f);
    static McItem rubyPickaxe   = McRegistry.RegisterPickaxe("testmod:ruby_pickaxe", McToolMaterial.Diamond, 2, -2.8f);
    static McItem rubyAxe       = McRegistry.RegisterAxe("testmod:ruby_axe", McToolMaterial.Diamond, 7.0f, -3.0f);
    static McItem rubyShovel    = McRegistry.RegisterShovel("testmod:ruby_shovel", McToolMaterial.Diamond, 2.5f, -3.0f);
    static McItem rubyHoe       = McRegistry.RegisterHoe("testmod:ruby_hoe", McToolMaterial.Diamond, 0, -2.0f);

    // ── Armor ────────────────────────────────────────────────────────────────
    static McItem rubyHelmet     = McRegistry.RegisterHelmet("testmod:ruby_helmet", McArmorMaterial.Diamond);
    static McItem rubyChestplate = McRegistry.RegisterChestplate("testmod:ruby_chestplate", McArmorMaterial.Diamond);
    static McItem rubyLeggings   = McRegistry.RegisterLeggings("testmod:ruby_leggings", McArmorMaterial.Diamond);
    static McItem rubyBoots      = McRegistry.RegisterBoots("testmod:ruby_boots", McArmorMaterial.Diamond);

    // ── Food ─────────────────────────────────────────────────────────────────
    static McItem magicApple    = McRegistry.RegisterFood("testmod:magic_apple", 8, 1.2f);
    static McItem rawMeat       = McRegistry.RegisterFood("testmod:raw_mystery_meat", 3, 0.3f, true);

    // ── Sounds ───────────────────────────────────────────────────────────────
    static McSoundEvent magicSound = McRegistry.RegisterSound("testmod:magic_chime");

    // ── Recipes ──────────────────────────────────────────────────────────────

    public void OnInitialize()
    {
        // ── Shaped Recipes ───────────────────────────────────────────────
        McRecipe.AddShaped("testmod:ruby_block_recipe",
            "testmod:basic_block", 1,
            "RRR",
            "RRR",
            "RRR",
            'R', "testmod:ruby");

        McRecipe.AddShaped("testmod:ruby_sword_recipe",
            "testmod:ruby_sword", 1,
            " R ",
            " R ",
            " S ",
            'R', "testmod:ruby",
            'S', "minecraft:stick");

        // ── Shapeless Recipes ────────────────────────────────────────────
        McRecipe.AddShapeless("testmod:magic_dust_recipe",
            "testmod:magic_dust", 4,
            "testmod:ruby", "testmod:sapphire");

        // ── Smelting / Blasting ──────────────────────────────────────────
        McRecipe.AddSmelting("testmod:ruby_from_ore",
            "testmod:ruby_ore", "testmod:ruby", 1.0f, 200);

        McRecipe.AddBlasting("testmod:ruby_from_ore_blast",
            "testmod:ruby_ore", "testmod:ruby", 1.0f, 100);

        McRecipe.AddSmelting("testmod:sapphire_from_ore",
            "testmod:sapphire_ore", "testmod:sapphire", 1.5f, 200);

        // ── Creative Tab ─────────────────────────────────────────────────
        McRegistry.AddToCreativeTab("minecraft:building_blocks", basicBlockItem);
        McRegistry.AddToCreativeTab("minecraft:combat", rubySword, rubyHelmet, rubyChestplate, rubyLeggings, rubyBoots);
        McRegistry.AddToCreativeTab("minecraft:tools_and_utilities", rubyPickaxe, rubyAxe, rubyShovel, rubyHoe);
        McRegistry.AddToCreativeTab("minecraft:food_and_drink", magicApple, rawMeat);

        // ── Player Join Event ────────────────────────────────────────────
        Events.PlayerJoin += (player) =>
        {
            player.SendMessage($"Welcome to the server, {player.Name}!");
            player.SendMessage("This server is running TestMod v1.0.0");
            player.GiveItem("testmod:ruby", 5);
            player.GiveItem("testmod:magic_apple", 3);

            // NBT data
            var nbt = new McNbt();
            nbt.SetInt("JoinCount", 1);
            nbt.SetString("FirstJoin", "true");
            string joinVal = nbt.GetString("FirstJoin");
            int count = nbt.GetInt("JoinCount");
            bool empty = nbt.IsEmpty;
        };

        // ── Player Death Event ───────────────────────────────────────────
        Events.PlayerDeath += (player) =>
        {
            player.SendMessage("You died! Here's some gear to get back on your feet.");
            player.GiveItem("testmod:ruby_sword", 1);
            player.GiveItem("testmod:magic_apple", 5);
        };

        // ── Block Break Event ────────────────────────────────────────────
        Events.BlockBreak += (player, x, y, z) =>
        {
            McWorld world = player.GetWorld();

            // Check block tags
            if (McTag.IsLog(world, x, y, z))
            {
                player.SendMessage("You broke a log!");
            }

            if (McTag.IsStone(world, x, y, z))
            {
                player.SendMessage("You broke stone!");
            }

            // Check the block state
            McBlockState state = world.GetBlockState(x, y, z);
            float hardness = state.Hardness;
            bool isAir = state.IsAir;
            bool isSolid = state.IsSolid;
        };

        // ── Chat Message Event ───────────────────────────────────────────
        Events.ChatMessage += (player, message) =>
        {
            if (message == "!heal")
            {
                player.Heal(player.MaxHealth);
                player.SendMessage("You have been healed!");
            }
            else if (message == "!feed")
            {
                player.SetHunger(20);
                player.SetSaturation(20.0f);
                player.SendMessage("You have been fed!");
            }
            else if (message == "!fly")
            {
                player.SetFlying(true);
                player.SendMessage("You are now flying!");
            }
            else if (message == "!gm")
            {
                player.SetGameMode("creative");
                player.SendMessage("Switched to creative mode!");
            }
            else if (message == "!pos")
            {
                double px = player.X;
                double py = player.Y;
                double pz = player.Z;
                player.SendMessage($"You are at {px}, {py}, {pz}");
            }
            else if (message == "!inv")
            {
                McInventory inv = player.GetInventory();
                int size = inv.Size;
                bool isEmpty = inv.IsEmpty;
                player.SendMessage($"Inventory size: {size}, empty: {isEmpty}");

                McItemStack slot0 = inv.GetSlot(0);
                string itemId = slot0.Id;
                int stackCount = slot0.Count;
                player.SendMessage($"Slot 0: {itemId} x{stackCount}");
            }
            else if (message == "!effects")
            {
                player.AddEffect("minecraft:speed", 600, 2);
                player.AddEffect("minecraft:strength", 600, 1);
                player.AddEffect("minecraft:night_vision", 1200, 0);
                player.AddEffect("minecraft:jump_boost", 600, 3);
                player.SendMessage("Applied speed, strength, night vision, and jump boost!");
            }
            else if (message == "!boom")
            {
                McWorld world = player.GetWorld();
                world.CreateExplosion(player.X, player.Y + 5, player.Z, 4.0f);
                player.SendMessage("Boom!");
            }
            else if (message == "!time day")
            {
                McWorld world = player.GetWorld();
                world.SetTimeOfDay(1000);
                player.SendMessage("Set time to day.");
            }
            else if (message == "!weather clear")
            {
                McWorld world = player.GetWorld();
                world.SetWeather(false, false);
                player.SendMessage("Weather cleared.");
            }
            else if (message == "!tp")
            {
                player.Teleport(0, 100, 0);
                player.SendMessage("Teleported to 0, 100, 0!");
            }
            else if (message == "!xp")
            {
                player.AddExperience(100);
                int level = player.ExperienceLevel;
                player.SendMessage($"Added 100 XP! Level: {level}");
            }
        };

        // ── Server Tick Event ────────────────────────────────────────────
        Events.ServerTick += (server) =>
        {
            // Example: check game time every tick
            long ticks = server.GetTicks();
            if (ticks % 24000 == 0)
            {
                server.BroadcastMessage("A new Minecraft day has begun!");
            }
        };

        // ── Commands ─────────────────────────────────────────────────────
        McCommand.Register("kit", (source) =>
        {
            source.SendMessage("Use /kit <name> to get a kit!");
        });

        McCommand.RegisterWithPlayer("heal", "target", (source, target) =>
        {
            target.Heal(target.MaxHealth);
            target.SetHunger(20);
            source.SendMessage($"Healed {target.Name}!");
        });

        McCommand.RegisterWithString("broadcast", "message", (source, message) =>
        {
            McServer server = source.GetServer();
            server.BroadcastMessage($"[Broadcast] {message}");
        });

        McCommand.RegisterWithInt("giverubies", "amount", (source, amount) =>
        {
            source.SendMessage($"Gave {amount} rubies (use with /giverubies <player> <amount>).");
        });

        // ── Scheduler ────────────────────────────────────────────────────
        McScheduler.RunLater(100, (server) =>
        {
            server.BroadcastMessage("TestMod has been active for 5 seconds!");
        });

        McScheduler.RunRepeating(6000, 6000, (server) =>
        {
            server.BroadcastMessage("5-minute reminder: TestMod is running!");
        });

        // ── Boss Bar (for fun) ───────────────────────────────────────────
        Events.PlayerJoin += (player) =>
        {
            var bossBar = new McBossBar("Welcome to TestMod!");
            bossBar.SetProgress(1.0f);
            bossBar.AddPlayer(player);
        };

        // ── Scoreboard ──────────────────────────────────────────────────
        Events.PlayerJoin += (player) =>
        {
            McScoreboard scoreboard = player.GetScoreboard();
            scoreboard.SetScore(player, "kills", 0);
            scoreboard.SetScore(player, "deaths", 0);
        };

        Events.PlayerDeath += (player) =>
        {
            McScoreboard scoreboard = player.GetScoreboard();
            int deaths = scoreboard.GetScore(player, "deaths");
            scoreboard.SetScore(player, "deaths", deaths + 1);
        };
    }
}
