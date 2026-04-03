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
Events.PlayerJoin    += (player) => { ... };
Events.PlayerLeave   += (player) => { ... };
Events.ChatMessage   += (player, message) => { ... };
Events.BlockBreak    += (player, world, pos) => { ... };
Events.BlockPlace    += (player, world, pos) => { ... };
Events.BlockInteract += (player, world, pos) => { ... };
Events.ItemUse       += (player, world, stack) => { ... };
Events.ItemPickup    += (player, stack) => { ... };
Events.ItemFinishUsing += (player, stack) => { ... };
Events.ItemCraft     += (player, stack) => { ... };
Events.PlayerHurt    += (player, source, amount) => { ... };
Events.PlayerAttack  += (player, entity) => { ... };
Events.PlayerUseEntity += (player, entity) => { ... };
Events.EntityHurt    += (entity, source, amount) => { ... };
Events.ServerStart   += (server) => { ... };
Events.ServerStop    += (server) => { ... };
Events.ServerTick    += (server) => { ... };
Events.ServerTickStart += (server) => { ... };
Events.ServerLoading += (server) => { ... };
Events.ChunkLoad     += (world, chunk) => { ... };
Events.ChunkGenerate += (world, chunk) => { ... };
Events.CommandRegister += (dispatcher) => { ... };
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
double x = player.X;
double y = player.Y;
double z = player.Z;
```

### Items & inventory

```csharp
player.GiveItem("minecraft:diamond", 10);
player.ClearInventory();
```

### Effects

```csharp
player.GiveEffect("minecraft:speed", durationTicks: 200, amplifier: 1);
player.RemoveEffect("minecraft:speed");
player.ClearEffects();
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
McWorld world = player.World;
McServer server = player.Server;
```

---

## World API

```csharp
world.SetBlock(x, y, z, "minecraft:stone");
string id = world.GetBlock(x, y, z);
world.BreakBlock(x, y, z);
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
McRegistry.RegisterSword("mymod:ruby_sword", "DIAMOND", attackDamage: 7, attackSpeed: -2.4f);
McRegistry.RegisterPickaxe("mymod:ruby_pickaxe", "DIAMOND", attackDamage: 1, attackSpeed: -2.8f);
McRegistry.RegisterAxe("mymod:ruby_axe", "DIAMOND", attackDamage: 9f, attackSpeed: -3f);
McRegistry.RegisterShovel("mymod:ruby_shovel", "DIAMOND", attackDamage: 1.5f, attackSpeed: -3f);
McRegistry.RegisterHoe("mymod:ruby_hoe", "DIAMOND", attackDamage: 0, attackSpeed: -3f);

// Armor
McRegistry.RegisterHelmet("mymod:ruby_helmet", "DIAMOND");
McRegistry.RegisterChestplate("mymod:ruby_chestplate", "DIAMOND");
McRegistry.RegisterLeggings("mymod:ruby_leggings", "DIAMOND");
McRegistry.RegisterBoots("mymod:ruby_boots", "DIAMOND");
```

Valid tool material names: `WOOD`, `STONE`, `IRON`, `GOLD`, `DIAMOND`, `NETHERITE`.
Valid armor material names: `LEATHER`, `CHAINMAIL`, `IRON`, `GOLD`, `DIAMOND`, `NETHERITE`, `TURTLE`.

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

## Commands

Register chat commands with `/`:

```csharp
// No arguments
McCommand.Register("heal", (source) =>
{
    source.SendMessage("Healed!");
});

// String argument
McCommand.RegisterWithStringArg("give", (source, arg) =>
{
    source.SendMessage($"Giving {arg}");
});

// Integer argument
McCommand.RegisterWithIntArg("setlevel", (source, level) =>
{
    source.SendMessage($"Level set to {level}");
});

// Operator-only command
McCommand.RegisterOp("broadcast", (source) =>
{
    source.SendMessage("OP command executed");
});
```

---

## Status Effects & Enchantments

### Status effects

```csharp
// Apply an effect
player.GiveEffect("minecraft:strength", 200, 1);

// Check enchantment level on held item
int lvl = McEnchantment.GetLevel(player.Inventory.GetMainHandStack(), "minecraft:sharpness");
bool has = McEnchantment.HasEnchantment(player.Inventory.GetMainHandStack(), "minecraft:fire_aspect");
```

### Register a custom sound

```csharp
McRegistry.RegisterSound("mymod:my_sound");
```

---

## Sounds & Particles

Play sounds in the world or to a player:

```csharp
world.PlaySound("minecraft:entity.lightning_bolt.thunder", x, y, z);
player.PlaySound("minecraft:entity.experience_orb.pickup");
```

Spawn particles:

```csharp
world.SpawnParticle(McParticles.Flame, x, y, z, count: 30);
world.SpawnParticle(McParticles.Heart, x, y, z, count: 5);
```

Available particle constants are in `McParticles`: `Flame`, `Smoke`, `Heart`, `Explosion`, `Crit`, `Enchant`, `Portal`, `Splash`, `Rain`, `Dust`, `Snowball`, `Lava`, `Cloud`, `SporesBlossom`, `DragonBreath`, and more.

---

## Game Rules & Attributes

### Game rules

```csharp
bool mobSpawn  = McGameRules.DoMobSpawning(world);
bool keepInv   = McGameRules.KeepInventory(world);
bool pvp       = McGameRules.Pvp(world);
int tickSpeed  = McGameRules.RandomTickSpeed(world);
```

### Entity attributes

```csharp
// Read an attribute value
double speed = McAttribute.GetValue(player, McAttributes.MovementSpeed);

// Register a custom attribute
McRegistry.RegisterAttribute("mymod:jump_boost", defaultValue: 1.0, min: 0.0, max: 10.0);
```

---

## Recipes

> Crafting recipes are best defined as JSON files in `data/modid/recipes/`. The stub methods below emit TODO comments to remind you.

```csharp
McRecipe.RegisterShaped("mymod:my_pick", pattern, keys, result);
McRecipe.RegisterSmelting("mymod:ruby", input, result, experience: 0.5f);
```

Runtime recipe helpers:

```csharp
bool known = McRecipe.PlayerKnowsRecipe(player, "mymod:ruby");
McRecipe.UnlockForPlayer(player, "mymod:ruby");
McRecipe.LockForPlayer(player, "mymod:ruby");
```

---

## Potions

```csharp
// Query a potion stack
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
bool isFluid = McFluid.IsFluid(world, x, y, z);
bool isWater = McFluid.IsWater(world, x, y, z);
bool isLava  = McFluid.IsLava(world, x, y, z);
bool source  = McFluid.IsSource(world, x, y, z);
int level    = McFluid.GetLevel(world, x, y, z);
bool submerged = McFluid.IsPlayerSubmerged(player);
bool inLava    = McFluid.IsPlayerInLava(player);
```

---

## Structures

```csharp
// Check if a position is inside a structure
bool inside = McStructure.IsInsideStructure(world, x, y, z, "minecraft:village");

// Find the nearest structure
McBlockPos? pos = McStructure.FindNearest(world, "minecraft:stronghold");

// Place an NBT structure
McStructure.Place(world, x, y, z, "mymod:my_house");
```

> NBT structure files go in `data/modid/structure/`.

---

## Tags

Check block, item, and entity tags:

```csharp
// Block tags
bool isLog    = McTag.IsLog(world, x, y, z);
bool isLeaves = McTag.IsLeaves(world, x, y, z);
bool isDirt   = McTag.IsDirt(world, x, y, z);
bool isStone  = McTag.IsStone(world, x, y, z);
bool custom   = McTag.BlockIsIn(world, x, y, z, "mymod:my_tag");

// Item tags
bool isSword   = McTag.IsSword(stack);
bool isPickaxe = McTag.IsPickaxe(stack);
bool customItem = McTag.ItemIsIn(stack, "minecraft:swords");

// Entity tags
bool isUndead     = McTag.IsUndead(entity);
bool canBreathe   = McTag.CanBreatheUnderwater(entity);
bool customEntity = McTag.EntityIsIn(entity, "minecraft:skeletons");
```

> Tag JSON files go in `data/modid/tags/`.

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
// Drop loot at a world position
McLootTable.DropLoot(world, "minecraft:chests/simple_dungeon", x, y, z);

// Give loot directly to a player's inventory
McLootTable.GiveLootToPlayer(player, "mymod:my_loot_table");
```

> Loot table JSON files go in `data/modid/loot_table/`.

---

## Villager Trades

```csharp
// Sell trade: player pays emeralds, gets item
McVillager.AddSellTrade("minecraft:farmer", tradeLevel: 1, MY_ITEM, count: 3, emeraldCost: 5);

// Buy trade: player gives items, gets emeralds
McVillager.AddBuyTrade("minecraft:farmer", tradeLevel: 1, MY_ITEM, itemCount: 10, emeraldReward: 1);

// Query a villager entity
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
        // Register a custom block and item
        McRegistry.RegisterBlock("examplemod:ruby_ore", hardness: 3.0f);
        McRegistry.RegisterItem("examplemod:ruby");
        McRegistry.RegisterBlockItem("examplemod:ruby_ore_item", MY_BLOCK);
        McCreativeTab.AddToNaturalBlocks(MY_RUBY_ITEM);

        // Register a slash command (op-only)
        McCommand.RegisterOp("ruby", (source) =>
        {
            source.SendMessage("You got a ruby!");
        });

        // Welcome players and give a starter kit
        Events.PlayerJoin += (player) =>
        {
            player.SendMessage($"Welcome, {player.Name}!");
            player.GiveItem("minecraft:bread", 16);
        };

        // Simple chat commands
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

        // Log block breaks
        Events.BlockBreak += (player, world, pos) =>
        {
            Console.WriteLine($"{player.Name} broke a block at {pos.X},{pos.Y},{pos.Z}");
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
