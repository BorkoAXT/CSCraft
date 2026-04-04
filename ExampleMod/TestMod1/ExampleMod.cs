using CSCraft;

[ModInfo(
    Id          = "examplemod",
    Name        = "Example Mod",
    Version     = "1.0.0",
    Author      = "BorkoAXT",
    Description = "A showcase mod demonstrating every CSCraft feature"
)]
public class ExampleMod : IMod
{
    // ── Registered content (fields so other methods can reference them) ────
    McBlock rubyOre = null!;
    McItem ruby = null!;
    McItem rubySword = null!;
    McItem rubyPickaxe = null!;
    McItem rubyHelmet = null!;
    McItem cookedMystery = null!;
    McBossBar eventBar = null!;

    public void OnInitialize()
    {
        RegisterContent();
        RegisterRecipes();
        RegisterCommands();
        RegisterEvents();
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  Content registration
    // ═══════════════════════════════════════════════════════════════════════

    void RegisterContent()
    {
        // Blocks
        rubyOre = McRegistry.RegisterBlock("examplemod:ruby_ore", hardness: 3.0f);
        McItem rubyOreItem = McRegistry.RegisterBlockItem("examplemod:ruby_ore", rubyOre);

        // Items
        ruby = McRegistry.RegisterItem("examplemod:ruby");
        cookedMystery = McRegistry.RegisterFood("examplemod:cooked_mystery", 8, 0.8f, meat: true);

        // Tools
        rubySword   = McRegistry.RegisterSword("examplemod:ruby_sword", McToolMaterial.Diamond, bonusDamage: 4);
        rubyPickaxe = McRegistry.RegisterPickaxe("examplemod:ruby_pickaxe", McToolMaterial.Diamond);

        // Armor
        rubyHelmet = McRegistry.RegisterHelmet("examplemod:ruby_helmet", McArmorMaterial.Diamond);
        McItem rubyChest = McRegistry.RegisterChestplate("examplemod:ruby_chestplate", McArmorMaterial.Diamond);

        // Custom sound
        McSoundEvent rubySound = McRegistry.RegisterSound("examplemod:ruby_chime");

        // Creative tabs
        McCreativeTab.AddToNaturalBlocks(rubyOreItem);
        McCreativeTab.AddToIngredients(ruby);
        McCreativeTab.AddToCombat(rubySword, rubyHelmet, rubyChest);
        McCreativeTab.AddToTools(rubyPickaxe);
        McCreativeTab.AddToFood(cookedMystery);

        // Custom game rules
        McGameRule<bool> doubleDrops = McRegistry.RegisterBoolRule("examplemod:double_drops", false);
        McGameRule<int> maxRubies = McRegistry.RegisterIntRule("examplemod:max_rubies", 64);

        // Villager trade: sell rubies for emeralds
        McVillager.AddSellTrade("minecraft:toolsmith", 2, ruby, 3, 5);
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  Recipes (JSON generated automatically at build time)
    // ═══════════════════════════════════════════════════════════════════════

    void RegisterRecipes()
    {
        // Shaped: ruby sword
        McRecipe.RegisterShaped(
            "examplemod:ruby_sword",
            new[] { " R ", " R ", " S " },
            new object[] { 'R', "examplemod:ruby", 'S', "minecraft:stick" },
            "examplemod:ruby_sword");

        // Shapeless: 9 rubies from ruby block
        McRecipe.RegisterShapeless(
            "examplemod:ruby_from_block",
            new[] { "examplemod:ruby_block" },
            "examplemod:ruby",
            count: 9);

        // Smelting: ruby ore → ruby
        McRecipe.RegisterSmelting(
            "examplemod:ruby_from_ore",
            "examplemod:ruby_ore",
            "examplemod:ruby",
            experience: 0.7f,
            cookTimeSeconds: 10);

        // Blasting (faster smelting)
        McRecipe.RegisterBlasting(
            "examplemod:ruby_from_ore_fast",
            "examplemod:ruby_ore",
            "examplemod:ruby",
            experience: 0.7f,
            cookTimeSeconds: 5);

        // Campfire cooking
        McRecipe.RegisterCampfire(
            "examplemod:campfire_mystery",
            "minecraft:rotten_flesh",
            "examplemod:cooked_mystery");

        // Stonecutting
        McRecipe.RegisterStonecutting(
            "examplemod:ruby_slab",
            "examplemod:ruby_block",
            "examplemod:ruby_slab",
            count: 2);
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  Commands
    // ═══════════════════════════════════════════════════════════════════════

    void RegisterCommands()
    {
        // /kit — give a starter kit to yourself
        McCommand.Register("kit", (src) =>
        {
            if (src.Player == null) { src.SendError("Players only!"); return; }
            src.Player.GiveItem("examplemod:ruby_sword", 1);
            src.Player.GiveItem("examplemod:ruby_pickaxe", 1);
            src.Player.GiveItem("examplemod:ruby_helmet", 1);
            src.Player.GiveItem("minecraft:bread", 32);
            src.Player.GiveEffect("minecraft:speed", 600, 1);
            src.SendMessage("Starter kit granted!");
        });

        // /heal <player> — op-only, heal a target player
        McCommand.RegisterOpWithPlayer("heal", "target", (src, target) =>
        {
            target.Heal(target.MaxHealth);
            target.ClearEffects();
            target.GiveEffect("minecraft:regeneration", 100, 1);
            src.SendMessage($"Healed {target.Name}!");
            target.SendMessage("You were healed by an admin.");
        });

        // /spawn — teleport to world spawn
        McCommand.Register("spawn", (src) =>
        {
            if (src.Player == null) return;
            McBlockPos spawn = src.Player.World.SpawnPos;
            src.Player.Teleport(spawn.X, spawn.Y, spawn.Z);
            src.Player.PlaySound("minecraft:entity.enderman.teleport");
            src.SendMessage("Teleported to spawn!");
        });

        // /ruby give <count> — give rubies
        McCommand.RegisterSub("ruby", "give", "count", (src, count) =>
        {
            src.Player?.GiveItem("examplemod:ruby", count);
            src.SendMessage($"Gave {count} rubies.");
        });

        // /locate — show biome and dimension
        McCommand.Register("locate", (src) =>
        {
            if (src.Player == null) return;
            string biome = src.Player.GetBiome();
            string dim = src.Player.GetDimension();
            src.SendMessage($"Biome: {biome}, Dimension: {dim}");
            src.SendMessage($"Position: {src.Player.X}, {src.Player.Y}, {src.Player.Z}");
        });
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  Events
    // ═══════════════════════════════════════════════════════════════════════

    void RegisterEvents()
    {
        // ── Boss bar for online players ──────────────────────────────────
        eventBar = new McBossBar("Server Events", McBossBar.BarColor.Purple);
        eventBar.SetVisible(true);

        // ── Player join: welcome + boss bar + scoreboard ─────────────────
        Events.PlayerJoin += (player) =>
        {
            player.SendTitle("Welcome!", $"Hello, {player.Name}");
            player.SendMessage($"Welcome to the server, {player.Name}!");
            player.GiveItem("minecraft:bread", 16);

            // Show the event boss bar
            eventBar.AddPlayer(player);
            eventBar.SetTitle($"{player.Name} joined!");
            eventBar.SetProgress(1.0f);

            // Set up scoreboard
            McScoreboard.CreateObjective(player.Server, "kills", "Kills");
            McScoreboard.ShowSidebar(player.Server, "kills");
            McScoreboard.SetScore(player.Server, player, "kills", 0);

            // Create teams
            McScoreboard.CreateTeam(player.Server, "red");
            McScoreboard.SetTeamColor(player.Server, "red", "red");
            McScoreboard.SetTeamPrefix(player.Server, "red", "[RED] ");

            // Persistent NBT greeting
            int visits = player.GetNbtInt("visits");
            visits++;
            player.SetNbtInt("visits", visits);
            player.SendMessage($"This is visit #{visits}!");
        };

        // ── Player leave ─────────────────────────────────────────────────
        Events.PlayerLeave += (player) =>
        {
            eventBar.RemovePlayer(player);
            player.Server.Broadcast($"{player.Name} left the server.");
        };

        // ── Block break: ruby ore drops + particle effects ───────────────
        Events.BlockBreak += (player, pos) =>
        {
            string block = player.World.GetBlock(pos.X, pos.Y, pos.Z);
            if (block == "examplemod:ruby_ore")
            {
                player.GiveItem("examplemod:ruby", 2);
                player.World.SpawnParticle(McParticles.Flame, pos.X, pos.Y, pos.Z, 30);
                player.PlaySound("minecraft:entity.experience_orb.pickup");
                player.SendActionBar("Found rubies!");

                // Update scoreboard
                int kills = McScoreboard.GetScore(player.Server, player, "kills");
                McScoreboard.SetScore(player.Server, player, "kills", kills + 1);
            }
        };

        // ── Chat: simple command system ──────────────────────────────────
        Events.ChatMessage += (player, message) =>
        {
            if (message == "!effects")
            {
                player.GiveEffect("minecraft:speed", 600, 1);
                player.GiveEffect("minecraft:night_vision", 600, 0);
                player.GiveEffect("minecraft:resistance", 600, 1);
                player.SendMessage("Buff package applied!");
            }

            if (message == "!info")
            {
                player.SendMessage($"Health: {player.Health}/{player.MaxHealth}");
                player.SendMessage($"Food: {player.FoodLevel}");
                player.SendMessage($"XP Level: {player.XpLevel}");
                player.SendMessage($"Biome: {player.GetBiome()}");
                player.SendMessage($"Ping: {player.GetPing()}ms");
                player.SendMessage($"Sneaking: {player.IsSneaking}, Flying: {player.IsFlying}");
            }

            if (message == "!inventory")
            {
                McInventory inv = McInventory.FromPlayer(player);
                int diamonds = inv.Count("minecraft:diamond");
                int rubies = inv.Count("examplemod:ruby");
                player.SendMessage($"Diamonds: {diamonds}, Rubies: {rubies}");
                player.SendMessage($"Inventory slots used: {inv.Size}");
            }
        };

        // ── Player death: lightning + announcement ───────────────────────
        Events.PlayerDeath += (player) =>
        {
            player.World.SpawnLightning(player.X, player.Y, player.Z);
            player.Server.Broadcast($"{player.Name} has fallen!");
        };

        // ── Entity hurt: undead burn, track damage ───────────────────────
        Events.EntityHurt += (entity, amount) =>
        {
            if (McTag.IsUndead(entity))
            {
                entity.SetOnFire(3);
                entity.World.SpawnParticle(McParticles.Flame, entity.X, entity.Y + 1, entity.Z, 10);
            }
        };

        // ── Player attack: knockback + particles ─────────────────────────
        Events.PlayerAttack += (player, target) =>
        {
            if (McTag.IsSword(player.MainHandItem))
            {
                target.World.SpawnParticle(McParticles.Crit, target.X, target.Y + 1, target.Z, 15);
                player.SendActionBar($"Hit {target.Name}!");
            }
        };

        // ── Server start: set up delayed tasks ──────────────────────────
        Events.ServerStart += (server) =>
        {
            Console.WriteLine("ExampleMod loaded!");

            // Broadcast a message 10 seconds after server starts
            McScheduler.RunLater(server, 200, (s) =>
            {
                s.Broadcast("ExampleMod is ready! Type !info or /kit to get started.");
            });
        };

        // ── Server tick: update boss bar progress ────────────────────────
        Events.ServerTick += (server) =>
        {
            // Slowly fade the boss bar progress down
            float progress = eventBar.Progress;
            if (progress > 0)
            {
                eventBar.SetProgress(progress - 0.001f);
            }
        };

        // ── World border: shrink on player death ─────────────────────────
        Events.PlayerDeath += (player) =>
        {
            double currentSize = player.World.GetBorderSize();
            if (currentSize > 100)
            {
                player.World.AnimateBorderSize(currentSize - 10, 5.0);
                player.Server.Broadcast("World border shrinking!");
            }
        };

        // ── Block interact: check for chest contents ─────────────────────
        Events.BlockInteract += (player, pos) =>
        {
            McBlockEntity? be = player.World.GetBlockEntity(pos.X, pos.Y, pos.Z);
            if (be != null && be.IsChest)
            {
                McInventory? chest = be.GetInventory();
                if (chest != null && chest.Contains("examplemod:ruby"))
                {
                    player.SendMessage("This chest contains rubies!");
                    player.World.SpawnParticle(McParticles.HappyVillager, pos.X, pos.Y + 1, pos.Z, 10);
                }
            }
        };
    }
}
