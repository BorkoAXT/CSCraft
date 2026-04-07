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
    McBlock  rubyOre      = null!;
    McItem   ruby         = null!;
    McItem   rubySword    = null!;
    McItem   rubyPickaxe  = null!;
    McItem   rubyHelmet   = null!;
    McItem   rubyChest    = null!;
    McItem   cookedMystery = null!;
    McBossBar eventBar    = null!;

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
        McBlock rubyBlock = McRegistry.RegisterBlock("examplemod:ruby_block", 5.0f, McMineTool.Pickaxe, McMineLevel.Diamond);

        // ── Items / block-items ───────────────────────────────────────────────
        ruby           = McRegistry.RegisterItem("examplemod:ruby");
        McItem rubyOreItem   = McRegistry.RegisterBlockItem("examplemod:ruby_ore", rubyOre);
        McItem rubyBlockItem = McRegistry.RegisterBlockItem("examplemod:ruby_block", rubyBlock);
        cookedMystery  = McRegistry.RegisterFood("examplemod:cooked_mystery", 8, 0.8f, meat: true);
        McItem rubyShard = McRegistry.RegisterItem("examplemod:ruby_shard");

        // ── Tools & armor ─────────────────────────────────────────────────────
        rubySword    = McRegistry.RegisterSword("examplemod:ruby_sword",      McToolMaterial.Diamond, bonusDamage: 4);
        rubyPickaxe  = McRegistry.RegisterPickaxe("examplemod:ruby_pickaxe",  McToolMaterial.Diamond);
        McItem rubyAxe    = McRegistry.RegisterAxe("examplemod:ruby_axe",         McToolMaterial.Diamond);
        McItem rubyShovel = McRegistry.RegisterShovel("examplemod:ruby_shovel",    McToolMaterial.Diamond);
        McItem rubyHoe    = McRegistry.RegisterHoe("examplemod:ruby_hoe",          McToolMaterial.Diamond);
        rubyHelmet = McRegistry.RegisterHelmet("examplemod:ruby_helmet",           McArmorMaterial.Diamond);
        rubyChest  = McRegistry.RegisterChestplate("examplemod:ruby_chestplate",   McArmorMaterial.Diamond);
        McItem rubyLegs  = McRegistry.RegisterLeggings("examplemod:ruby_leggings", McArmorMaterial.Diamond);
        McItem rubyBoots = McRegistry.RegisterBoots("examplemod:ruby_boots",        McArmorMaterial.Diamond);

        // ── Sound ─────────────────────────────────────────────────────────────
        McSoundEvent rubyChime = McRegistry.RegisterSound("examplemod:ruby_chime");

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
    //  Recipes  (build-time hints — transpiler writes data/modid/recipe/ JSON)
    // =========================================================================

    void RegisterRecipes()
    {
        McRecipe.RegisterShaped(
            "examplemod:ruby_sword",
            new string[] { " R ", " R ", " S " },
            new object[] { "R", "examplemod:ruby", "S", "minecraft:stick" },
            "examplemod:ruby_sword");

        McRecipe.RegisterShapeless(
            "examplemod:ruby_from_block",
            new string[] { "examplemod:ruby_block" },
            "examplemod:ruby",
            count: 9);

        McRecipe.RegisterSmelting("examplemod:ruby_from_ore",        "examplemod:ruby_ore", "examplemod:ruby", experience: 0.7f, cookTimeSeconds: 10);
        McRecipe.RegisterBlasting("examplemod:ruby_from_ore_fast",   "examplemod:ruby_ore", "examplemod:ruby", experience: 0.7f, cookTimeSeconds: 5);
        McRecipe.RegisterSmoking("examplemod:cooked_mystery_smoked",  "minecraft:rotten_flesh", "examplemod:cooked_mystery");
        McRecipe.RegisterCampfire("examplemod:campfire_mystery",      "minecraft:rotten_flesh", "examplemod:cooked_mystery");
        McRecipe.RegisterStonecutting("examplemod:ruby_slab",        "examplemod:ruby_block", "examplemod:ruby_slab", count: 2);
    }

    // =========================================================================
    //  Commands
    // =========================================================================

    void RegisterCommands()
    {
        // /kit — give starter kit to yourself
        McCommand.Register("kit", (src) =>
        {
            McPlayer p = src.Player;
            if (p == null) { src.SendError("Players only!"); return; }
            p.GiveItem("examplemod:ruby_sword", 1);
            p.GiveItem("examplemod:ruby_pickaxe", 1);
            p.GiveItem("examplemod:ruby_helmet", 1);
            p.GiveItem("minecraft:bread", 32);
            p.GiveEffect("minecraft:speed", 600, 1);
            p.GiveEffect("minecraft:haste", 600, 1);
            src.SendMessage("Ruby kit granted!");
        });

        // /heal <target> — op only, heals a named player
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
            McPlayer p = src.Player;
            if (p == null) return;
            McWorld w = p.World;
            McBlockPos sp = w.SpawnPos;
            p.Teleport(sp.X, sp.Y, sp.Z);
            p.PlaySound("minecraft:entity.enderman.teleport");
            src.SendMessage("Teleported to spawn!");
        });

        // /ruby give <count> — subcommand with int arg
        McCommand.RegisterSubWithInt("ruby", "give", "count", (src, count) =>
        {
            McPlayer p = src.Player;
            if (p == null) { src.SendError("Players only!"); return; }
            p.GiveItem("examplemod:ruby", count);
            src.SendMessage("Gave " + count + " rubies.");
        });

        // /locate — show biome, dimension, position
        McCommand.Register("locate", (src) =>
        {
            McPlayer p = src.Player;
            if (p == null) return;
            string biome = p.GetBiome();
            string dim   = p.GetDimension();
            src.SendMessage("Biome: " + biome);
            src.SendMessage("Dimension: " + dim);
            src.SendMessage("X=" + p.X + " Y=" + p.Y + " Z=" + p.Z);
        });

        // /info — detailed player info
        McCommand.Register("info", (src) =>
        {
            McPlayer p = src.Player;
            if (p == null) return;
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
            McPlayer p = src.Player;
            if (p == null) return;
            int visits = PlayerData.GetInt(p, "visits", 0);
            src.SendMessage("You have visited " + visits + " times.");
        });

        // /settime <ticks> — op only, set world time
        McCommand.RegisterWithInt("settime", "ticks", (src, ticks) =>
        {
            McPlayer p = src.Player;
            if (p == null) return;
            McWorld w = p.World;
            w.SetTime(ticks);
            src.SendMessage("Time set to " + ticks + ".");
        });

        // /fill <blockId> — fill 5x5 area at feet with a block
        McCommand.RegisterWithString("fill", "blockId", (src, blockId) =>
        {
            McPlayer p = src.Player;
            if (p == null) return;
            McWorld w = p.World;
            int x = (int)p.X;
            int y = (int)p.Y - 1;
            int z = (int)p.Z;
            w.FillBlocks(x - 2, y, z - 2, x + 2, y, z + 2, blockId);
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
            McPlayer p = src.Player;
            if (p == null) return;
            McInventory inv = McInventory.FromPlayer(p);
            int rubies   = inv.Count("examplemod:ruby");
            int diamonds = inv.Count("minecraft:diamond");
            bool hasRuby = inv.Contains("examplemod:ruby");
            src.SendMessage("Inventory size: " + inv.Size);
            src.SendMessage("Rubies: " + rubies + "  Diamonds: " + diamonds);
            src.SendMessage("Has rubies: " + hasRuby);
        });

        // /weather <mode> — toggle weather
        McCommand.RegisterWithString("weather", "mode", (src, mode) =>
        {
            McPlayer p = src.Player;
            if (p == null) return;
            McWorld w = p.World;
            if (mode == "rain")
                w.SetWeather(0, 6000, true, false);
            else
                w.SetWeather(6000, 0, false, false);
            src.SendMessage("Weather set to " + mode + ".");
        });

        // /effect <effectId> — give a 60s effect to yourself
        McCommand.RegisterWithString("effect", "effectId", (src, effectId) =>
        {
            McPlayer p = src.Player;
            if (p == null) return;
            p.GiveEffect(effectId, 1200, 0);
            src.SendMessage("Applied " + effectId + " for 60 seconds.");
        });

        // /creative — op only, switch to creative
        McCommand.RegisterOp("creative", (src) =>
        {
            McPlayer p = src.Player;
            if (p != null) p.SetGameMode("creative");
            src.SendMessage("Game mode set to creative.");
        });

        // /survival — op only, switch to survival
        McCommand.RegisterOp("survival", (src) =>
        {
            McPlayer p = src.Player;
            if (p != null) p.SetGameMode("survival");
            src.SendMessage("Game mode set to survival.");
        });

        // /border <size> — animate world border
        McCommand.RegisterWithInt("border", "size", (src, size) =>
        {
            McPlayer p = src.Player;
            if (p == null) return;
            McWorld w = p.World;
            w.AnimateBorderSize(size, 5.0);
            src.SendMessage("World border animating to " + size + ".");
        });

        // /lightning — strike at feet
        McCommand.Register("lightning", (src) =>
        {
            McPlayer p = src.Player;
            if (p == null) return;
            McWorld w = p.World;
            w.SpawnLightning(p.X, p.Y, p.Z);
            src.SendMessage("Lightning!");
        });

        // /explode — small explosion at feet
        McCommand.Register("explode", (src) =>
        {
            McPlayer p = src.Player;
            if (p == null) return;
            McWorld w = p.World;
            w.CreateExplosion(p.X, p.Y, p.Z, 3.0f);
        });

        // /xp <amount> — give yourself XP
        McCommand.RegisterWithInt("xp", "amount", (src, amount) =>
        {
            McPlayer p = src.Player;
            if (p == null) return;
            p.GiveXp(amount);
            src.SendMessage("Gave " + amount + " XP.");
        });

        // /homeset — set home position
        McCommand.Register("homeset", (src) =>
        {
            McPlayer p = src.Player;
            if (p == null) return;
            PlayerData.Set(p, "home_x", (int)p.X);
            PlayerData.Set(p, "home_y", (int)p.Y);
            PlayerData.Set(p, "home_z", (int)p.Z);
            src.SendMessage("Home set!");
        });

        // /homego — teleport to home
        McCommand.Register("homego", (src) =>
        {
            McPlayer p = src.Player;
            if (p == null) return;
            if (!PlayerData.Has(p, "home_x"))
            {
                src.SendError("No home set. Use /homeset first.");
                return;
            }
            int hx = PlayerData.GetInt(p, "home_x", 0);
            int hy = PlayerData.GetInt(p, "home_y", 64);
            int hz = PlayerData.GetInt(p, "home_z", 0);
            p.Teleport(hx, hy, hz);
            p.PlaySound("minecraft:entity.enderman.teleport");
            src.SendMessage("Teleported home!");
        });

        // /nbt get <key>
        McCommand.RegisterSub("nbt", "get", "key", (src, key) =>
        {
            McPlayer p = src.Player;
            if (p == null) return;
            string val = p.GetNbtString(key);
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
            McServer srv = player.Server;

            int visits = player.GetNbtInt("visits");
            visits++;
            player.SetNbtInt("visits", visits);

            player.SendTitle("Welcome!", "Hello, " + player.Name);
            player.SendMessage("Welcome to the server! Visit #" + visits + ".");
            player.SendActionBar("Server has " + srv.PlayerCount + " player(s) online.");

            if (visits == 1)
            {
                player.GiveItem("minecraft:bread", 16);
                player.GiveItem("minecraft:wooden_sword", 1);
                player.SendMessage("First visit! Here's some bread and a sword.");
            }

            eventBar.AddPlayer(player);
            eventBar.SetTitle(player.Name + " joined!");
            eventBar.SetProgress(1.0f);

            McScoreboard.CreateObjective(srv, "kills", "Kills");
            McScoreboard.ShowSidebar(srv, "kills");
            if (!PlayerData.Has(player, "kills_init"))
            {
                McScoreboard.SetScore(srv, player, "kills", 0);
                PlayerData.Set(player, "kills_init", 1);
            }
            else
            {
                int savedKills = PlayerData.GetInt(player, "kills", 0);
                McScoreboard.SetScore(srv, player, "kills", savedKills);
            }

            McScoreboard.CreateTeam(srv, "players");
            McScoreboard.SetTeamColor(srv, "players", "green");
            McScoreboard.AddPlayerToTeam(srv, player, "players");

            McInventory inv = McInventory.FromPlayer(player);
            int rubyCount = inv.Count("examplemod:ruby");
            if (rubyCount > 0)
                player.SendMessage("You have " + rubyCount + " rubies.");
        };

        // ── Player leave ──────────────────────────────────────────────────────
        Events.PlayerLeave += (player) =>
        {
            McServer srv = player.Server;
            eventBar.RemovePlayer(player);
            int kills = McScoreboard.GetScore(srv, player, "kills");
            PlayerData.Set(player, "kills", kills);
            srv.Broadcast(player.Name + " left. Kills this session: " + kills + ".");
        };

        // ── Player death ──────────────────────────────────────────────────────
        Events.PlayerDeath += (player) =>
        {
            McWorld world = player.World;
            McServer srv  = player.Server;
            world.SpawnLightning(player.X, player.Y, player.Z);
            world.SpawnParticle("minecraft:explosion_emitter", player.X, player.Y + 1, player.Z, 3);
            srv.Broadcast(player.Name + " has fallen!");

            double currentSize = world.GetBorderSize();
            if (currentSize > 200)
            {
                world.AnimateBorderSize(currentSize - 10, 5.0);
                srv.Broadcast("World border shrinking to " + (currentSize - 10) + "!");
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
            McWorld bw   = player.World;
            McServer srv = player.Server;
            string block = bw.GetBlock(pos.X, pos.Y, pos.Z);

            if (block == "examplemod:ruby_ore")
            {
                player.GiveItem("examplemod:ruby", 2);
                bw.SpawnParticle("minecraft:glow", pos.X, pos.Y + 1, pos.Z, 20);
                bw.SpawnParticle("minecraft:crit",  pos.X, pos.Y + 1, pos.Z, 10);
                bw.PlaySound("examplemod:ruby_chime", pos.X, pos.Y, pos.Z);
                player.SendActionBar("Found rubies!");

                int score = McScoreboard.GetScore(srv, player, "kills");
                McScoreboard.SetScore(srv, player, "kills", score + 1);
                PlayerData.Set(player, "kills", score + 1);
            }

            if (block == "minecraft:diamond_ore" || block == "minecraft:deepslate_diamond_ore")
            {
                bw.SpawnParticle("minecraft:enchant", pos.X, pos.Y + 1, pos.Z, 30);
                player.SendActionBar("Diamonds!");
            }
        };

        // ── Block interact ────────────────────────────────────────────────────
        Events.BlockInteract += (player, pos) =>
        {
            McWorld bw = player.World;
            McBlockEntity be = bw.GetBlockEntity(pos.X, pos.Y, pos.Z);
            if (be != null && be.IsChest)
            {
                McInventory chest = be.GetInventory();
                if (chest != null && chest.Contains("examplemod:ruby"))
                {
                    player.SendMessage("This chest contains rubies!");
                    bw.SpawnParticle("minecraft:happy_villager", pos.X, pos.Y + 1, pos.Z, 10);
                }
            }
        };

        // ── Chat message ──────────────────────────────────────────────────────
        Events.ChatMessage += (player, message) =>
        {
            if (message == "!effects")
            {
                player.GiveEffect("minecraft:speed",       600, 1);
                player.GiveEffect("minecraft:night_vision", 600, 0);
                player.GiveEffect("minecraft:resistance",   600, 1);
                player.GiveEffect("minecraft:strength",     600, 1);
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
                world.SpawnParticle("minecraft:heart",            player.X, player.Y + 2, player.Z, 15);
                world.SpawnParticle("minecraft:totem_of_undying", player.X, player.Y + 1, player.Z, 20);
                world.SpawnParticle("minecraft:note",             player.X, player.Y + 3, player.Z, 10);
                world.SpawnParticle("minecraft:cherry_leaves",    player.X + 1, player.Y + 2, player.Z, 30);
                player.SendActionBar("Particles!");
            }

            if (message == "!sound")
            {
                player.PlaySound("minecraft:entity.experience_orb.pickup");
                McWorld world = player.World;
                world.PlaySound("minecraft:block.bell.use", player.X, player.Y, player.Z);
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
                McWorld ew = player.World;
                List<McEntity> nearby = ew.GetNearbyEntities(player.X, player.Y, player.Z, 16.0);
                foreach (McEntity e in nearby)
                {
                    player.SendMessage("  " + e.TypeId);
                }
            }
        };

        // ── Entity hurt — undead burn ─────────────────────────────────────────
        Events.EntityHurt += (entity, amount) =>
        {
            if (McTag.IsUndead(entity))
            {
                McWorld world = entity.World;
                entity.SetOnFire(3);
                world.SpawnParticle("minecraft:flame", entity.X, entity.Y + 1, entity.Z, 10);
            }
        };

        // ── Player attack — sword particle burst ──────────────────────────────
        Events.PlayerAttack += (player, target) =>
        {
            McItemStack main = player.MainHandItem;
            if (McTag.IsSword(main))
            {
                McWorld tw = target.World;
                tw.SpawnParticle("minecraft:crit",          target.X, target.Y + 1, target.Z, 15);
                tw.SpawnParticle("minecraft:enchanted_hit",  target.X, target.Y + 1, target.Z, 10);
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

        // ── Server start ──────────────────────────────────────────────────────
        Events.ServerStart += (server) =>
        {
            Console.WriteLine("ExampleMod initialized!");

            McScoreboard.CreateObjective(server, "kills", "Kills");
            McScoreboard.ShowSidebar(server, "kills");

            McScheduler.RunLater(server, 200, (s) =>
            {
                s.Broadcast("ExampleMod is ready! Type !effects, /kit, or /info.");
            });

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

        // ── World tick ────────────────────────────────────────────────────────
        Events.WorldTick += (world) =>
        {
            if (world.Time % 100 == 0 && world.IsRaining && world.IsDay)
            {
                // Just monitoring — clear rain would need server access
            }
        };

        // ── Entity spawn ──────────────────────────────────────────────────────
        Events.EntitySpawn += (entity) =>
        {
            if (entity.TypeId == "minecraft:wither")
            {
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
                McServer srv  = world.Server;
                world.SpawnParticle("minecraft:dragon_breath", entity.X, entity.Y + 5, entity.Z, 50);
                world.SpawnParticle("minecraft:end_rod",       entity.X, entity.Y + 5, entity.Z, 100);
                srv.Broadcast("The Ender Dragon has been defeated!");
            }
        };

        // ── Chunk / world load ────────────────────────────────────────────────
        Events.ChunkLoad += (world) => { };

        Events.WorldLoad += (world) =>
        {
            Console.WriteLine("World loaded: " + world.Dimension);
        };

        Events.WorldUnload += (world) =>
        {
            Console.WriteLine("World unloaded: " + world.Dimension);
        };

        // ── Loot table additions (build-time hints) ───────────────────────────
        LootTables.AddMobDrop("minecraft:zombie",  "examplemod:ruby", 0.05f, 1, 1);
        LootTables.AddMobDrop("minecraft:creeper", "examplemod:ruby", 0.1f,  1, 2);
        LootTables.AddChestLoot(LootChest.Dungeon,    "examplemod:ruby", 0.3f, 1, 3);
        LootTables.AddChestLoot(LootChest.Mineshaft,  "examplemod:ruby", 0.2f, 1, 2);
    }
}
