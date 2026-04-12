# CSCraft

CSCraft lets you write Minecraft Fabric mods in C# instead of Java. You write mod logic in C#, and CSCraft transpiles it into a working Fabric mod `.jar` — no Gradle, no Java knowledge, and no manual setup required.

One command: `dotnet build` does everything.

---

## 🚀 Quick Start

### 1. Create a new project
```bash
dotnet new classlib -n MyMod
cd MyMod
dotnet add package CSCraft.Sdk
```

### 2. Write your mod
Replace `Class1.cs` with the following:

```csharp
using CSCraft;

[ModInfo(
    Id          = "mymod",
    Name        = "My Mod",
    Version     = "1.0.0",
    Author      = "YourName",
    Description = "My first C# Minecraft mod",
    MinecraftVersion = "1.21.1"
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

A single `dotnet build` performs the following steps in one pass:
1.  **Setup:** Reads `[ModInfo]` and generates the Fabric template.
2.  **Java Discovery:** Auto-discovers your JDK (no `JAVA_HOME` setup needed).
3.  **Transpile:** Converts C# logic to Java.
4.  **Resource Generation:** Generates recipe, model, blockstate, and lang JSONs.
5.  **Gradle:** Runs Gradle to produce the final mod `.jar`.

The output `.jar` is automatically copied to your project folder. Simply move it to your Minecraft `mods/` folder.

---

## 📋 Prerequisites

- .NET 9 SDK
- Java 21 (Automatically discovered by CSCraft)
- Fabric Loader installed in your Minecraft instance

### Automatic Java Discovery
CSCraft automatically finds a suitable JDK on your system, checking:
1. `<CSCraftJavaHome>` in your `.csproj` (Manual override).
2. `JAVA_HOME` / `JDK_HOME` environment variables.
3. Windows Registry.
4. Common installation folders.

---

## 🛠️ Key Features & APIs

### The `[ModInfo]` Attribute
Configure your mod metadata directly in C#. Supported versions include 1.20.x through 1.21.1.

### Events API
Subscribe to server-side events like `PlayerJoin`, `BlockBreak`, `ChatMessage`, and `ServerTick` easily.

### Player & Entity API
Comprehensive methods for handling player identity, health, movement, inventory, effects, and NBT data.

### World & Server API
Manipulate blocks, spawn entities, manage weather/time, and broadcast server-wide messages.

### Registration & Auto-Resources
Register Blocks, Items, Tools, and Armor with a single line of code. CSCraft automatically generates:
- Blockstates and Models
- Handheld Item Models
- Language entries (`en_us.json`)
- Mining tags (tool type + mining level)

### Block Categories (Mining Levels)
Register blocks with mining requirements — the correct Fabric block tags are auto-generated:
```csharp
// Ore block: requires a pickaxe, iron tier minimum
var rubyOre = McRegistry.RegisterBlock("mymod:ruby_ore", 3.0f, McMineTool.Pickaxe, McMineLevel.Iron);
```
This generates `mineable/pickaxe.json` and `needs_iron_tool.json` tags automatically.

### Custom Textures & Assets
Place your texture PNGs in an `Assets/` folder in your project:
```
MyMod/
├── Assets/
│   ├── textures/
│   │   ├── block/
│   │   │   └── ruby_ore.png
���   │   └── item/
│   │       ���── ruby.png
│   └── sounds/
│       └── my_sound.ogg
├── MyMod.cs
└── MyMod.csproj
```
They are automatically copied to the Fabric resource pack on build.

### Commands
Register custom commands with arguments, selectors, and permissions:
```csharp
McCommand.RegisterWithPlayer("heal", "target", (src, target) => {
    target.Heal(target.MaxHealth);
    src.SendMessage($"Healed {target.Name}");
});
```

### Scheduler
Run delayed or repeating tasks on the main server thread or asynchronously.

---

## 🏗️ Build Pipeline

When you run `dotnet build`, the following unified target executes:

```text
dotnet build
  |
  |-- 1. Setup:      Reads [ModInfo], generates FabricTemplate/
  |-- 2. Assets:     Copies textures/sounds from Assets/ folder
  |-- 3. Transpile:  Converts .cs files to .java, generates JSON resources + block tags
  |-- 4. Gradle:     Runs gradlew build to compile and JAR
  |-- 5. Copy JAR:   Copies the .jar to your project folder
  |
  v
MyMod/mymod-1.0.0.jar
```

---

## 🔧 Advanced Configuration

You can manually override settings in your `.csproj`:

```xml
<PropertyGroup>
  <CSCraftPackageName>com.yourname.mymod</CSCraftPackageName>
  <CSCraftJavaHome>C:\Program Files\Java\jdk-21</CSCraftJavaHome>
  <CSCraftRunGradle>true</CSCraftRunGradle>
</PropertyGroup>
```

---

## ❓ Troubleshooting

- **JDK Not Found:** Install JDK 21. If it's still not found, set `<CSCraftJavaHome>` in your `.csproj`.
- **Wrong Java Version:** Ensure you are using JDK 21 (required for Minecraft 1.20.6+).
- **Resources Not Generated:** Ensure `McRegistry` and `McRecipe` calls use compile-time string literals.
- **Build Error:** If your on Visual Studio 2022, make sure you are using the CLI dotnet build instead of the built-in button as the button doesn't work for now.
- **Force Rebuild:** If changes aren't reflecting, run `dotnet build --no-incremental`.
- **Fabric Json Error:** If your getting an entrypoint error, make sure your `[ModInfo]` attribute is on the class that implements `IMod`.
- **Other errors:" If you have any build errors or you don't see the jar file, make sure to remove the obj, bin and FabricTemplate folders and do a dotnet clean. If the error isn't here, send a message via email or discord at BorkoAXT#5390

---

## 📚 Documentation
For a full API reference, visit the CSCraft Documentation.
https://borkoaxt.github.io/CSCraft/#world-api

Built for the Minecraft .NET Community.

SSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSS