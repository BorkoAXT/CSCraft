namespace Transpiler;

/// <summary>
/// Maps C# type names to their Java equivalents.
/// Used by the emitter when translating variable declarations,
/// method return types, and parameter types.
/// </summary>
public static class TypeMapper
{
    // ── Primitives ────────────────────────────────────────────────────────────

    private static readonly Dictionary<string, string> Primitives = new()
    {
        ["void"]    = "void",
        ["bool"]    = "boolean",
        ["int"]     = "int",
        ["long"]    = "long",
        ["float"]   = "float",
        ["double"]  = "double",
        ["char"]    = "char",
        ["byte"]    = "byte",
        ["short"]   = "short",
        ["string"]  = "String",
        ["String"]  = "String",
        ["object"]  = "Object",
        ["Object"]  = "Object",
    };

    // ── Collections ───────────────────────────────────────────────────────────

    private static readonly Dictionary<string, string> Collections = new()
    {
        ["List"]            = "List",
        ["IList"]           = "List",
        ["IReadOnlyList"]   = "List",
        ["Dictionary"]      = "HashMap",
        ["IDictionary"]     = "HashMap",
        ["HashSet"]         = "HashSet",
        ["ISet"]            = "HashSet",
        ["IEnumerable"]     = "Iterable",
        ["Queue"]           = "ArrayDeque",
        ["Stack"]           = "ArrayDeque",
        ["Array"]           = "Object[]",
    };

    // ── Minecraft / Fabric API types ──────────────────────────────────────────

    private static readonly Dictionary<string, string> MinecraftTypes = new()
    {
        // Players
        ["McPlayer"]            = "ServerPlayerEntity",
        ["McPlayerEntity"]      = "ServerPlayerEntity",
        ["PlayerEntity"]        = "PlayerEntity",

        // World
        ["McWorld"]             = "ServerWorld",
        ["ServerWorld"]         = "ServerWorld",
        ["World"]               = "World",

        // Blocks & items
        ["BlockPos"]            = "BlockPos",
        ["BlockState"]          = "BlockState",
        ["Block"]               = "Block",
        ["ItemStack"]           = "ItemStack",
        ["Item"]                = "Item",

        // Text / chat
        ["McText"]              = "Text",
        ["ChatMessage"]         = "Text",

        // Mod entrypoint
        ["IMod"]                = "ModInitializer",
        ["ModInitializer"]      = "ModInitializer",

        // Server
        ["McServer"]            = "MinecraftServer",
        ["MinecraftServer"]     = "MinecraftServer",

        // Identifiers
        ["ResourceLocation"]    = "Identifier",
        ["McIdentifier"]        = "Identifier",

        // NBT
        ["NbtCompound"]         = "NbtCompound",
        ["NbtElement"]          = "NbtElement",

        // Math
        ["Vec3"]                = "Vec3d",
        ["Vec3i"]               = "Vec3i",
        ["Direction"]           = "Direction",
        ["Box"]                 = "Box",

        // Entities
        ["McEntity"]            = "Entity",
        ["Entity"]              = "Entity",
        ["LivingEntity"]        = "LivingEntity",
        ["MobEntity"]           = "MobEntity",

        // Effects & enchantments
        ["StatusEffect"]        = "StatusEffect",
        ["StatusEffectInstance" ]= "StatusEffectInstance",
        ["Enchantment"]         = "Enchantment",

        // Damage
        ["DamageSource"]        = "DamageSource",

        // Inventory
        ["Inventory"]           = "Inventory",
        ["PlayerInventory"]     = "PlayerInventory",

        // Dimensions
        ["Dimension"]           = "RegistryKey<World>",
    };

    // ── Java standard library extras ─────────────────────────────────────────

    private static readonly Dictionary<string, string> JavaStd = new()
    {
        ["Task"]            = "CompletableFuture",
        ["CancellationToken"] = "void",        // no direct equivalent — dropped
        ["TimeSpan"]        = "long",           // represented as ticks or ms
        ["DateTime"]        = "long",           // epoch ms
        ["Guid"]            = "UUID",
        ["Action"]          = "Runnable",
        ["Func"]            = "Supplier",       // simplified — generics vary
        ["Exception"]       = "Exception",
        ["Random"]          = "Random",
        ["Math"]            = "Math",
        ["Console"]         = "LOGGER",         // Console.WriteLine → LOGGER.info
    };

    // ── Nullable suffix handling ──────────────────────────────────────────────
    // C# nullable types like "string?" → "String" (Java uses @Nullable annotations
    // but we just strip the ? for now)

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Maps a C# type string to its Java equivalent.
    /// Handles generics like List&lt;McPlayer&gt; → List&lt;ServerPlayerEntity&gt;.
    /// Returns the original type unchanged if no mapping is found.
    /// </summary>
    public static string Map(string csType)
    {
        if (string.IsNullOrWhiteSpace(csType)) return csType;

        // Strip nullable suffix
        string type = csType.TrimEnd('?');

        // Handle arrays: int[] → int[]
        if (type.EndsWith("[]"))
        {
            string inner = Map(type[..^2]);
            return inner + "[]";
        }

        // Handle generics: List<McPlayer> → List<ServerPlayerEntity>
        int genericStart = type.IndexOf('<');
        if (genericStart >= 0)
        {
            string outer = type[..genericStart];
            string inner = type[(genericStart + 1)..^1];
            string mappedOuter = MapSimple(outer);
            string mappedInner = MapGenericArgs(inner);
            return $"{mappedOuter}<{mappedInner}>";
        }

        return MapSimple(type);
    }

    /// <summary>
    /// Maps the Java import path for a given C# type, if needed.
    /// Returns null if no import is required (primitive, already in scope).
    /// </summary>
    public static string? GetImport(string csType)
    {
        string javaType = Map(csType.TrimEnd('?'));
        return ImportMap.TryGetValue(javaType, out var import) ? import : null;
    }

    // ── Internals ─────────────────────────────────────────────────────────────

    private static string MapSimple(string type)
    {
        if (Primitives.TryGetValue(type, out var p)) return p;
        if (Collections.TryGetValue(type, out var c)) return c;
        if (MinecraftTypes.TryGetValue(type, out var m)) return m;
        if (JavaStd.TryGetValue(type, out var j)) return j;
        return type; // unknown — pass through unchanged
    }

    private static string MapGenericArgs(string args)
    {
        // Simple split on top-level comma (doesn't handle nested generics > 1 deep)
        var parts = SplitTopLevel(args);
        return string.Join(", ", parts.Select(Map));
    }

    private static List<string> SplitTopLevel(string s)
    {
        var result = new List<string>();
        int depth = 0, start = 0;
        for (int i = 0; i < s.Length; i++)
        {
            if (s[i] == '<') depth++;
            else if (s[i] == '>') depth--;
            else if (s[i] == ',' && depth == 0)
            {
                result.Add(s[start..i].Trim());
                start = i + 1;
            }
        }
        result.Add(s[start..].Trim());
        return result;
    }

    // ── Import map ────────────────────────────────────────────────────────────
    // Java types that need explicit import statements

    public static readonly Dictionary<string, string> ImportMap = new()
    {
        ["ModInitializer"]       = "net.fabricmc.api.ModInitializer",
        ["ServerPlayerEntity"]   = "net.minecraft.server.network.ServerPlayerEntity",
        ["PlayerEntity"]         = "net.minecraft.entity.player.PlayerEntity",
        ["ServerWorld"]          = "net.minecraft.server.world.ServerWorld",
        ["World"]                = "net.minecraft.world.World",
        ["BlockPos"]             = "net.minecraft.util.math.BlockPos",
        ["BlockState"]           = "net.minecraft.block.BlockState",
        ["Block"]                = "net.minecraft.block.Block",
        ["ItemStack"]            = "net.minecraft.item.ItemStack",
        ["Item"]                 = "net.minecraft.item.Item",
        ["Text"]                 = "net.minecraft.text.Text",
        ["MinecraftServer"]      = "net.minecraft.server.MinecraftServer",
        ["Identifier"]           = "net.minecraft.util.Identifier",
        ["NbtCompound"]          = "net.minecraft.nbt.NbtCompound",
        ["NbtElement"]           = "net.minecraft.nbt.NbtElement",
        ["Vec3d"]                = "net.minecraft.util.math.Vec3d",
        ["Vec3i"]                = "net.minecraft.util.math.Vec3i",
        ["Direction"]            = "net.minecraft.util.math.Direction",
        ["Box"]                  = "net.minecraft.util.math.Box",
        ["Entity"]               = "net.minecraft.entity.Entity",
        ["LivingEntity"]         = "net.minecraft.entity.LivingEntity",
        ["MobEntity"]            = "net.minecraft.entity.mob.MobEntity",
        ["StatusEffect"]         = "net.minecraft.entity.effect.StatusEffect",
        ["StatusEffectInstance"] = "net.minecraft.entity.effect.StatusEffectInstance",
        ["Enchantment"]          = "net.minecraft.enchantment.Enchantment",
        ["DamageSource"]         = "net.minecraft.entity.damage.DamageSource",
        ["Inventory"]            = "net.minecraft.inventory.Inventory",
        ["PlayerInventory"]      = "net.minecraft.entity.player.PlayerInventory",
        ["UUID"]                 = "java.util.UUID",
        ["HashMap"]              = "java.util.HashMap",
        ["HashSet"]              = "java.util.HashSet",
        ["ArrayDeque"]           = "java.util.ArrayDeque",
        ["CompletableFuture"]    = "java.util.concurrent.CompletableFuture",
        ["List"]                 = "java.util.List",
        ["ArrayList"]            = "java.util.ArrayList",
        ["Registries"]           = "net.minecraft.registry.Registries",
    };
}