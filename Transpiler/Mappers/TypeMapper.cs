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

        // Blocks
        ["McBlock"]             = "Block",
        ["McBlockState"]        = "BlockState",
        ["McBlockSettings"]     = "AbstractBlock.Settings",
        ["BlockPos"]            = "BlockPos",
        ["BlockState"]          = "BlockState",
        ["Block"]               = "Block",

        // Items
        ["McItem"]              = "Item",
        ["McItemSettings"]      = "Item.Settings",
        ["McToolMaterial"]      = "ToolMaterial",
        ["McArmorMaterial"]     = "ArmorMaterial",
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
        ["McNbt"]               = "NbtCompound",
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
        ["McStatusEffect"]          = "StatusEffect",
        ["McStatusEffectInstance"]  = "StatusEffectInstance",
        ["StatusEffect"]            = "StatusEffect",
        ["StatusEffectInstance"]    = "StatusEffectInstance",
        ["McEnchantment"]           = "Enchantment",
        ["Enchantment"]             = "Enchantment",

        // Damage
        ["DamageSource"]        = "DamageSource",

        // Inventory
        ["McInventory"]         = "Inventory",
        ["Inventory"]           = "Inventory",
        ["PlayerInventory"]     = "PlayerInventory",

        // Dimensions
        ["Dimension"]           = "RegistryKey<World>",

        // Sounds
        ["McSoundEvent"]        = "SoundEvent",
        ["SoundEvent"]          = "SoundEvent",

        // Attributes
        ["McAttribute"]         = "EntityAttribute",
        ["EntityAttribute"]     = "EntityAttribute",

        // Commands
        ["McCommandSource"]     = "ServerCommandSource",
        ["ServerCommandSource"] = "ServerCommandSource",

        // Item stacks / block positions (aliases)
        ["McItemStack"]         = "ItemStack",
        ["McBlockPos"]          = "BlockPos",

        // Villager
        ["McVillager"]          = "VillagerEntity",
        ["VillagerEntity"]      = "VillagerEntity",

        // Boss bar
        ["McBossBar"]           = "ServerBossBar",

        // Block entity
        ["McBlockEntity"]       = "BlockEntity",

        // Damage
        ["McDamageSource"]      = "DamageSource",
        ["McDamage"]            = "DamageSource",
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
        // Fabric
        ["ModInitializer"]          = "net.fabricmc.api.ModInitializer",
        // Players
        ["ServerPlayerEntity"]      = "net.minecraft.server.network.ServerPlayerEntity",
        ["PlayerEntity"]            = "net.minecraft.entity.player.PlayerEntity",
        ["PlayerInventory"]         = "net.minecraft.entity.player.PlayerInventory",
        // World & server
        ["ServerWorld"]             = "net.minecraft.server.world.ServerWorld",
        ["World"]                   = "net.minecraft.world.World",
        ["MinecraftServer"]         = "net.minecraft.server.MinecraftServer",
        ["ServerCommandSource"]     = "net.minecraft.server.command.ServerCommandSource",
        // Blocks
        ["BlockPos"]                = "net.minecraft.util.math.BlockPos",
        ["BlockState"]              = "net.minecraft.block.BlockState",
        ["Block"]                   = "net.minecraft.block.Block",
        ["AbstractBlock"]           = "net.minecraft.block.AbstractBlock",
        ["BlockItem"]               = "net.minecraft.item.BlockItem",
        ["BlockSoundGroup"]         = "net.minecraft.sound.BlockSoundGroup",
        ["Heightmap"]               = "net.minecraft.world.Heightmap",
        // Items
        ["ItemStack"]               = "net.minecraft.item.ItemStack",
        ["Item"]                    = "net.minecraft.item.Item",
        ["ToolMaterials"]           = "net.minecraft.item.ToolMaterials",
        ["ArmorMaterials"]          = "net.minecraft.item.ArmorMaterials",
        ["SwordItem"]               = "net.minecraft.item.SwordItem",
        ["PickaxeItem"]             = "net.minecraft.item.PickaxeItem",
        ["AxeItem"]                 = "net.minecraft.item.AxeItem",
        ["ShovelItem"]              = "net.minecraft.item.ShovelItem",
        ["HoeItem"]                 = "net.minecraft.item.HoeItem",
        ["ArmorItem"]               = "net.minecraft.item.ArmorItem",
        ["EquipmentSlot"]           = "net.minecraft.entity.EquipmentSlot",
        ["FoodComponent"]           = "net.minecraft.component.type.FoodComponent",
        ["Rarity"]                  = "net.minecraft.util.Rarity",
        // Text
        ["Text"]                    = "net.minecraft.text.Text",
        // Registry
        ["Identifier"]              = "net.minecraft.util.Identifier",
        ["Registries"]              = "net.minecraft.registry.Registries",
        ["Registry"]                = "net.minecraft.registry.Registry",
        ["RegistryKey"]             = "net.minecraft.registry.RegistryKey",
        // NBT
        ["NbtCompound"]             = "net.minecraft.nbt.NbtCompound",
        ["NbtElement"]              = "net.minecraft.nbt.NbtElement",
        // Math
        ["Vec3d"]                   = "net.minecraft.util.math.Vec3d",
        ["Vec3i"]                   = "net.minecraft.util.math.Vec3i",
        ["Direction"]               = "net.minecraft.util.math.Direction",
        ["Box"]                     = "net.minecraft.util.math.Box",
        // Entities
        ["Entity"]                  = "net.minecraft.entity.Entity",
        ["LivingEntity"]            = "net.minecraft.entity.LivingEntity",
        ["MobEntity"]               = "net.minecraft.entity.mob.MobEntity",
        ["EntityType"]              = "net.minecraft.entity.EntityType",
        ["EntityAttribute"]         = "net.minecraft.entity.attribute.EntityAttribute",
        ["EntityAttributes"]        = "net.minecraft.entity.attribute.EntityAttributes",
        ["EntityAttributeModifier"] = "net.minecraft.entity.attribute.EntityAttributeModifier",
        // Effects & enchantments
        ["StatusEffect"]            = "net.minecraft.entity.effect.StatusEffect",
        ["StatusEffectInstance"]    = "net.minecraft.entity.effect.StatusEffectInstance",
        ["Enchantment"]             = "net.minecraft.enchantment.Enchantment",
        ["EnchantmentHelper"]       = "net.minecraft.enchantment.EnchantmentHelper",
        // Damage
        ["DamageSource"]            = "net.minecraft.entity.damage.DamageSource",
        // Inventory
        ["Inventory"]               = "net.minecraft.inventory.Inventory",
        // Sound
        ["SoundEvent"]              = "net.minecraft.sound.SoundEvent",
        ["SoundCategory"]           = "net.minecraft.sound.SoundCategory",
        ["SoundEvents"]             = "net.minecraft.sound.SoundEvents",
        // Game rules
        ["GameRules"]               = "net.minecraft.world.GameRules",
        // Scoreboard
        ["ScoreboardCriterion"]     = "net.minecraft.scoreboard.ScoreboardCriterion",
        // Map
        ["MapColor"]                = "net.minecraft.block.MapColor",
        // Misc
        ["TypeFilter"]              = "net.minecraft.util.TypeFilter",
        ["GameMode"]                = "net.minecraft.world.GameMode",
        // Attributes
        ["ClampedEntityAttribute"]  = "net.minecraft.entity.attribute.ClampedEntityAttribute",
        // Registry helpers
        ["RegistryKeys"]            = "net.minecraft.registry.RegistryKeys",
        // Creative tabs
        ["ItemGroups"]              = "net.minecraft.item.ItemGroups",
        // Villager
        ["VillagerEntity"]          = "net.minecraft.entity.passive.VillagerEntity",
        // Projectiles
        ["SnowballEntity"]          = "net.minecraft.entity.projectile.thrown.SnowballEntity",
        ["ArrowEntity"]             = "net.minecraft.entity.projectile.ArrowEntity",
        ["SmallFireballEntity"]     = "net.minecraft.entity.projectile.SmallFireballEntity",
        ["ThrownPotionEntity"]      = "net.minecraft.entity.projectile.thrown.ThrownPotionEntity",
        // Boss bar
        ["ServerBossBar"]           = "net.minecraft.entity.boss.ServerBossBar",
        ["BossBar"]                 = "net.minecraft.entity.boss.BossBar",
        // Block entity
        ["BlockEntity"]             = "net.minecraft.block.entity.BlockEntity",
        // Scoreboard
        ["ScoreboardDisplaySlot"]   = "net.minecraft.scoreboard.ScoreboardDisplaySlot",
        // Potions (legacy util still present in 1.21.x)
        ["PotionUtil"]              = "net.minecraft.potion.PotionUtil",
        ["BrewingRecipeRegistry"]   = "net.minecraft.recipe.BrewingRecipeRegistry",
        // Java stdlib
        ["UUID"]                    = "java.util.UUID",
        ["HashMap"]                 = "java.util.HashMap",
        ["HashSet"]                 = "java.util.HashSet",
        ["ArrayDeque"]              = "java.util.ArrayDeque",
        ["CompletableFuture"]       = "java.util.concurrent.CompletableFuture",
        ["List"]                    = "java.util.List",
        ["ArrayList"]               = "java.util.ArrayList",
    };
}