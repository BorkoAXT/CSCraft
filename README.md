# CSCraft

CSCraft lets you write Minecraft Fabric mods in C# instead of Java. You write mod logic in C#, and CSCraft transpiles it into a working Fabric mod `.jar` — no Gradle or Java knowledge required.

---

## Quick Start

### 1. Create a new project

```bash
dotnet new classlib -n MyMod
cd MyMod
dotnet add package CSCraft.Sdk
```

### 2. Write your mod

Replace `Class1.cs` with:

```csharp
using CSCraft;

[ModInfo(
    Id          = "mymod",
    Name        = "My Mod",
    Version     = "1.0.0",
    Author      = "YourName",
    Description = "My first C# Minecraft mod"
)]
public class MyMod : IMod
{
    public void OnInitialize()
    {
        Events.PlayerJoin += (player) =>
        {
            player.SendMessage($"Welcome, {player.Name}!");
            player.GiveItem("minecraft:diamond", 3);
        };
    }
}
```

### 3. Build

```bash
dotnet build
```

That's it. CSCraft reads the `[ModInfo]` attribute, generates the entire Fabric template, transpiles your C# to Java, and runs Gradle to produce the mod `.jar`.

The output is in `FabricTemplate/build/libs/`. Copy it to your Minecraft `mods/` folder and launch with Fabric Loader.

---

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- [Java 21+](https://adoptium.net/) (must be on PATH, or set `<CSCraftJavaHome>` in your .csproj)
- [Fabric Loader](https://fabricmc.net/use/installer/) installed in your Minecraft instance

---

## The `[ModInfo]` Attribute

Place this on your mod class to configure everything automatically:

```csharp
[ModInfo(
    Id               = "mymod",           // Required: lowercase mod ID
    Name             = "My Mod",          // Required: display name
    Version          = "1.0.0",           // Mod version (default: "1.0.0")
    Author           = "YourName",        // Shown in Fabric mod menu
    Description      = "A cool mod",      // Short description
    MinecraftVersion = "1.21.1",          // Target MC version (default: "1.21.1")
    PackageName      = "com.you.mymod"    // Java package (default: derived from Author + Id)
)]
public class MyMod : IMod { ... }
```

**Supported Minecraft versions:** 1.20.1, 1.20.2, 1.20.4, 1.20.6, 1.21, 1.21.1

CSCraft automatically resolves the correct Yarn mappings, Fabric Loader, Fabric API, and Loom versions.

---

## Manual Setup (Advanced)

If you prefer to manage the Fabric template yourself instead of using `[ModInfo]`, set these in your `.csproj`:

```xml
<PropertyGroup>
  <CSCraftPackageName>com.yourname.mymod</CSCraftPackageName>
  <CSCraftFabricPath>$(MSBuildProjectDirectory)\FabricTemplate</CSCraftFabricPath>
  <CSCraftResourcesPath>$(MSBuildProjectDirectory)\FabricTemplate\src\main\resources</CSCraftResourcesPath>
  <CSCraftJavaHome>C:\Program Files\Eclipse Adoptium\jdk-21</CSCraftJavaHome>
  <CSCraftRunGradle>true</CSCraftRunGradle>
</PropertyGroup>
```

---

## Building the SDK (for SDK developers)

If you're working on CSCraft itself:

```bash
dotnet build CSharpStubs/CSharpStubs.csproj -c Release
dotnet build Transpiler/Transpiler.csproj -c Release
dotnet build BuildTask/BuildTask.csproj -c Release

cp Transpiler/bin/Release/net9.0/Transpiler.dll CSCraft.Sdk/build/tasks/
cp BuildTask/bin/Release/net9.0/BuildTask.dll   CSCraft.Sdk/build/tasks/

dotnet pack CSCraft.Sdk/CSCraft.Sdk.csproj -o nupkg
dotnet nuget locals all --clear
```

---

## Events

Subscribe to server-side events in `OnInitialize()`:

```csharp
// Player lifecycle
Events.PlayerJoin    += (player) => { ... };
Events.PlayerLeave   += (player) => { ... };
Events.PlayerDeath   += (player) => { ... };
Events.PlayerRespawn += (player) => { ... };
Events.PlayerConnect += (player) => { ... };
Events.PlayerSleep   += (player, pos) => { ... };
Events.PlayerWakeUp  += (player) => { ... };

// Chat
Events.ChatMessage += (player, message) => { ... };

// Block interaction
Events.BlockBreak   += (player, pos) => { ... };
Events.BlockInteract += (player, pos) => { ... };
Events.BlockAttack   += (player, pos) => { ... };

// Combat
Events.PlayerHurt   += (player, amount) => { ... };
Events.PlayerAttack += (player, entity) => { ... };
Events.EntityHurt   += (entity, amount) => { ... };
Events.EntityDeath  += (entity) => { ... };

// Items
Events.ItemUse    += (player, stack) => { ... };
Events.ItemPickup += (player, stack) => { ... };

// Server lifecycle
Events.ServerStart += (server) => { ... };
Events.ServerStop  += (server) => { ... };
Events.ServerTick  += (server) => { ... };

// World
Events.WorldLoad += (world) => { ... };
Events.WorldTick += (world) => { ... };
Events.ChunkLoad += (world) => { ... };
```

---

## Player API

```csharp
// Identity
string name = player.Name;
string uuid = player.Uuid;
int ping    = player.GetPing();
string locale = player.GetLocale();

// Chat
player.SendMessage("Hello!");
player.SendActionBar("Action bar text");
player.SendTitle("Title", "Subtitle");

// Health & food
player.Heal(10);
player.Damage(5);
float hp   = player.Health;
int food   = player.FoodLevel;

// Movement
player.Teleport(x, y, z);
player.LookAt(x, y, z);
double px = player.X;

// State
bool sneaking  = player.IsSneaking;
bool flying    = player.IsFlying;
bool swimming  = player.IsSwimming;
bool sleeping  = player.IsSleeping;
bool spectator = player.IsSpectator;
bool blocking  = player.IsBlocking;

// Items & inventory
player.GiveItem("minecraft:diamond", 10);
player.ClearInventory();
McItemStack main = player.MainHandItem;
McItemStack helm = player.Helmet;

// Item cooldowns
player.SetCooldown("minecraft:ender_pearl", 60);
bool onCd = player.IsOnCooldown("minecraft:ender_pearl");

// Active item use
bool using  = player.IsUsingItem;
McItemStack active = player.GetActiveItem();

// Ender chest
McInventory enderChest = player.GetEnderChest();

// Effects
player.GiveEffect("minecraft:speed", 200, 1);
player.RemoveEffect("minecraft:speed");
bool hasSpeed = player.HasEffect("minecraft:speed");
player.ClearEffects();

// XP & game mode
player.GiveXp(100);
player.SetXpLevel(5);
player.SetGameMode("creative");

// Permissions
bool op = player.IsOp();
player.Kick("Banned.");

// Sleep
player.WakeUp();

// UI
player.CloseInventory();

// Location
string biome = player.GetBiome();
string dim   = player.GetDimension();

// NBT (persistent per-player data)
player.SetNbtString("key", "value");
player.SetNbtInt("score", 42);
int score = player.GetNbtInt("score");
bool has  = player.HasNbt("key");
```

---

## Entity API

```csharp
// Identity
string type = entity.TypeId;    // e.g. "minecraft:zombie"
string uuid = entity.Uuid;
bool isMob  = entity.IsMob;
bool isPlayer = entity.IsPlayer;

// Position & velocity
double x = entity.X;
entity.Teleport(x, y, z);
entity.SetVelocity(0, 1, 0);   // launch upward
double vx = entity.VelocityX;

// Health
float hp = entity.Health;
entity.Kill();
entity.Remove();

// State
entity.SetOnFire(5);
entity.SetInvisible(true);
entity.SetNoGravity(true);
entity.SetCustomName("Boss");
entity.SetCustomNameVisible(true);

// Age (passive mobs)
entity.SetBaby(true);
int age = entity.GetAge();

// Tags
entity.AddTag("marked");
bool has = entity.HasTag("marked");

// Riding
entity.StartRiding(otherEntity);
entity.StopRiding();

// NBT
entity.SetNbtString("owner", "Steve");
```

---

## World API

```csharp
// Blocks
world.SetBlock(x, y, z, "minecraft:stone");
string id = world.GetBlock(x, y, z);
world.BreakBlock(x, y, z);
world.FillBlocks(x1, y1, z1, x2, y2, z2, "minecraft:air");
bool air = world.IsAir(x, y, z);

// Entities
McEntity mob = world.SpawnEntity("minecraft:creeper", x, y, z);
List<McEntity> nearby = world.GetNearbyEntities(x, y, z, 10.0);

// Item drops
world.DropItem("minecraft:diamond", x, y, z, 3);

// Weather & time
world.SetTime(6000);
world.SetWeather(0, 6000, true, false);
bool day = world.IsDay;

// Lightning
world.SpawnLightning(x, y, z);

// Effects
world.CreateExplosion(x, y, z, 4.0f);
world.SpawnParticle(McParticles.Flame, x, y, z, 30);
world.PlaySound("minecraft:entity.lightning_bolt.thunder", x, y, z);

// Info
int light = world.GetLightLevel(x, y, z);
string biome = world.GetBiome(x, y, z);
int topY = world.GetTopY(x, z);
bool loaded = world.IsChunkLoaded(x, z);

// Block entities (chests, furnaces, etc.)
McBlockEntity? be = world.GetBlockEntity(x, y, z);
if (be != null && be.IsChest)
{
    McInventory? inv = be.GetInventory();
}

// World border
double size = world.GetBorderSize();
world.SetBorderSize(1000);
world.AnimateBorderSize(500, 60.0);   // shrink to 500 over 60 seconds
world.SetBorderCenter(0, 0);
```

---

## Server API

```csharp
server.Broadcast("Hello everyone!");
McPlayer? p = server.GetPlayer("Steve");
List<McWorld> worlds = server.GetAllWorlds();
server.RunCommand("say Hello from console");

// Properties
int count  = server.PlayerCount;
int max    = server.MaxPlayers;
string ver = server.Version;
bool hc    = server.IsHardcore;
long seed  = server.GetSeed();

// Game mode
string mode = server.GetDefaultGameMode();
server.SetDefaultGameMode("survival");

// Ban list
server.BanPlayer("Griefer", "Griefing");
server.PardonPlayer("Griefer");
bool banned = server.IsPlayerBanned("Griefer");

// Whitelist
server.SetWhitelistEnabled(true);
server.AddToWhitelist("Friend");
bool wl = server.IsWhitelisted("Friend");

// Worlds
McWorld overworld = server.Overworld;
McWorld nether    = server.Nether;
McWorld end       = server.End;
```

---

## Boss Bars

```csharp
// Create a boss bar
McBossBar bar = new McBossBar("Raid Progress", McBossBar.BarColor.Red);
bar.SetProgress(0.75f);           // 0.0 to 1.0
bar.SetVisible(true);

// Show to players
bar.AddPlayer(player);
bar.AddAllPlayers(server);
bar.RemovePlayer(player);
bar.RemoveAllPlayers();

// Customize
bar.SetTitle("New Title");
bar.SetColor("blue");
bar.SetDarkenSky(true);
bar.SetThickenFog(true);
```

---

## Scoreboard

```csharp
// Create objectives
McScoreboard.CreateObjective(server, "kills", "Player Kills");
McScoreboard.ShowSidebar(server, "kills");
McScoreboard.ShowBelowName(server, "kills");

// Scores
McScoreboard.SetScore(server, player, "kills", 10);
int kills = McScoreboard.GetScore(server, player, "kills");
McScoreboard.AddScore(server, player, "kills", 1);
McScoreboard.ResetScore(server, player, "kills");

// Teams
McScoreboard.CreateTeam(server, "red");
McScoreboard.SetTeamColor(server, "red", "red");
McScoreboard.SetTeamPrefix(server, "red", "[RED] ");
McScoreboard.AddPlayerToTeam(server, player, "red");
McScoreboard.SetFriendlyFire(server, "red", false);
```

---

## Scheduler (Delayed & Repeating Tasks)

```csharp
// Run after 5 seconds (100 ticks)
McScheduler.RunLater(server, 100, (s) =>
{
    s.Broadcast("5 seconds have passed!");
});

// Run every second (20 ticks)
McScheduler.RunRepeating(server, 20, (s, cancel) =>
{
    s.Broadcast("Tick!");
    // Call cancel() to stop the repeating task
});

// Run off the main thread (don't call MC API from here)
McScheduler.RunAsync(server, () =>
{
    // Heavy computation, file I/O, HTTP requests, etc.
});
```

---

## Inventory

```csharp
// Player inventory
McInventory inv = McInventory.FromPlayer(player);
McItemStack slot0 = inv.GetSlot(0);       // hotbar slot 1
inv.SetSlot(0, new McItemStack("minecraft:diamond", 64));
inv.Clear();

// Search
int count = inv.Count("minecraft:diamond");
bool has  = inv.Contains("minecraft:diamond");
int slot  = inv.FindSlot("minecraft:diamond");

// Give/take items
McInventory.Give(player, new McItemStack("minecraft:diamond", 10));
McInventory.Take(player, "minecraft:dirt", 64);

// Ender chest
McInventory ender = McInventory.EnderChest(player);
```

---

## NBT (Persistent Data)

```csharp
// Create a compound
McNbt nbt = new McNbt();
nbt.SetString("name", "Steve");
nbt.SetInt("level", 5);
nbt.SetBool("vip", true);
nbt.SetDouble("balance", 1000.50);

// Read
string name = nbt.GetString("name");
bool has    = nbt.Has("name");
List<string> keys = nbt.GetKeys();

// Nested compounds
McNbt inner = new McNbt();
inner.SetString("type", "quest");
nbt.SetCompound("data", inner);

// Entity NBT
McNbt entityData = McNbt.FromEntity(entity);
McNbt.ToEntity(entity, nbt);

// Item NBT
McNbt itemData = McNbt.FromItem(stack);
McNbt.ToItem(stack, nbt);
```

---

## Damage Types

```csharp
// Deal typed damage
McDamage.DealDamage(entity, McDamage.Fire, 5.0f);
McDamage.DealDamage(entity, McDamage.Magic, 10.0f);
McDamage.DealFallDamage(entity, 20.0f);
McDamage.DealFireDamage(entity, 3.0f);
McDamage.DealMagicDamage(entity, 8.0f);

// Built-in damage type IDs
// McDamage.Generic, McDamage.Fire, McDamage.Lava, McDamage.Drown,
// McDamage.Starve, McDamage.Fall, McDamage.Magic, McDamage.Wither,
// McDamage.Lightning, McDamage.Explosion, McDamage.Arrow,
// McDamage.Freeze, McDamage.SonicBoom, and more
```

---

## Commands

```csharp
// No arguments
McCommand.Register("heal", (src) => src.Player?.Heal(20));

// String argument
McCommand.Register("say", "message", (src, msg) => src.Server.Broadcast(msg));

// Integer argument
McCommand.Register("setlevel", "level", (src, lvl) => src.Player?.SetXpLevel(lvl));

// Float argument
McCommand.Register("sethp", "amount", (src, hp) => src.Player?.Heal(hp));

// Boolean argument
McCommand.Register("fly", "enabled", (src, on) => { ... });

// Player selector — resolves to an online player
McCommand.RegisterWithPlayer("heal", "target", (src, target) =>
{
    target.Heal(target.MaxHealth);
    src.SendMessage($"Healed {target.Name}");
});

// Player selector + int argument
McCommand.RegisterWithPlayer("give", "target", "count", (src, target, count) =>
{
    target.GiveItem("minecraft:diamond", count);
});

// BlockPos argument
McCommand.RegisterWithPos("setblock", "pos", (src, pos) =>
{
    src.Player?.World.SetBlock(pos.X, pos.Y, pos.Z, "minecraft:stone");
});

// Subcommands:  /myplugin reload
McCommand.RegisterSub("myplugin", "reload", (src) => { ... });

// Operator-only
McCommand.RegisterOp("ban", (src) => { ... });
McCommand.RegisterOpWithPlayer("kick", "target", (src, target) => target.Kick("Kicked"));
```

---

## Block & Item Registration

```csharp
// Blocks
McBlock myBlock = McRegistry.RegisterBlock("mymod:my_block", hardness: 3.0f);
McItem blockItem = McRegistry.RegisterBlockItem("mymod:my_block", myBlock);

// Items
McItem myItem = McRegistry.RegisterItem("mymod:my_item");
McItem food = McRegistry.RegisterFood("mymod:my_food", 6, 0.8f, meat: true);

// Tools
McItem sword   = McRegistry.RegisterSword("mymod:my_sword", McToolMaterial.Diamond);
McItem pickaxe = McRegistry.RegisterPickaxe("mymod:my_pick", McToolMaterial.Iron);
McItem axe     = McRegistry.RegisterAxe("mymod:my_axe", McToolMaterial.Diamond);
McItem shovel  = McRegistry.RegisterShovel("mymod:my_shovel", McToolMaterial.Iron);
McItem hoe     = McRegistry.RegisterHoe("mymod:my_hoe", McToolMaterial.Stone);

// Armor
McItem helmet = McRegistry.RegisterHelmet("mymod:my_helmet", McArmorMaterial.Diamond);
McItem chest  = McRegistry.RegisterChestplate("mymod:my_chest", McArmorMaterial.Diamond);
McItem legs   = McRegistry.RegisterLeggings("mymod:my_legs", McArmorMaterial.Diamond);
McItem boots  = McRegistry.RegisterBoots("mymod:my_boots", McArmorMaterial.Diamond);

// Creative tabs
McCreativeTab.AddToCombat(sword, helmet);
McCreativeTab.AddToTools(pickaxe);
McCreativeTab.AddToFood(food);
```

---

## Recipes

Recipe JSON files are generated automatically at build time from `McRecipe.Register*()` calls. All arguments must be compile-time literals.

```csharp
// Shaped crafting
McRecipe.RegisterShaped("mymod:my_sword",
    new[] { " R ", " R ", " S " },
    new object[] { 'R', "mymod:ruby", 'S', "minecraft:stick" },
    "mymod:my_sword");

// Shapeless
McRecipe.RegisterShapeless("mymod:ruby_x9",
    new[] { "mymod:ruby_block" }, "mymod:ruby", count: 9);

// Smelting / blasting / smoking / campfire
McRecipe.RegisterSmelting("mymod:smelt", "mymod:ore", "mymod:ingot", 0.7f, 10);
McRecipe.RegisterBlasting("mymod:blast", "mymod:ore", "mymod:ingot", 0.7f, 5);

// Stonecutting
McRecipe.RegisterStonecutting("mymod:slab", "mymod:block", "mymod:slab", count: 2);
```

---

## Auto-Generated Resources (Models, Blockstates, Lang)

CSCraft automatically generates Minecraft resource files at build time from your `McRegistry.Register*()` calls. No manual JSON writing required.

**What gets generated:**

| Registration Call | Generated Files |
|---|---|
| `RegisterBlock("mymod:ruby_block", ...)` | `blockstates/ruby_block.json`, `models/block/ruby_block.json`, `lang/en_us.json` entry |
| `RegisterBlockItem("mymod:ruby_block", ...)` | `models/item/ruby_block.json` (inherits block model) |
| `RegisterItem("mymod:ruby")` | `models/item/ruby.json` (generated parent), lang entry |
| `RegisterFood(...)` | `models/item/{name}.json` (generated parent), lang entry |
| `RegisterSword/Pickaxe/Axe/Shovel/Hoe(...)` | `models/item/{name}.json` (handheld parent), lang entry |
| `RegisterHelmet/Chestplate/Leggings/Boots(...)` | `models/item/{name}.json` (generated parent), lang entry |

**Language entries** are merged into a single `assets/{modId}/lang/en_us.json`. Names are derived automatically: `ruby_ore` becomes `"Ruby Ore"`.

**Textures** must still be provided manually. Place them in:
- `FabricTemplate/src/main/resources/assets/{modId}/textures/item/{name}.png` for items
- `FabricTemplate/src/main/resources/assets/{modId}/textures/block/{name}.png` for blocks

All arguments to registration calls must be compile-time string literals (same as recipes). Variable references emit a CSCRAFT004 warning.

---

## Tags, Fluids, Structures, Advancements

```csharp
// Block tags
bool isLog = McTag.IsLog(world, x, y, z);
bool custom = McTag.BlockIsIn(world, x, y, z, "mymod:my_tag");

// Item tags
bool isSword = McTag.IsSword(stack);

// Entity tags
bool isUndead = McTag.IsUndead(entity);

// Fluids
bool water = McFluid.IsWater(world, x, y, z);
bool submerged = McFluid.IsPlayerSubmerged(player);

// Structures
bool inVillage = McStructure.IsInsideStructure(world, x, y, z, "minecraft:village");
McBlockPos? pos = McStructure.FindNearest(world, "minecraft:stronghold");

// Advancements
McAdvancement.Grant(player, "mymod:first_ruby");
bool done = McAdvancement.HasCompleted(player, "mymod:first_ruby");

// Loot tables
McLootTable.GiveLootToPlayer(player, "mymod:reward_chest");

// Villager trades
McVillager.AddSellTrade("minecraft:toolsmith", 2, myItem, 1, 5);

// Potions
McPotion.RegisterBrewingRecipe("minecraft:awkward", ingredient, "minecraft:strength");

// Projectiles
McProjectile.ShootArrow(player);
McProjectile.ThrowSnowball(player);
```

---

## Game Rules

```csharp
// Custom rules
McGameRule<bool> myRule = McRegistry.RegisterBoolRule("mymod:pvp_drops", true);
McGameRule<int> myInt = McRegistry.RegisterIntRule("mymod:max_homes", 3);

bool val = myRule.GetValue(server);
myRule.SetValue(server, false);

// Built-in rules
bool keepInv = McGameRules.KeepInventory(server);
bool pvp = McGameRules.Pvp(server);
int tickSpeed = McGameRules.RandomTickSpeed(server);
```

---

## Sounds & Particles

```csharp
// Sounds
world.PlaySound("minecraft:entity.lightning_bolt.thunder", x, y, z);
player.PlaySound("minecraft:entity.experience_orb.pickup");

// Particles (use McParticles constants)
world.SpawnParticle(McParticles.Flame, x, y, z, 30);
world.SpawnParticle(McParticles.Heart, x, y, z, 5);
player.SpawnParticle(McParticles.End, x, y, z, 10);

// Available: Flame, Smoke, Heart, Explosion, Crit, MagicCrit, Splash,
// Portal, EnchantTable, Witch, Snow, Lava, End, Dragon, Bubble, Note,
// XP, Campfire, Dust, Totem, Warped, and more
```

---

## Attributes

```csharp
// Read/modify entity attributes
double speed = McAttribute.GetValue(entity, McAttributes.MovementSpeed);
McAttribute.AddModifier(entity, McAttributes.MaxHealth, 10.0, 0); // +10 max HP

// Available: MaxHealth, AttackDamage, AttackSpeed, MovementSpeed,
// FlyingSpeed, Armor, ArmorToughness, Knockback, KnockbackResist,
// FollowRange, ReachDistance, BlockInteractRange, Luck
```

---

## Troubleshooting

**`Java not found`** — Add Java 21 to your PATH, or set `<CSCraftJavaHome>` in your .csproj.

**`release version X not supported`** — Wrong Java version. Point `<CSCraftJavaHome>` to Java 21.

**`mappings not found`** — Update `MinecraftVersion` in your `[ModInfo]` attribute to a supported version.

**Recipe JSON not generated** — All `McRecipe.Register*()` arguments must be string/char/numeric literals. Variables emit a CSCRAFT003 warning.

**Model/blockstate JSON not generated** — All `McRegistry.Register*()` arguments must be string literals. Variables emit a CSCRAFT004 warning.

**Build succeeds but no `.jar`** — Check that `<CSCraftRunGradle>` is `true` (the default) and Java is available.
