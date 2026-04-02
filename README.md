# CSCraft Documentation

CSCraft lets you write Minecraft Fabric mods in C# instead of Java. You write mod logic in C#, and CSCraft compiles it into a working Fabric mod `.jar`.

---

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- [Java 21](https://adoptium.net/) — required to build the Fabric mod
- [Fabric Loader](https://fabricmc.net/use/installer/) installed in your Minecraft instance

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

### 3. Set up the FabricTemplate

Copy the FabricTemplate folder (included with CSCraft) into your project directory so it sits next to your `.csproj` file:

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

    <!-- Your mod's package name (reverse domain style) -->
    <CSCraftPackageName>com.yourname.mymod</CSCraftPackageName>

    <!-- Path to your FabricTemplate folder -->
    <CSCraftFabricPath>$(MSBuildProjectDirectory)\FabricTemplate</CSCraftFabricPath>

    <!-- Optional: path to Java 21 if not on PATH -->
    <!-- Windows: C:\Program Files\Eclipse Adoptium\jdk-21... -->
    <!-- Linux:   /usr/lib/jvm/java-21-openjdk             -->
    <CSCraftJavaHome></CSCraftJavaHome>

    <!-- Set to true to automatically run Gradle after build -->
    <CSCraftRunGrandle>true</CSCraftRunGrandle>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="CSCraft.Sdk" Version="1.0.0" />
  </ItemGroup>

</Project>
```

### 5. Configure FabricTemplate

Ensure your FabricTemplate folder contains the correct build.gradle and gradle.properties files. If the template is outdated or has hardcoded versions, update them to use properties and disable split source sets to avoid compilation issues.

#### gradle.properties
```properties
# Done to increase the memory available to gradle.
org.gradle.jvmargs=-Xmx1G
org.gradle.parallel=true
org.gradle.configuration-cache=false

# Fabric Properties
# check these on https://fabricmc.net/develop
minecraft_version=1.21.1
yarn_mappings=1.21.1+build.3
loader_version=0.18.6
loom_version=1.6.12

# Mod Properties
mod_version=1.0.0
maven_group=com.yourname.mymod
archives_base_name=mymod

# Dependencies
fabric_api_version=0.116.9+1.21.1
```

#### build.gradle
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
group = project.maven_group

base {
    archivesName = project.archives_base_name
}

repositories {
    maven { url = 'https://maven.fabricmc.net/' }
    mavenCentral()
}

loom {
    // splitEnvironmentSourceSets()
    // no need to manually assign sourceSets.client
}

dependencies {
    minecraft "com.mojang:minecraft:${project.minecraft_version}"
    mappings "net.fabricmc:yarn:${project.yarn_mappings}:v2"
    modImplementation "net.fabricmc:fabric-loader:${project.loader_version}"
    modImplementation "net.fabricmc.fabric-api:fabric-api:${project.fabric_api_version}"
}

processResources {
    inputs.property "version", project.version

    filesMatching("fabric.mod.json") {
        expand "version": inputs.properties.version
    }
}

tasks.withType(JavaCompile).configureEach {
    it.options.release = 21
}
```

**Important Notes:**
- Always use `${project.property}` references in build.gradle instead of hardcoded versions to ensure the properties in gradle.properties are used.
- Comment out `splitEnvironmentSourceSets()` to prevent classpath issues with client-side mixins; the client source set will inherit from the main source set.
- Regularly update the versions in gradle.properties to the latest compatible with your Minecraft version (check https://fabricmc.net/develop for the latest recommended versions).

---

## Tutorial: Creating Your First Mod

Follow this step-by-step guide to create a simple mod that welcomes players, gives them starter items, and adds chat commands.

### Step 1: Set Up the Project

1. Create a new C# class library: `dotnet new classlib -n MyFirstMod`
2. Navigate to the directory: `cd MyFirstMod`
3. Add the CSCraft SDK: `dotnet add package CSCraft.Sdk`
4. Copy the FabricTemplate folder into your project directory.
5. Update your `.csproj` file as shown in the Project Setup section.
6. Configure build.gradle and gradle.properties as detailed above, ensuring versions are up-to-date and `splitEnvironmentSourceSets()` is commented out.

### Step 2: Write the Mod Code

Create a new file `MyFirstMod.cs` in your project root:

```csharp
using CSCraft;

public class MyFirstMod : IMod
{
    public void OnInitialize()
    {
        // Welcome message and starter kit on player join
        Events.PlayerJoin += (player) =>
        {
            player.SendMessage($"Welcome to the server, {player.Name}!");
            player.GiveItem("minecraft:bread", 16);
            player.GiveItem("minecraft:iron_sword", 1);
        };

        // Simple chat commands
        Events.ChatMessage += (player, message) =>
        {
            if (message == "!heal")
            {
                player.Heal(20);
                player.SendMessage("You have been healed!");
            }
            else if (message == "!spawn")
            {
                player.Teleport(0, 64, 0);
                player.SendMessage("Teleported to spawn!");
            }
        };
    }
}
```

### Step 3: Build and Test

1. Build the project: `dotnet build`
   - This compiles your C# code and runs Gradle to build the Fabric mod.
2. Locate the output `.jar` in libs.
3. Copy the `.jar` to your Minecraft `mods/` folder.
4. Ensure Fabric Loader is installed in your Minecraft launcher.
5. Launch Minecraft and join a world.
6. Test the mod: Join as a new player to receive the welcome message and items. Use `!heal` and `!spawn` in chat.

Congratulations! You've created your first CSCraft mod. Experiment with more events and player methods to expand functionality.

---

## Writing Mods

Every mod must have one class implementing `IMod`:

```csharp
using CSCraft;

public class MyMod : IMod
{
    public void OnInitialize()
    {
        // Your mod logic goes here
    }
}
```

`OnInitialize()` is called once when the server starts. Register all your events here.

---

## Events

### Player Join

```csharp
Events.PlayerJoin += (player) =>
{
    player.SendMessage($"Welcome, {player.Name}!");
};
```

### Player Leave

```csharp
Events.PlayerLeave += (player) =>
{
    player.SendMessage("Goodbye!");
};
```

### Chat Message

```csharp
Events.ChatMessage += (player, message) =>
{
    if (message == "!hello")
    {
        player.SendMessage("Hello!");
    }
};
```

### Block Break

```csharp
Events.BlockBreak += (player, pos) =>
{
    player.SendMessage($"Broke block at {pos.X}, {pos.Y}, {pos.Z}");
};
```

---

## Player API

| Method | Description |
|---|---|
| `player.SendMessage(string)` | Send a chat message to the player |
| `player.GiveItem(string id, int count)` | Give items (use Minecraft item IDs) |
| `player.Heal(int amount)` | Restore health |
| `player.Teleport(double x, double y, double z)` | Teleport the player |
| `player.SetGameMode(string mode)` | Set gamemode (`"survival"`, `"creative"`, `"adventure"`, `"spectator"`) |
| `player.Name` | The player's username |

---

## Building

From your project directory:

```bash
dotnet build
```

If `CSCraftRunGrandle` is `true`, this will also compile the Fabric mod automatically. The output `.jar` will be in:

```
FabricTemplate/build/libs/
```

To build just the Fabric jar manually:

```bash
cd FabricTemplate
./gradlew build
```

---

## Installing the Mod

1. Build the project (`dotnet build`)
2. Copy the `.jar` from libs into your Minecraft `mods/` folder
3. Make sure Fabric Loader is installed
4. Launch Minecraft

---

## Example Mod

```csharp
using CSCraft;

public class MyMod : IMod
{
    public void OnInitialize()
    {
        // Give starter kit on join
        Events.PlayerJoin += (player) =>
        {
            player.SendMessage($"Welcome to the server, {player.Name}!");
            player.GiveItem("minecraft:bread", 16);
            player.GiveItem("minecraft:iron_sword", 1);
        };

        // Chat commands
        Events.ChatMessage += (player, message) =>
        {
            if (message == "!heal")
            {
                player.Heal(20);
                player.SendMessage("Healed!");
            }

            if (message == "!spawn")
            {
                player.Teleport(0, 64, 0);
                player.SendMessage("Teleported to spawn!");
            }
        };
    }
}
```

---

## Troubleshooting

**`mappings not found` or Gradle errors**
Make sure your build.gradle uses the buildscript approach and Gradle 8.8. See the included FabricTemplate for the correct configuration. Update versions in gradle.properties to the latest from https://fabricmc.net/develop.

**`Java not found`**
Either add Java 21 to your system PATH, or set `<CSCraftJavaHome>` in your `.csproj` to the full path of your Java 21 installation.

**`release version X not supported`**
You're using the wrong Java version. CSCraft requires Java 21. Check that `<CSCraftJavaHome>` points to Java 21, not a newer version.

**Client mixin compilation errors**
Ensure `splitEnvironmentSourceSets()` is commented out in build.gradle to allow the client source set to inherit the classpath. If issues persist, update method names in mixins to match the current Minecraft version's Yarn mappings.
