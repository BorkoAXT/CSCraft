using CSCraft;

[ModInfo(
    Id          = "examplemod",
    Name        = "Example Mod",
    Version     = "1.0.0",
    Author      = "BorkoAXT",
    Description = "A showcase mod exercising every major CSCraft API"
)]
public class ExampleMod : IMod
{
    // ── Registered content ────────────────────────────────────────────────────
    McBlock  rubyOre    = null!;
    McItem   ruby       = null!;
    McItem   rubySword  = null!;
    McItem   rubyPickaxe = null!;
    McItem   rubyHelmet  = null!;
    McItem   rubyChest   = null!;
    McItem   cookedMystery = null!;
    McBossBar eventBar  = null!;

    public void OnInitialize()
    {
        RegisterContent();
        RegisterRecipes();
        RegisterCommands();
        RegisterEvents();
    }

    // =========================================================================
    //  Content registration
    // =========================================================================

    void RegisterContent()
    {
        // ── Blocks ────────────────────────────────────────────────────────────
        rubyOre = McRegistry.RegisterBlock("examplemod:ruby_ore", 3.0f, McMineTool.Pickaxe, McMineLevel.Iron);
        McBlock rubyBlock = McRegistry.RegisterBlock(
            "examplemod:ruby_block",
            McBlockSettings.Create().Strength(5.0f, 10.0f).RequiresTool().Sounds("stone"),
            McMineTool.Pickaxe, McMineLevel.Diamond);

        // ── Items / block-items ───────────────────────────────────────────────
        ruby          = McRegistry.RegisterItem("examplemod:ruby");
        McItem rubyOreItem = McRegistry.RegisterBlockItem("examplemod:ruby_ore", rubyOre);
        McItem rubyBlockItem = McRegistry.RegisterBlockItem("examplemod:ruby_block", rubyBlock);
        cookedMystery = McRegistry.RegisterFood("examplemod:cooked_mystery", 8, 0.8f, meat: true);
        McItem rubyArmor  = McRegistry.RegisterItem(
            "examplemod:ruby_shard",
            McItemSettings.Create().MaxCount(16).Rarity("RARE"));

        // ── Tools & armor ─────────────────────────────────────────────────────
        rubySword    = McRegistry.RegisterSword("examplemod:ruby_sword",   McToolMaterial.Diamond, bonusDamage: 4);
        rubyPickaxe  = McRegistry.RegisterPickaxe("examplemod:ruby_pickaxe", McToolMaterial.Diamond);
        McItem rubyAxe    = McRegistry.RegisterAxe("examplemod:ruby_axe",      McToolMaterial.Diamond);
        McItem rubyShovel = McRegistry.RegisterShovel("examplemod:ruby_shovel",  McToolMaterial.Diamond);
        McItem rubyHoe    = McRegistry.RegisterHoe("examplemod:ruby_hoe",        McToolMaterial.Diamond);
        rubyHelmet = McRegistry.RegisterHelmet("examplemod:ruby_helmet",         McArmorMaterial.Diamond);
        rubyChest  = McRegistry.RegisterChestplate("examplemod:ruby_chestplate", McArmorMaterial.Diamond);
        McItem rubyLegs  = McRegistry.RegisterLeggings("examplemod:ruby_leggings", McArmorMaterial.Diamond);
        McItem rubyBoots = McRegistry.RegisterBoots("examplemod:ruby_boots",       McArmorMaterial.Diamond);

        // ── Sound ─────────────────────────────────────────────────────────────
        McSoundEvent rubyChime = McRegistry.RegisterSound("examplemod:ruby_chime");

        // ── Game rules ────────────────────────────────────────────────────────
        McGameRule<bool> doubleDrops = McRegistry.RegisterBoolRule("examplemod:double_drops", false);
        McGameRule<int>  maxRubies   = McRegistry.RegisterIntRule("examplemod:max_rubies", 64);

        // ── Creative tabs ─────────────────────────────────────────────────────
        McCreativeTab.AddToNaturalBlocks(rubyOreItem);
        McCreativeTab.AddToBuildingBlocks(rubyBlockItem);
        McCreativeTab.AddToIngredients(ruby);
        McCreativeTab.AddToCombat(rubySword, rubyHelmet, rubyChest);
        McCreativeTab.AddToTools(rubyPickaxe, rubyAxe, rubyShovel, rubyHoe);
        McCreativeTab.AddToFood(cookedMystery);

        // ── Villager trades ────────────────────────────────────────────────────
        McVillager.AddSellTrade("minecraft:toolsmith", 2, ruby, 3, 5);
        McVillager.AddBuyTrade("minecraft:toolsmith", 3, ruby, 10, 1);
    }

    // =========================================================================
    //  Recipes  (all string-based — build system writes the JSON)
    // =========================================================================

    void RegisterRecipes()
    {
        // Shaped — ruby sword (2 rubies + stick)
        McRecipe.RegisterShaped(
            "examplemod:ruby_sword",
            new[] { " R ", " R ", " S " },
            new object[] { 'R', "examplemod:ruby", 'S', "minecraft:stick" },
            "examplemod:ruby_sword");

        // Shapeless — 9 rubies from ruby block
        McRecipe.RegisterShapeless(
            "examplemod:ruby_from_block",
            new[] { "examplemod:ruby_block" },
            "examplemod:ruby",
            count: 9);

        // Smelting — ruby ore → ruby
        McRecipe.RegisterSmelting(
            "examplemod:ruby_from_ore",
            "examplemod:ruby_ore",
            "examplemod:ruby",
            experience: 0.7f,
            cookTimeSeconds: 10);

        // Blasting — faster ore → ruby
        McRecipe.RegisterBlasting(
            "examplemod:ruby_from_ore_fast",
            "examplemod:ruby_ore",
            "examplemod:ruby",
            experience: 0.7f,
            cookTimeSeconds: 5);

        // Smoking — campfire mystery meat
        McRecipe.RegisterSmoking(
            "examplemod:cooked_mystery_smoked",
            "minecraft:rotten_flesh",
            "examplemod:cooked_mystery");

        // Campfire cooking
        McRecipe.RegisterCampfire(
            "examplemod:campfire_mystery",
            "minecraft:rotten_flesh",
            "examplemod:cooked_mystery");

        // Stonecutting — ruby slabs (2 per block)
        McRecipe.RegisterStonecutting(
            "examplemod:ruby_slab",
            "examplemod:ruby_block",
            "examplemod:ruby_slab",
            count: 2);
    }

    // =========================================================================
    //  Commands
    // =========================================================================

    void RegisterCommands()
    {
        // /kit — give starter kit to yourself
        McCommand.Register("kit", (src) =>
        {
            if (src.Player == null) { src.SendError("Players only!"); return; }
            src.Player.GiveItem("examplemod:ruby_sword", 1);
            src.Player.GiveItem("examplemod:ruby_pickaxe", 1);
            src.Player.GiveItem("examplemod:ruby_helmet", 1);
            src.Player.GiveItem("minecraft:bread", 32);
            src.Player.GiveEffect("minecraft:speed", 600, 1);
            src.Player.GiveEffect("minecraft:haste", 600, 1);
            src.SendMessage("Ruby kit granted!");
        });

        // /heal [target] — op only, heals a named player
        McCommand.RegisterOpWithPlayer("heal", "target", (src, target) =>
        {
            target.Heal(target.MaxHealth);
            target.ClearEffects();
            target.GiveEffect("minecraft:regeneration", 100, 1);
            target.SendMessage("You were healed by an admin.");
            src.SendMessage("Healed " + target.Name + "!");
        });

        // /spawn — teleport to world spawn
        McCommand.Register("spawn", (src) =>
        {
            if (src.Player == null) return;
            McBlockPos sp = src.Player.World.SpawnPos;
            src.Player.Teleport(sp.X, sp.Y, sp.Z);
            src.Player.PlaySound("minecraft:entity.enderman.teleport");
            src.SendMessage("Teleported to spawn!");
        });

        // /ruby give <count> — subcommand with int arg
        McCommand.RegisterSub("ruby", "give", "count", (src, count) =>
        {
            if (src.Player == null) { src.SendError("Players only!"); return; }
            src.Player.GiveItem("examplemod:ruby", count);
            src.SendMessage("Gave " + count + " rubies.");
        });

        // /locate — show biome, dimension, position
        McCommand.Register("locate", (src) =>
        {
            if (src.Player == null) return;
            string biome = src.Player.GetBiome();
            string dim   = src.Player.GetDimension();
            src.SendMessage("Biome: " + biome);
            src.SendMessage("Dimension: " + dim);
            src.SendMessage("X=" + src.Player.X + " Y=" + src.Player.Y + " Z=" + src.Player.Z);
        });

        // /info — detailed player info
        McCommand.Register("info", (src) =>
        {
            if (src.Player == null) return;
            McPlayer p = src.Player;
            p.SendMessage("=== Player Info ===");
            p.SendMessage("Name: " + p.Name);
            p.SendMessage("UUID: " + p.Uuid);
            p.SendMessage("HP: " + p.Health + "/" + p.MaxHealth);
            p.SendMessage("Food: " + p.FoodLevel);
            p.SendMessage("XP Level: " + p.XpLevel);
            p.SendMessage("Ping: " + p.GetPing() + "ms");
            p.SendMessage("Sneaking: " + p.IsSneaking);
            p.SendMessage("Flying: " + p.IsFlying);
            p.SendMessage("Blocking: " + p.IsBlocking);
            p.SendMessage("Game mode: " + p.GameMode);
        });

        // /visits — show persistent visit count
        McCommand.Register("visits", (src) =>
        {
            if (src.Player == null) return;
            int visits = PlayerData.GetInt(src.Player, "visits", 0);
            src.SendMessage("You have visited " + visits + " times.");
        });

        // /setvisits <n> — op only, set visit count
        McCommand.RegisterSub("ruby", "resetvisits", "count", (src, count) =>
        {
            if (src.Player == null) return;
            PlayerData.Set(src.Player, "visits", count);
            src.SendMessage("Visits reset to " + count + ".");
        });

        // /settime <ticks> — op only, set world time
        McCommand.Register("settime", "ticks", (src, ticks) =>
        {
            if (src.Player == null) return;
            src.Player.World.SetTime(ticks);
            src.SendMessage("Time set to " + ticks + ".");
        });

        // /fill <blockId> — fill 5x5 area at feet with a block
        McCommand.Register("fill", "blockId", (src, blockId) =>
        {
            if (src.Player == null) return;
            int x = (int)src.Player.X;
            int y = (int)src.Player.Y - 1;
            int z = (int)src.Player.Z;
            src.Player.World.FillBlocks(x - 2, y, z - 2, x + 2, y, z + 2, blockId);
            src.SendMessage("Filled 5x5 with " + blockId + "!");
        });

        // /serverinfo — show server stats
        McCommand.Register("serverinfo", (src) =>
        {
            McServer s = src.Server;
            src.SendMessage("Version: " + s.Version);
            src.SendMessage("Players: " + s.PlayerCount + "/" + s.MaxPlayers);
            src.SendMessage("TPS: " + s.Tps);
            src.SendMessage("Hardcore: " + s.IsHardcore);
            src.SendMessage("Seed: " + s.GetSeed());
        });

        // /inventory — inspect own inventory
        McCommand.Register("inventory", (src) =>
        {
            if (src.Player == null) return;
            McInventory inv = McInventory.FromPlayer(src.Player);
            int rubies   = inv.Count("examplemod:ruby");
            int diamonds = inv.Count("minecraft:diamond");
            bool hasRuby = inv.Contains("examplemod:ruby");
            src.SendMessage("Inventory size: " + inv.Size);
            src.SendMessage("Rubies: " + rubies + "  Diamonds: " + diamonds);
            src.SendMessage("Has rubies: " + hasRuby);
        });

        // /weather rain|clear — toggle weather
        McCommand.Register("weather", "mode", (src, mode) =>
        {
            if (src.Player == null) return;
            if (mode == "rain")
                src.Player.World.SetWeather(0, 6000, true, false);
            else
                src.Player.World.SetWeather(6000, 0, false, false);
            src.SendMessage("Weather set to " + mode + ".");
        });

        // /effect <effectId> — give a 60s effect to yourself
        McCommand.Register("effect", "effectId", (src, effectId) =>
        {
            if (src.Player == null) return;
            src.Player.GiveEffect(effectId, 1200, 0);
            src.SendMessage("Applied " + effectId + " for 60 seconds.");
        });

        // /setgm <mode> — op only, change own game mode
        McCommand.RegisterOp("creative", (src) =>
        {
            src.Player?.SetGameMode("creative");
            src.SendMessage("Game mode set to creative.");
        });

        McCommand.RegisterOp("survival", (src) =>
        {
            src.Player?.SetGameMode("survival");
            src.SendMessage("Game mode set to survival.");
        });

        // /border <size> — op only, set world border
        McCommand.Register("border", "size", (src, size) =>
        {
            if (src.Player == null) return;
            src.Player.World.AnimateBorderSize(size, 5.0);
            src.SendMessage("World border animating to " + size + ".");
        });

        // /lightning — strike lightning at your feet
        McCommand.Register("lightning", (src) =>
        {
            if (src.Player == null) return;
            src.Player.World.SpawnLightning(src.Player.X, src.Player.Y, src.Player.Z);
            src.SendMessage("Lightning!");
        });

        // /explode — small explosion at feet
        McCommand.Register("explode", (src) =>
        {
            if (src.Player == null) return;
            src.Player.World.CreateExplosion(src.Player.X, src.Player.Y, src.Player.Z, 3.0f);
        });

        // /xp <amount> — give yourself XP
        McCommand.Register("xp", "amount", (src, amount) =>
        {
            if (src.Player == null) return;
            src.Player.GiveXp(amount);
            src.SendMessage("Gave " + amount + " XP.");
        });

        // /home set / /home go — teleport home using persistent data
        McCommand.RegisterSub("home", "set", (src) =>
        {
            if (src.Player == null) return;
            PlayerData.Set(src.Player, "home_x", (int)src.Player.X);
            PlayerData.Set(src.Player, "home_y", (int)src.Player.Y);
            PlayerData.Set(src.Player, "home_z", (int)src.Player.Z);
            src.SendMessage("Home set!");
        });

        McCommand.RegisterSub("home", "go", (src) =>
        {
            if (src.Player == null) return;
            if (!PlayerData.Has(src.Player, "home_x"))
            {
                src.SendError("No home set. Use /home set first.");
                return;
            }
            int hx = PlayerData.GetInt(src.Player, "home_x", 0);
            int hy = PlayerData.GetInt(src.Player, "home_y", 64);
            int hz = PlayerData.GetInt(src.Player, "home_z", 0);
            src.Player.Teleport(hx, hy, hz);
            src.Player.PlaySound("minecraft:entity.enderman.teleport");
            src.SendMessage("Teleported home!");
        });

        // /nbt get <key> / /nbt set <key> <value>
        McCommand.RegisterSub("nbt", "get", "key", (src, key) =>
        {
            if (src.Player == null) return;
            string val = src.Player.GetNbtString(key);
            src.SendMessage(key + " = " + val);
        });
    }

    // =========================================================================
    //  Events
    // =========================================================================

    void RegisterEvents()
    {
        // ── Boss bar setup ────────────────────────────────────────────────────
        eventBar = new McBossBar("Server Events", McBossBar.BarColor.Purple);
        eventBar.SetVisible(true);

        // ── Player join ───────────────────────────────────────────────────────
        Events.PlayerJoin += (player) =>
        {
            // Persistent visit counter
            int visits = player.GetNbtInt("visits");
            visits++;
            player.SetNbtInt("visits", visits);

            // Title + chat welcome
            player.SendTitle("Welcome!", "Hello, " + player.Name);
            player.SendMessage("Welcome to the server! Visit #" + visits + ".");
            player.SendActionBar("Server has " + player.Server.PlayerCount + " player(s) online.");

            // Starter bread on first visit only
            if (visits == 1)
            {
                player.GiveItem("minecraft:bread", 16);
                player.GiveItem("minecraft:wooden_sword", 1);
                player.SendMessage("First visit! Here's some bread and a sword.");
            }

            // Boss bar
            eventBar.AddPlayer(player);
            eventBar.SetTitle(player.Name + " joined!");
            eventBar.SetProgress(1.0f);

            // Scoreboard
            McScoreboard.CreateObjective(player.Server, "kills", "Kills");
            McScoreboard.ShowSidebar(player.Server, "kills");
            if (!PlayerData.Has(player, "kills_init"))
            {
                McScoreboard.SetScore(player.Server, player, "kills", 0);
                PlayerData.Set(player, "kills_init", true);
            }
            else
            {
                int savedKills = PlayerData.GetInt(player, "kills", 0);
                McScoreboard.SetScore(player.Server, player, "kills", savedKills);
            }

            // Teams — put all players in "players" team
            McScoreboard.CreateTeam(player.Server, "players");
            McScoreboard.SetTeamColor(player.Server, "players", "green");
            McScoreboard.AddPlayerToTeam(player.Server, player, "players");

            // Inventory state
            McInventory inv = McInventory.FromPlayer(player);
            int rubyCount = inv.Count("examplemod:ruby");
            if (rubyCount > 0)
                player.SendMessage("You have " + rubyCount + " rubies.");
        };

        // ── Player leave ──────────────────────────────────────────────────────
        Events.PlayerLeave += (player) =>
        {
            eventBar.RemovePlayer(player);

            // Save kill count to NBT before disconnect
            int kills = McScoreboard.GetScore(player.Server, player, "kills");
            PlayerData.Set(player, "kills", kills);

            player.Server.Broadcast(player.Name + " left. Kills this session: " + kills + ".");
        };

        // ── Player death ──────────────────────────────────────────────────────
        Events.PlayerDeath += (player) =>
        {
            player.World.SpawnLightning(player.X, player.Y, player.Z);
            player.World.SpawnParticle(McParticles.HugeExplosion, player.X, player.Y + 1, player.Z, 3);
            player.Server.Broadcast(player.Name + " has fallen!");

            // Shrink world border slightly
            double currentSize = player.World.GetBorderSize();
            if (currentSize > 200)
            {
                player.World.AnimateBorderSize(currentSize - 10, 5.0);
                player.Server.Broadcast("World border shrinking to " + (currentSize - 10) + "!");
            }
        };

        // ── Player respawn ────────────────────────────────────────────────────
        Events.PlayerRespawn += (player) =>
        {
            player.SendMessage("You respawned! Stay alive.");
            player.GiveEffect("minecraft:resistance", 100, 1);
        };

        // ── Block break ───────────────────────────────────────────────────────
        Events.BlockBreak += (player, pos) =>
        {
            string block = player.World.GetBlock(pos.X, pos.Y, pos.Z);

            if (block == "examplemod:ruby_ore")
            {
                player.GiveItem("examplemod:ruby", 2);
                player.World.SpawnParticle(McParticles.Glow, pos.X, pos.Y + 1, pos.Z, 20);
                player.World.SpawnParticle(McParticles.Crit, pos.X, pos.Y + 1, pos.Z, 10);
                player.World.PlaySound("examplemod:ruby_chime", pos.X, pos.Y, pos.Z);
                player.SendActionBar("Found rubies!");

                // Update scoreboard kill counter as a "score" event
                int score = McScoreboard.GetScore(player.Server, player, "kills");
                McScoreboard.SetScore(player.Server, player, "kills", score + 1);
                PlayerData.Set(player, "kills", score + 1);
            }

            if (block == "minecraft:diamond_ore" || block == "minecraft:deepslate_diamond_ore")
            {
                player.World.SpawnParticle(McParticles.EnchantTable, pos.X, pos.Y + 1, pos.Z, 30);
                player.SendActionBar("Diamonds!");
            }
        };

        // ── Block place ───────────────────────────────────────────────────────
        Events.BlockInteract += (player, pos) =>
        {
            // If they right-click a chest containing rubies, notify them
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

        // ── Chat commands ─────────────────────────────────────────────────────
        Events.ChatMessage += (player, message) =>
        {
            if (message == "!effects")
            {
                player.GiveEffect("minecraft:speed", 600, 1);
                player.GiveEffect("minecraft:night_vision", 600, 0);
                player.GiveEffect("minecraft:resistance", 600, 1);
                player.GiveEffect("minecraft:strength", 600, 1);
                player.SendMessage("Buff package applied for 30 seconds!");
            }

            if (message == "!clear")
            {
                player.ClearEffects();
                player.SendMessage("All effects cleared.");
            }

            if (message == "!setspawn")
            {
                player.SetSpawn((int)player.X, (int)player.Y, (int)player.Z);
                player.SendMessage("Spawn point updated.");
            }

            if (message == "!fly")
            {
                player.SetGameMode("creative");
                player.SendMessage("Creative mode enabled for flight.");
            }

            if (message == "!health")
            {
                player.SendMessage("HP: " + player.Health + "/" + player.MaxHealth);
                player.SendMessage("Food: " + player.FoodLevel);
            }

            if (message == "!world")
            {
                McWorld world = player.World;
                player.SendMessage("Time: " + world.Time);
                player.SendMessage("Day: " + world.IsDay + " | Raining: " + world.IsRaining);
                player.SendMessage("Difficulty: " + world.Difficulty);
                player.SendMessage("Dimension: " + world.Dimension);
                int topY = world.GetTopY((int)player.X, (int)player.Z);
                player.SendMessage("Top Y at position: " + topY);
            }

            if (message == "!inv")
            {
                McInventory inv = McInventory.FromPlayer(player);
                McItemStack main = player.MainHandItem;
                McItemStack off  = player.OffHandItem;
                player.SendMessage("Main hand: " + main.GetItem());
                player.SendMessage("Off hand: " + off.GetItem());
                player.SendMessage("Inventory slots: " + inv.Size);
            }

            if (message == "!armor")
            {
                player.SendMessage("Helmet: " + player.Helmet.GetItem());
                player.SendMessage("Chest: "  + player.Chestplate.GetItem());
                player.SendMessage("Legs: "   + player.Leggings.GetItem());
                player.SendMessage("Boots: "  + player.Boots.GetItem());
            }

            if (message == "!border")
            {
                McWorld world = player.World;
                player.SendMessage("Border size: " + world.GetBorderSize());
                player.SendMessage("Center: " + world.GetBorderCenterX() + ", " + world.GetBorderCenterZ());
                player.SendMessage("In border: " + world.IsInBorder((int)player.X, (int)player.Y, (int)player.Z));
            }

            if (message == "!particles")
            {
                McWorld world = player.World;
                world.SpawnParticle(McParticles.Heart, player.X, player.Y + 2, player.Z, 15);
                world.SpawnParticle(McParticles.Totem, player.X, player.Y + 1, player.Z, 20);
                world.SpawnParticle(McParticles.Note,  player.X, player.Y + 3, player.Z, 10);
                world.SpawnParticle(McParticles.Cherry, player.X + 1, player.Y + 2, player.Z, 30);
                player.SendActionBar("Particles!");
            }

            if (message == "!sound")
            {
                player.PlaySound("minecraft:entity.experience_orb.pickup");
                player.World.PlaySound("minecraft:block.bell.use", player.X, player.Y, player.Z);
            }

            if (message == "!enchant")
            {
                McItemStack sword = player.MainHandItem;
                if (!sword.IsEmpty)
                {
                    sword.AddEnchantment("minecraft:sharpness", 5);
                    sword.AddEnchantment("minecraft:unbreaking", 3);
                    player.SendMessage("Enchanted your held item!");
                }
                else
                {
                    player.SendMessage("Hold an item to enchant.");
                }
            }

            if (message == "!nbt")
            {
                McItemStack held = player.MainHandItem;
                held.SetNbtString("owner", player.Name);
                held.SetNbtInt("power", 99);
                string owner = held.GetNbtString("owner");
                int power    = held.GetNbtInt("power");
                player.SendMessage("Set owner=" + owner + " power=" + power + " on held item.");
            }

            if (message == "!tags")
            {
                McWorld world = player.World;
                int bx = (int)player.X;
                int by = (int)player.Y - 1;
                int bz = (int)player.Z;
                bool isLog  = McTag.IsLog(world, bx, by, bz);
                bool isDirt = McTag.IsDirt(world, bx, by, bz);
                bool isSword = McTag.IsSword(player.MainHandItem);
                player.SendMessage("Block below — log: " + isLog + " dirt: " + isDirt);
                player.SendMessage("Main hand is sword: " + isSword);
            }

            if (message == "!fluid")
            {
                McWorld world = player.World;
                int fx = (int)player.X;
                int fy = (int)player.Y;
                int fz = (int)player.Z;
                player.SendMessage("Water: " + McFluid.IsWater(world, fx, fy, fz));
                player.SendMessage("Lava: "  + McFluid.IsLava(world, fx, fy, fz));
                player.SendMessage("Submerged: " + McFluid.IsPlayerSubmerged(player));
            }

            if (message == "!entities")
            {
                McWorld world = player.World;
                List<McEntity> nearby = world.GetNearbyEntities(player.X, player.Y, player.Z, 16.0);
                player.SendMessage("Nearby entities (16r): " + nearby.Count);
                foreach (McEntity e in nearby)
                {
                    player.SendMessage("  " + e.TypeId + " — alive: " + e.IsAlive);
                }
            }
        };

        // ── Entity hurt — undead burn ─────────────────────────────────────────
        Events.EntityHurt += (entity, amount) =>
        {
            if (McTag.IsUndead(entity))
            {
                entity.SetOnFire(3);
                entity.World.SpawnParticle(McParticles.Flame, entity.X, entity.Y + 1, entity.Z, 10);
            }
        };

        // ── Player attack — sword particle burst ──────────────────────────────
        Events.PlayerAttack += (player, target) =>
        {
            McItemStack main = player.MainHandItem;
            if (McTag.IsSword(main))
            {
                target.World.SpawnParticle(McParticles.Crit, target.X, target.Y + 1, target.Z, 15);
                target.World.SpawnParticle(McParticles.MagicCrit, target.X, target.Y + 1, target.Z, 10);
                player.SendActionBar("Hit " + target.Name + "!");
            }
        };

        // ── Player use entity — show entity info ──────────────────────────────
        Events.PlayerUseEntity += (player, entity) =>
        {
            player.SendMessage("Entity: " + entity.TypeId);
            player.SendMessage("Health: " + entity.Health + "/" + entity.MaxHealth);
            player.SendMessage("Baby: " + entity.IsBaby());
            if (entity.IsPlayer)
                player.SendMessage("(This is a player)");
        };

        // ── Item pickup ───────────────────────────────────────────────────────
        Events.ItemPickup += (player, stack) =>
        {
            if (!stack.IsEmpty && stack.GetItem() == "examplemod:ruby")
            {
                player.SendActionBar("Picked up a ruby!");
                player.World.SpawnParticle(McParticles.Glow, player.X, player.Y + 1, player.Z, 5);
            }
        };

        // ── Server start — delayed broadcast + scoreboard setup ───────────────
        Events.ServerStart += (server) =>
        {
            Console.WriteLine("ExampleMod initialized!");

            McScoreboard.CreateObjective(server, "kills", "Kills");
            McScoreboard.ShowSidebar(server, "kills");

            // Announce 10 seconds after start
            McScheduler.RunLater(server, 200, (s) =>
            {
                s.Broadcast("ExampleMod is ready! Type !effects, /kit, or /info.");
            });

            // Repeating announcements every 5 minutes
            McScheduler.RunRepeating(server, 6000, (s, cancel) =>
            {
                if (s.PlayerCount > 0)
                    s.Broadcast("Reminder: Type /kit for a free ruby kit!");
            });
        };

        // ── Server stop ───────────────────────────────────────────────────────
        Events.ServerStop += (server) =>
        {
            Console.WriteLine("ExampleMod shutting down.");
        };

        // ── Server tick — slowly fade boss bar ───────────────────────────────
        Events.ServerTick += (server) =>
        {
            float prog = eventBar.Progress;
            if (prog > 0)
                eventBar.SetProgress(prog - 0.0005f);
        };

        // ── World tick — world-specific logic ────────────────────────────────
        Events.WorldTick += (world) =>
        {
            // Every ~5 seconds (100 ticks), check if it's raining and reset time
            if (world.Time % 100 == 0 && world.IsRaining && world.IsDay)
            {
                // Don't clear rain, just note it
            }
        };

        // ── Entity spawn ──────────────────────────────────────────────────────
        Events.EntitySpawn += (entity) =>
        {
            if (entity.TypeId == "minecraft:wither")
            {
                // Name the wither and tag it
                entity.SetCustomName("Server Boss");
                entity.SetCustomNameVisible(true);
                entity.AddTag("boss");
                Console.WriteLine("A wither spawned!");
            }
        };

        // ── Entity death ──────────────────────────────────────────────────────
        Events.EntityDeath += (entity) =>
        {
            if (entity.TypeId == "minecraft:ender_dragon")
            {
                McWorld world = entity.World;
                world.SpawnParticle(McParticles.Dragon, entity.X, entity.Y + 5, entity.Z, 50);
                world.SpawnParticle(McParticles.End, entity.X, entity.Y + 5, entity.Z, 100);
                world.Server.Broadcast("The Ender Dragon has been defeated!");
            }
        };

        // ── Chunk load ────────────────────────────────────────────────────────
        Events.ChunkLoad += (world) =>
        {
            // Nothing needed — example of subscribing to this event
        };

        // ── World load / unload ───────────────────────────────────────────────
        Events.WorldLoad += (world) =>
        {
            Console.WriteLine("World loaded: " + world.Dimension);
        };

        Events.WorldUnload += (world) =>
        {
            Console.WriteLine("World unloaded: " + world.Dimension);
        };

        // ── Loot table additions (build-time hints, emits TODO comment) ───────
        LootTables.AddMobDrop("minecraft:zombie", "examplemod:ruby", 0.05f, 1, 1);
        LootTables.AddMobDrop("minecraft:creeper", "examplemod:ruby", 0.1f, 1, 2);
        LootTables.AddChestLoot(LootChest.Dungeon, "examplemod:ruby", 0.3f, 1, 3);
        LootTables.AddChestLoot(LootChest.Mineshaft, "examplemod:ruby", 0.2f, 1, 2);
    }
}
