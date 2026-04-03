# CSCraft

CSCraft lets you write Minecraft Fabric mods in C# instead of Java. You write mod logic in C#, and CSCraft transpiles it into a working Fabric mod `.jar`.

---

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- [Java 21+](https://adoptium.net/) — required to build the Fabric mod
- [Fabric Loader](https://fabricmc.net/use/installer/) installed in your Minecraft instance

---

## Building the CSCraft SDK (for SDK developers)

If you're working on CSCraft itself, build the SDK and repack the NuGet package after making changes:

```bash
dotnet build Transpiler/Transpiler.csproj -c Release
dotnet build BuildTask/BuildTask.csproj -c Release

cp Transpiler/bin/Release/net9.0/Transpiler.dll CSCraft.Sdk/build/tasks/
cp BuildTask/bin/Release/net9.0/BuildTask.dll   CSCraft.Sdk/build/tasks/

dotnet pack CSCraft.Sdk/CSCraft.Sdk.csproj -o nupkg
dotnet nuget locals all --clear
```

After clearing the cache, re-run `dotnet restore` in any mod project that references the SDK.

---

## Project Setup

### 1. Create a new C# class library

```bash
dotnet new classlib -n MyMod
cd MyMod
```

### 2. Add the CSCraft SDK

```bash
dotnet add package CSCraft.Sdk
```

### 3. Copy the FabricTemplate

Copy the `FabricTemplate/` folder into your project directory:

```
MyMod/
├── MyMod.csproj
├── MyMod.cs
└── FabricTemplate/
    ├── build.gradle
    ├── gradle.properties
    └── src/
```

### 4. Configure your `.csproj`

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>

    <!-- Reverse-domain package name for your mod -->
    <CSCraftPackageName>com.yourname.mymod</CSCraftPackageName>

    <!-- Path to the FabricTemplate folder -->
    <CSCraftFabricPath>$(MSBuildProjectDirectory)\FabricTemplate</CSCraftFabricPath>

    <!--
      Path to the Fabric resources directory (the one containing assets/ and data/).
      When set, recipe JSONs are generated automatically from McRecipe.Register* calls.
    -->
    <CSCraftResourcesPath>$(MSBuildProjectDirectory)\FabricTemplate\src\main\resources</CSCraftResourcesPath>

    <!-- Optional: full path to Java if not on PATH -->
    <!-- Linux:   /usr/lib/jvm/java-21-openjdk     -->
    <!-- Windows: C:\Program Files\Eclipse Adoptium\jdk-21... -->
    <CSCraftJavaHome></CSCraftJavaHome>

    <!-- Set to true to run Gradle automatically after dotnet build -->
    <CSCraftRunGrandle>true</CSCraftRunGrandle>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="CSCraft.Sdk" Version="1.0.0" />
  </ItemGroup>
</Project>
```

### 5. Configure FabricTemplate

#### `gradle.properties`

```properties
org.gradle.jvmargs=-Xmx1G
org.gradle.parallel=true
org.gradle.configuration-cache=false

minecraft_version=1.21.1
yarn_mappings=1.21.1+build.3
loader_version=0.18.6
loom_version=1.6.12

mod_version=1.0.0
maven_group=com.yourname.mymod
archives_base_name=mymod

fabric_api_version=0.116.9+1.21.1
```

#### `build.gradle`

```gradle
buildscript {
    repositories {
        maven { url = 'https://maven.fabricmc.net/' }
        mavenCentral()
        gradlePluginPortal()
    }
    dependencies {
        classpath "net.fabricmc:fabric-loom:${project.loom_version}"
    }
}

apply plugin: 'fabric-loom'
apply plugin: 'maven-publish'

version = project.mod_version
group   = project.maven_group
base { archivesName = project.archives_base_name }

repositories {
    maven { url = 'https://maven.fabricmc.net/' }
    mavenCentral()
}

loom {
    // splitEnvironmentSourceSets()  // keep commented out
}

dependencies {
    minecraft "com.mojang:minecraft:${project.minecraft_version}"
    mappings  "net.fabricmc:yarn:${project.yarn_mappings}:v2"
    modImplementation "net.fabricmc:fabric-loader:${project.loader_version}"
    modImplementation "net.fabricmc.fabric-api:fabric-api:${project.fabric_api_version}"
}

processResources {
    inputs.property "version", project.version
    filesMatching("fabric.mod.json") { expand "version": inputs.properties.version }
}

tasks.withType(JavaCompile).configureEach { it.options.release = 21 }
```

---

## Building a Mod

From your mod project directory:

```bash
dotnet build
```

If `CSCraftRunGrandle` is `true`, this also runs Gradle and produces the Fabric `.jar`. The output is in:

```
FabricTemplate/build/libs/
```

To run Gradle manually:

```bash
cd FabricTemplate
./gradlew build           # Linux/Mac
gradlew.bat build         # Windows
```

Copy the resulting `.jar` into your Minecraft `mods/` folder and launch with Fabric Loader.

---

## Writing a Mod

Every mod has one class implementing `IMod`:

```csharp
using CSCraft;

public class MyMod : IMod
{
    public void OnInitialize()
    {
        // Register events and content here
    }
}
```

`OnInitialize()` runs once when the server starts.

---

## Events

Subscribe to server-side events with `+=`:

```csharp
// Player lifecycle
Events.PlayerJoin        += (player) => { ... };
Events.PlayerLeave       += (player) => { ... };
Events.PlayerRespawn     += (player) => { ... };
Events.PlayerConnect     += (player) => { ... };   // connection established (before join)
Events.PlayerAllowDeath  += (player, source, amount) => { ... };  // return false to cancel
Events.PlayerCopyFrom    += (oldPlayer, newPlayer) => { ... };    // after respawn, copy data

// Chat & input
Events.ChatMessage        += (player, message) => { ... };
Events.ChatAllowed        += (player, message) => { ... };         // return false to block
Events.CommandMessageAllowed += (player, message) => { ... };

// Interaction
Events.BlockBreak         += (player, world, pos) => { ... };
Events.BlockBreakCanceled += (player, world, pos) => { ... };
Events.BlockInteract      += (player, world, pos) => { ... };
Events.BlockAttack        += (player, world, pos) => { ... };
Events.ItemUse            += (player, world, stack) => { ... };
Events.ItemPickup         += (player, stack) => { ... };
Events.ItemAfterPickup    += (player, stack) => { ... };
Events.PlayerHurt         += (player, source, amount) => { ... };
Events.PlayerAttack       += (player, entity) => { ... };
Events.PlayerUseEntity    += (player, entity) => { ... };

// Entity
Events.EntityHurt         += (entity, source, amount) => { ... };
Events.EntityAfterHurt    += (entity, source, amount) => { ... };
Events.EntityUnload       += (entity, world) => { ... };
Events.EntityAllowDeath   += (entity, source, amount) => { ... }; // return false to cancel

// Server lifecycle
Events.ServerStart        += (server) => { ... };
Events.ServerStopped      += (server) => { ... };
Events.ServerTick         += (server) => { ... };
Events.ServerTickStart    += (server) => { ... };
Events.ServerLoading      += (server) => { ... };
Events.DataPacksReload    += (server) => { ... };

// World
Events.WorldLoad          += (world) => { ... };
Events.WorldUnload        += (world) => { ... };
Events.WorldTickStart     += (world) => { ... };

// Block entity
Events.BlockEntityLoad    += (blockEntity, world) => { ... };
Events.BlockEntityUnload  += (blockEntity, world) => { ... };

// Chunks
Events.ChunkLoad          += (world, chunk) => { ... };
```

### Example — chat commands

```csharp
Events.ChatMessage += (player, message) =>
{
    if (message == "!heal")
    {
        player.Heal(20);
        player.SendMessage("Healed!");
    }
    else if (message == "!spawn")
    {
        player.Teleport(0, 64, 0);
        player.SendMessage("Teleported to spawn!");
    }
};
```

---

## Player API

### Chat & identity

```csharp
player.SendMessage("Hello!");
player.SendActionBar("Action bar text");
player.SendTitle("Title", "Subtitle");
string name = player.Name;
string uuid = player.Uuid;
```

### Health & food

```csharp
player.Heal(10);
player.Damage(5);
float hp    = player.Health;
float maxHp = player.MaxHealth;
int food    = player.FoodLevel;
player.SetFoodLevel(20);
```

### Movement & position

```csharp
player.Teleport(x, y, z);
player.LookAt(x, y, z);
double x   = player.X;
double y   = player.Y;
double z   = player.Z;
float yaw  = player.Yaw;
float pitch = player.Pitch;
```

### State checks

```csharp
bool swimming = player.IsSwimming;
bool gliding  = player.IsGliding;
bool flying   = player.IsFlying;
```

### Items & inventory

```csharp
player.GiveItem("minecraft:diamond", 10);
player.ClearInventory();
McItemStack main = player.MainHandItem;
McItemStack off  = player.OffHandItem;
```

### Armor

```csharp
McItemStack helmet     = player.Helmet;
McItemStack chestplate = player.Chestplate;
McItemStack leggings   = player.Leggings;
McItemStack boots      = player.Boots;

player.SetHelmet("minecraft:diamond_helmet");
player.SetChestplate("minecraft:diamond_chestplate");
player.SetLeggings("minecraft:diamond_leggings");
player.SetBoots("minecraft:diamond_boots");
```

### Effects

```csharp
player.GiveEffect("minecraft:speed", durationTicks: 200, amplifier: 1);
player.RemoveEffect("minecraft:speed");
player.ClearEffects();
bool hasSpeed = player.HasEffect("minecraft:speed");
List<string> active = player.GetActiveEffects();
```

### XP & game mode

```csharp
player.GiveXp(100);
int level = player.XpLevel;
player.SetXpLevel(5);
player.SetGameMode("creative");
```

### Permissions & misc

```csharp
bool op    = player.IsOp;
bool isOp2 = player.HasPermissionLevel(2);
player.Kick("You have been kicked.");
player.PlaySound("minecraft:entity.experience_orb.pickup");
player.SetSpawn(x, y, z);
```

### NBT data (persistent per-player storage)

```csharp
player.SetNbtString("myKey", "hello");
string val = player.GetNbtString("myKey");
player.SetNbtInt("score", 42);
int score  = player.GetNbtInt("score");
bool has   = player.HasNbt("myKey");
```

### Location info

```csharp
string biome = player.GetBiome();
string dim   = player.GetDimension();
McWorld world   = player.World;
McServer server = player.Server;
```

---

## Entity API

```csharp
// Position & rotation
double x    = entity.X;
double y    = entity.Y;
double z    = entity.Z;
float yaw   = entity.Yaw;
float pitch = entity.Pitch;
entity.Teleport(x, y, z);

// Health
float hp    = entity.Health;
float maxHp = entity.MaxHealth;

// State
bool onFire    = entity.IsOnFire;
bool invisible = entity.IsInvisible;
bool swimming  = entity.IsSwimming;
bool gliding   = entity.IsGliding;

entity.SetOnFire(5);         // burn for N seconds
entity.SetInvisible(true);

// Name
string name = entity.CustomName;
entity.SetCustomName("Bob");
entity.SetCustomNameVisible(true);

// Riding / passengers
List<McEntity> riders = entity.GetPassengers();
McEntity? vehicle = entity.GetVehicle();
entity.StartRiding(otherEntity);
entity.StopRiding();

// Scoreboard tags
bool has = entity.HasTag("my_tag");
entity.AddTag("my_tag");
entity.RemoveTag("my_tag");

// Identity
string type = entity.Type;
string uuid = entity.Uuid;
McWorld world = entity.World;
```

---

## Item Stack API

```csharp
McItemStack stack = player.MainHandItem;

// Identity & count
string id  = stack.ItemId;
int count  = stack.Count;
bool empty = stack.IsEmpty;
bool is    = stack.IsOf("minecraft:diamond");

// Display name
string name = stack.GetCustomName();
stack.SetCustomName("Shiny Diamond");

// Damage (tools)
int dmg = stack.GetDamage();
stack.SetDamage(100);

// Enchantments
stack.AddEnchantment("minecraft:sharpness", 5);

// NBT
stack.SetNbtString("myKey", "value");
string val = stack.GetNbtString("myKey");
stack.SetNbtInt("uses", 3);
int uses = stack.GetNbtInt("uses");
```

---

## World API

```csharp
world.SetBlock(x, y, z, "minecraft:stone");
string id = world.GetBlock(x, y, z);
world.BreakBlock(x, y, z);
world.FillBlocks(x1, y1, z1, x2, y2, z2, "minecraft:air");  // fill a region
world.SpawnEntity("minecraft:creeper", x, y, z);
world.CreateExplosion(x, y, z, power: 4);
world.SetTime(6000);
world.SetWeather(clearDuration, rainDuration, false, false);
int light  = world.GetLightLevel(x, y, z);
string bio = world.GetBiome(x, y, z);
int topY   = world.GetTopY(x, z);
bool inBorder = world.IsInBorder(x, y, z);
world.PlaySound("minecraft:block.note_block.bell", x, y, z);
world.SpawnParticle(McParticles.Flame, x, y, z, count: 20);
int rng = world.GetRandomInt(0, 100);

// Get all entities within radius
List<McEntity> nearby = world.GetNearbyEntities(x, y, z, radius: 10.0);
```

---

## Server API

```csharp
server.Broadcast("Hello everyone!");
McPlayer? player = server.GetPlayer("PlayerName");
McPlayer? byUuid = server.GetPlayerByUuid("550e8400-e29b-41d4-a716-446655440000");
List<McWorld> worlds = server.GetAllWorlds();
int ticks = server.CurrentTick;
```

---

## Registering Blocks and Items

Register blocks and items in `OnInitialize()`:

```csharp
// Block
McRegistry.RegisterBlock("mymod:ruby_ore", hardness: 3.0f);

// Item
McRegistry.RegisterItem("mymod:ruby");

// BlockItem (places the block when used)
McRegistry.RegisterBlockItem("mymod:ruby_ore", MY_BLOCK);

// Food
McRegistry.RegisterFood("mymod:mystery_meat", nutrition: 6, saturation: 0.8f);

// Tools
McRegistry.RegisterSword("mymod:ruby_sword", McToolMaterial.DIAMOND, bonusDamage: 3, attackSpeed: -2.4f);
McRegistry.RegisterPickaxe("mymod:ruby_pickaxe", McToolMaterial.DIAMOND);
McRegistry.RegisterAxe("mymod:ruby_axe", McToolMaterial.DIAMOND);
McRegistry.RegisterShovel("mymod:ruby_shovel", McToolMaterial.DIAMOND);
McRegistry.RegisterHoe("mymod:ruby_hoe", McToolMaterial.DIAMOND);

// Armor
McRegistry.RegisterHelmet("mymod:ruby_helmet", McArmorMaterial.DIAMOND);
McRegistry.RegisterChestplate("mymod:ruby_chestplate", McArmorMaterial.DIAMOND);
McRegistry.RegisterLeggings("mymod:ruby_leggings", McArmorMaterial.DIAMOND);
McRegistry.RegisterBoots("mymod:ruby_boots", McArmorMaterial.DIAMOND);

// Custom entity type
McEntityType myEntity = McRegistry.RegisterEntity("mymod:my_mob", "monster", width: 0.6f, height: 1.8f);

// Block entity type
McBlockEntityType myBE = McRegistry.RegisterBlockEntity("mymod:my_block_entity", MY_BLOCK);

// Custom sound
McRegistry.RegisterSound("mymod:my_sound");

// Custom attribute
McRegistry.RegisterAttribute("mymod:jump_boost", defaultValue: 1.0, min: 0.0, max: 10.0);
```

Valid tool material names: `WOOD`, `STONE`, `IRON`, `GOLD`, `DIAMOND`, `NETHERITE`.
Valid armor material names: `LEATHER`, `CHAINMAIL`, `IRON`, `GOLD`, `DIAMOND`, `NETHERITE`, `TURTLE`.

---

## Custom Game Rules

```csharp
// Register
McGameRule<bool> myRule = McRegistry.RegisterBoolRule("mymod:my_rule", defaultValue: true);
McGameRule<int>  myInt  = McRegistry.RegisterIntRule("mymod:max_wolves", defaultValue: 5);

// Read / write at runtime
bool val = myRule.GetValue(server);
myRule.SetValue(server, false);

// Built-in game rules
bool mobSpawn = McGameRules.DoMobSpawning(world);
bool keepInv  = McGameRules.KeepInventory(world);
int tickSpeed = McGameRules.RandomTickSpeed(world);
```

---

## Creative Tabs

Add items to existing creative tabs:

```csharp
McCreativeTab.AddToBuildingBlocks(MY_BLOCK_ITEM);
McCreativeTab.AddToNaturalBlocks(MY_BLOCK_ITEM);
McCreativeTab.AddToFunctional(MY_ITEM);
McCreativeTab.AddToRedstone(MY_ITEM);
McCreativeTab.AddToTools(MY_ITEM);
McCreativeTab.AddToCombat(MY_SWORD);
McCreativeTab.AddToFood(MY_FOOD);
McCreativeTab.AddToIngredients(MY_MATERIAL);
McCreativeTab.AddToSpawnEggs(MY_EGG);

// Custom tab by ID
McCreativeTab.AddToTab("minecraft:building_blocks", MY_ITEM);
```

---

## Recipes

CSCraft generates Minecraft recipe JSON files **automatically at build time**. Call `McRecipe.Register*()` in `OnInitialize()` with string-literal IDs; the build system writes the JSON into `data/modid/recipe/` before Gradle runs — no hand-written JSON files needed.

> All arguments must be string/char/numeric literals so the build system can read them. Variable references are skipped with a build warning (`CSCRAFT003`).

### Shaped crafting

```csharp
McRecipe.RegisterShaped(
    "mymod:ruby_sword",
    new[] { " R ", " R ", " S " },
    new object[] { 'R', "mymod:ruby", 'S', "minecraft:stick" },
    "mymod:ruby_sword");
```

### Shapeless crafting

```csharp
McRecipe.RegisterShapeless(
    "mymod:ruby_from_block",
    new[] { "mymod:ruby_block" },
    "mymod:ruby",
    count: 9);
```

### Smelting / cooking

```csharp
McRecipe.RegisterSmelting(
    "mymod:ruby_from_ore",
    "mymod:ruby_ore",
    "mymod:ruby",
    experience: 0.7f,
    cookTimeSeconds: 10);

McRecipe.RegisterBlasting(
    "mymod:ruby_from_ore_fast",
    "mymod:ruby_ore",
    "mymod:ruby",
    experience: 0.7f,
    cookTimeSeconds: 5);

McRecipe.RegisterSmoking("mymod:cooked_meat", "mymod:raw_meat", "mymod:cooked_meat");
McRecipe.RegisterCampfire("mymod:campfire_meat", "mymod:raw_meat", "mymod:cooked_meat");
```

### Stonecutting

```csharp
McRecipe.RegisterStonecutting("mymod:ruby_slab", "mymod:ruby_block", "mymod:ruby_slab", count: 2);
```

### Generated file location

Each recipe produces a JSON file at:

```
src/main/resources/data/<modId>/recipe/<recipeName>.json
```

For example, `"mymod:ruby_sword"` produces `data/mymod/recipe/ruby_sword.json`.

### Runtime recipe helpers

```csharp
bool known = McRecipe.PlayerKnowsRecipe(player, "mymod:ruby");
McRecipe.UnlockForPlayer(player, "mymod:ruby");
McRecipe.LockForPlayer(player, "mymod:ruby");
```

---

## Commands

Register Brigadier commands in `OnInitialize()`:

```csharp
// No arguments
McCommand.Register("heal", (source) =>
{
    source.Player?.Heal(20);
    source.SendMessage("Healed!");
});

// One string argument
McCommand.Register("give", "item", (source, item) =>
{
    source.Player?.GiveItem(item, 1);
});

// One integer argument
McCommand.Register("setlevel", "level", (source, level) =>
{
    source.Player?.SetXpLevel(level);
});

// Two string arguments
McCommand.Register("msg", "player", "message", (source, player, msg) =>
{
    source.SendMessage($"Sending '{msg}' to {player}");
});

// Subcommand  →  /myplugin reload
McCommand.RegisterSub("myplugin", "reload", (source) =>
{
    source.SendMessage("Reloaded!");
});

// Subcommand with argument  →  /myplugin ban <player>
McCommand.RegisterSub("myplugin", "ban", "player", (source, player) =>
{
    source.SendMessage($"Banned {player}");
});

// Operator-only commands
McCommand.RegisterOp("broadcast", (source) =>
{
    source.Server.Broadcast("Server message");
});

McCommand.RegisterOp("setrule", "value", (source, value) =>
{
    source.SendMessage($"Rule set to {value}");
});
```

---

## Tags

Check block, item, and entity tags:

```csharp
// Block tags
bool isLog    = McTag.IsLog(world, x, y, z);
bool isLeaves = McTag.IsLeaves(world, x, y, z);
bool isDirt   = McTag.IsDirt(world, x, y, z);
bool isStone  = McTag.IsStone(world, x, y, z);
bool custom   = McTag.BlockIsIn(world, x, y, z, "mymod:my_block_tag");
bool blockTag = McTag.IsInTag(block, "minecraft:logs");

// Item tags
bool isSword     = McTag.IsSword(stack);
bool isPickaxe   = McTag.IsPickaxe(stack);
bool isAxe       = McTag.IsAxe(stack);
bool isHoe       = McTag.IsHoe(stack);
bool isShovel    = McTag.IsShovel(stack);
bool isArmor     = McTag.IsArmor(stack);
bool isRanged    = McTag.IsRangedWeapon(stack);
bool isWearable  = McTag.IsWearable(stack);
bool customItem  = McTag.ItemIsIn(stack, "minecraft:swords");

// Entity tags
bool isUndead   = McTag.IsUndead(entity);
bool canBreathe = McTag.CanBreatheUnderwater(entity);
bool isBoss     = McTag.IsBoss(entity);
bool customEnt  = McTag.EntityIsIn(entity, "minecraft:skeletons");
```

> Tag JSON files go in `data/modid/tags/`.

---

## Status Effects & Enchantments

```csharp
// Apply / remove effects
player.GiveEffect("minecraft:strength", 200, 1);
player.RemoveEffect("minecraft:strength");
bool hasIt     = player.HasEffect("minecraft:speed");
List<string> active = player.GetActiveEffects();

// Check enchantments on an item stack
int lvl  = McEnchantment.GetLevel(player.MainHandItem, "minecraft:sharpness");
bool has = McEnchantment.HasEnchantment(player.MainHandItem, "minecraft:fire_aspect");

// Add enchantment to a stack
player.MainHandItem.AddEnchantment("minecraft:sharpness", 5);
```

---

## Sounds & Particles

```csharp
world.PlaySound("minecraft:entity.lightning_bolt.thunder", x, y, z);
player.PlaySound("minecraft:entity.experience_orb.pickup");

world.SpawnParticle(McParticles.Flame, x, y, z, count: 30);
world.SpawnParticle(McParticles.Heart, x, y, z, count: 5);
```

Available particle constants in `McParticles`: `Flame`, `Smoke`, `Heart`, `Explosion`, `Crit`, `Enchant`, `Portal`, `Splash`, `Rain`, `Dust`, `Snowball`, `Lava`, `Cloud`, `SporesBlossom`, `DragonBreath`, and more.

---

## Potions

```csharp
string potionId = McPotion.GetPotionId(stack);
bool hasSpeed   = McPotion.HasEffect(stack, "minecraft:speed");

// Register a custom brewing recipe
McPotion.RegisterBrewingRecipe("minecraft:awkward", MY_INGREDIENT, "minecraft:strength");
```

---

## Projectiles

```csharp
McProjectile.ThrowSnowball(player);
McProjectile.ShootArrow(player);
McProjectile.SpawnFireball(world, x, y, z);
McProjectile.ThrowPotion(player, "minecraft:harming");
```

---

## Fluids

```csharp
bool isFluid   = McFluid.IsFluid(world, x, y, z);
bool isWater   = McFluid.IsWater(world, x, y, z);
bool isLava    = McFluid.IsLava(world, x, y, z);
bool source    = McFluid.IsSource(world, x, y, z);
int level      = McFluid.GetLevel(world, x, y, z);
bool submerged = McFluid.IsPlayerSubmerged(player);
bool inLava    = McFluid.IsPlayerInLava(player);
```

---

## Structures

```csharp
bool inside    = McStructure.IsInsideStructure(world, x, y, z, "minecraft:village");
McBlockPos? pos = McStructure.FindNearest(world, "minecraft:stronghold");
McStructure.Place(world, x, y, z, "mymod:my_house");
```

> NBT structure files go in `data/modid/structure/`.

---

## Advancements

```csharp
McAdvancement.Grant(player, "mymod:my_advancement");
McAdvancement.Revoke(player, "mymod:my_advancement");
bool done = McAdvancement.HasCompleted(player, "mymod:my_advancement");
McAdvancement.GrantCriterion(player, "mymod:multi_step", "step_one");
```

> Advancement JSON files go in `data/modid/advancement/`.

---

## Loot Tables

```csharp
McLootTable.DropLoot(world, "minecraft:chests/simple_dungeon", x, y, z);
McLootTable.GiveLootToPlayer(player, "mymod:my_loot_table");
```

> Loot table JSON files go in `data/modid/loot_table/`.

---

## Villager Trades

```csharp
McVillager.AddSellTrade("minecraft:farmer", tradeLevel: 1, MY_ITEM, count: 3, emeraldCost: 5);
McVillager.AddBuyTrade("minecraft:farmer", tradeLevel: 1, MY_ITEM, itemCount: 10, emeraldReward: 1);

string profession = McVillager.GetProfession(villagerEntity);
int level         = McVillager.GetLevel(villagerEntity);
string type       = McVillager.GetType(villagerEntity);
```

---

## Full Example Mod

```csharp
using CSCraft;

public class ExampleMod : IMod
{
    public void OnInitialize()
    {
        // Register content
        McBlock rubyOre  = McRegistry.RegisterBlock("examplemod:ruby_ore", hardness: 3.0f);
        McItem  ruby     = McRegistry.RegisterItem("examplemod:ruby");
        McItem  rubyOreItem = McRegistry.RegisterBlockItem("examplemod:ruby_ore", rubyOre);
        McCreativeTab.AddToNaturalBlocks(rubyOreItem);
        McCreativeTab.AddToIngredients(ruby);

        // Recipes — JSON files are generated automatically at build time
        McRecipe.RegisterSmelting(
            "examplemod:ruby_from_ore",
            "examplemod:ruby_ore",
            "examplemod:ruby",
            experience: 0.7f);

        McRecipe.RegisterShaped(
            "examplemod:ruby_sword",
            new[] { " R ", " R ", " S " },
            new object[] { 'R', "examplemod:ruby", 'S', "minecraft:stick" },
            "examplemod:ruby_sword");

        // Register a custom boolean game rule
        McGameRule<bool> doubleDrops = McRegistry.RegisterBoolRule("examplemod:double_drops", false);

        // Commands
        McCommand.Register("ruby", (source) =>
        {
            source.Player?.GiveItem("examplemod:ruby", 1);
            source.SendMessage("Here's a ruby!");
        });

        McCommand.RegisterOp("setrule", "value", (source, value) =>
        {
            source.SendMessage($"Rule set to {value}");
        });

        // Events
        Events.PlayerJoin += (player) =>
        {
            player.SendMessage($"Welcome, {player.Name}!");
            player.SendTitle("Welcome", player.Name);
            player.GiveItem("minecraft:bread", 16);
        };

        Events.ChatMessage += (player, message) =>
        {
            if (message == "!heal")
            {
                player.Heal(20);
                player.SendMessage("Healed!");
            }
            else if (message == "!spawn")
            {
                player.Teleport(0, 64, 0);
                player.SendMessage("Teleported to spawn!");
            }
        };

        Events.BlockBreak += (player, world, pos) =>
        {
            string block = world.GetBlock(pos.X, pos.Y, pos.Z);
            if (block == "examplemod:ruby_ore")
            {
                player.GiveItem("examplemod:ruby", 1);
                player.SendMessage("Found a ruby!");
            }
        };

        Events.EntityHurt += (entity, source, amount) =>
        {
            if (McTag.IsUndead(entity))
                entity.SetOnFire(5);
        };

        Events.ServerStopped += (server) =>
        {
            Console.WriteLine("ExampleMod: server stopped cleanly.");
        };
    }
}
```

---

## Troubleshooting

**`Java not found`**
Add Java 21 to your system PATH, or set `<CSCraftJavaHome>` in your `.csproj`.

**`release version X not supported`**
Wrong Java version. Set `<CSCraftJavaHome>` to point to Java 21 specifically.

**`mappings not found` / Gradle errors**
Update `yarn_mappings` and `minecraft_version` in `gradle.properties` to values from [fabricmc.net/develop](https://fabricmc.net/develop).

**`Cannot configure layered mappings in a non-obfuscated environment`**
You are targeting a snapshot/pre-release Minecraft version. Use `loom_version=1.16-SNAPSHOT` and remove the `mappings` line from `build.gradle`.

**`net.minecraft.*` packages not found**
Make sure `loom_version` in `gradle.properties` is up to date. For Minecraft 26.x snapshots use `loom_version=1.16-SNAPSHOT`.

**Client mixin errors**
Keep `splitEnvironmentSourceSets()` commented out in `build.gradle`.

**Recipe JSON not generated**
Make sure `<CSCraftResourcesPath>` is set in your `.csproj` and all `McRecipe.Register*()` arguments are string/char/numeric literals — variable references are not supported at build time and will emit a `CSCRAFT003` warning.
