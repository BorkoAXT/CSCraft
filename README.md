# CSCraft

**v1.4.2** — CSCraft lets you write Minecraft Fabric mods in C# instead of Java. You write mod logic in C#, and CSCraft transpiles it into a working Fabric mod `.jar` — no Gradle, no Java knowledge, and no manual setup required.

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
McCommand.Register("heal", (src) => {
    McPlayer p = src.Player;
    if (p == null) return;
    p.Heal(p.MaxHealth);
    src.SendMessage("Healed!");
});

McCommand.RegisterWithInt("settime", "ticks", (src, ticks) => {
    McPlayer p = src.Player;
    if (p == null) return;
    McWorld w = p.World;
    w.SetTime(ticks);
});

McCommand.RegisterOpWithPlayer("heal", "target", (src, target) => {
    target.Heal(target.MaxHealth);
    src.SendMessage("Healed " + target.Name);
});
```

> **Transpiler rule:** Always extract `McPlayer p = src.Player;` before calling player methods. The transpiler does not resolve property chains automatically.

### Persistent Player Data
Store and retrieve per-player data that persists across sessions:
```csharp
PlayerData.Set(player, "visits", 1);
int visits = PlayerData.GetInt(player, "visits", 0);
bool hasHome = PlayerData.Has(player, "home_x");
```

### Scheduler
Run delayed tasks on the main server thread:
```csharp
McScheduler.RunLater(server, 200, (s) => {
    s.Broadcast("Server has been running for 10 seconds!");
});
```

> **Note:** `McScheduler.RunRepeating` is not yet supported. Use `Events.ServerTick` with a tick counter instead.

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
- **Build Error on Visual Studio 2022:** Use the CLI `dotnet build` instead of the built-in button.
- **Force Rebuild:** If changes aren't reflecting, run `dotnet build --no-incremental`.
- **Fabric JSON Error:** If you're getting an entrypoint error, make sure your `[ModInfo]` attribute is on the class that implements `IMod`.
- **CSCRAFT002 warnings:** These warn when a method or property chain couldn't be resolved. Always extract variables to a typed local before chaining — e.g. `McPlayer p = src.Player; p.Heal(...)` instead of `src.Player.Heal(...)`.
- **Other errors:** Remove the `obj/`, `bin/`, and `FabricTemplate/` folders, run `dotnet clean`, then rebuild. If the error persists, open an issue or contact BorkoAXT#5390 on Discord.

## ⚠️ Transpiler Rules

The C# → Java transpiler has a few requirements to produce valid Java:

1. **No property chains.** Always extract to a typed local variable:
   ```csharp
   // ✅ Correct
   McPlayer p = src.Player;
   p.Heal(10);

   // ❌ Wrong — transpiler can't resolve the chain
   src.Player.Heal(10);
   ```

2. **No nullable types on local variables.** Use `McPlayer p` not `McPlayer? p`.

3. **No null-conditional operators.** Use `if (p != null) p.Heal(...)` not `p?.Heal(...)`.

4. **Extract world variables when inside BlockBreak/PlayerAttack events** — those event lambdas already have a `world` parameter. Name your local variable differently (e.g. `bw`, `tw`).

5. **Recipes:** `McRecipe.RegisterShaped` keys must be single `char` literals (e.g. `'X'`), not strings.

---

## 📚 Documentation
For a full API reference, visit the CSCraft Documentation.
https://borkoaxt.github.io/CSCraft/#world-api

Built for the Minecraft .NET Community.
