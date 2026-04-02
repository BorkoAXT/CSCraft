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

Copy the `FabricTemplate` folder (included with CSCraft) into your project directory so it sits next to your `.csproj` file:

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
2. Copy the `.jar` from `FabricTemplate/build/libs/` into your Minecraft `mods/` folder
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
Make sure your `FabricTemplate/build.gradle` uses the buildscript approach and Gradle 8.8. See the included `FabricTemplate` for the correct configuration.

**`Java not found`**
Either add Java 21 to your system PATH, or set `<CSCraftJavaHome>` in your `.csproj` to the full path of your Java 21 installation.

**`release version X not supported`**
You're using the wrong Java version. CSCraft requires Java 21. Check that `<CSCraftJavaHome>` points to Java 21, not a newer version.
