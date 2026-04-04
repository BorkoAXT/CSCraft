namespace Transpiler;

/// <summary>
/// Tracks which Java imports are needed during transpilation
/// and emits the final import block at the top of the Java file.
///
/// The emitter calls Add() as it encounters types/methods that need imports.
/// At the end, GetImportBlock() returns the sorted, deduplicated import statements.
/// </summary>
public class ImportTracker
{
    private readonly HashSet<string> _imports = new();

    // ── Always-present imports for any Fabric mod ─────────────────────────────

    private static readonly string[] FabricBaseImports =
    [
        "net.fabricmc.api.ModInitializer",
        "org.slf4j.Logger",
        "org.slf4j.LoggerFactory",
    ];

    // ── Common Minecraft imports that are almost always needed ────────────────

    private static readonly string[] MinecraftCommonImports =
    [
        "net.minecraft.server.network.ServerPlayerEntity",
        "net.minecraft.text.Text",
    ];

    public ImportTracker(bool includeFabricBase = true)
    {
        if (includeFabricBase)
            foreach (var i in FabricBaseImports)
                _imports.Add(i);
    }

    // ── Add imports ───────────────────────────────────────────────────────────

    /// <summary>Add a single fully-qualified import.</summary>
    public void Add(string fullyQualified) => _imports.Add(fullyQualified);

    /// <summary>Add multiple imports at once.</summary>
    public void AddRange(IEnumerable<string> imports)
    {
        foreach (var i in imports) _imports.Add(i);
    }

    /// <summary>
    /// Add an import by looking up a C# type name in TypeMapper's import map.
    /// Does nothing if no mapping is found.
    /// </summary>
    public void AddForCsType(string csTypeName)
    {
        string javaType = TypeMapper.Map(csTypeName);
        if (TypeMapper.ImportMap.TryGetValue(javaType, out var import))
            _imports.Add(import);
    }

    /// <summary>
    /// Add imports declared on a MethodMapping.
    /// </summary>
    public void AddFromMethod(MethodMapping? mapping)
    {
        if (mapping?.Imports == null) return;
        foreach (var i in mapping.Imports) _imports.Add(i);
    }

    /// <summary>
    /// Add the import for a Fabric event.
    /// </summary>
    public void AddForEvent(EventMapping mapping)
        => _imports.Add(mapping.FabricImport);

    // ── Expose raw set ────────────────────────────────────────────────────────

    public HashSet<string> GetImports() => new(_imports);

    // ── Emit ──────────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns the sorted, grouped import block as a Java source string.
    /// Groups: java.*, javax.*, net.minecraft.*, net.fabricmc.*, org.*
    /// </summary>
    public string GetImportBlock()
    {
        var groups = new[]
        {
            _imports.Where(i => i.StartsWith("java.")).OrderBy(x => x),
            _imports.Where(i => i.StartsWith("javax.")).OrderBy(x => x),
            _imports.Where(i => i.StartsWith("net.minecraft.")).OrderBy(x => x),
            _imports.Where(i => i.StartsWith("net.fabricmc.")).OrderBy(x => x),
            _imports.Where(i => i.StartsWith("org.")).OrderBy(x => x),
            _imports.Where(i => !i.StartsWith("java") &&
                                !i.StartsWith("javax") &&
                                !i.StartsWith("net.") &&
                                !i.StartsWith("org.")).OrderBy(x => x),
        };

        var lines = new List<string>();
        foreach (var group in groups)
        {
            var groupList = group.ToList();
            if (groupList.Count == 0) continue;
            foreach (var import in groupList)
                lines.Add($"import {import};");
            lines.Add(""); // blank line between groups
        }

        // Remove trailing blank line
        while (lines.Count > 0 && lines[^1] == "")
            lines.RemoveAt(lines.Count - 1);

        return string.Join("\n", lines);
    }
}

/// <summary>
/// Static lookup for well-known import paths that the emitter might need
/// without going through TypeMapper (e.g. for Fabric event classes).
/// </summary>
public static class ImportMapper
{
    public static readonly Dictionary<string, string> WellKnown = new()
    {
        // Fabric events
        ["ServerPlayConnectionEvents"]  = "net.fabricmc.fabric.api.networking.v1.ServerPlayConnectionEvents",
        ["PlayerBlockBreakEvents"]      = "net.fabricmc.fabric.api.event.player.PlayerBlockBreakEvents",
        ["UseBlockCallback"]            = "net.fabricmc.fabric.api.event.player.UseBlockCallback",
        ["UseItemCallback"]             = "net.fabricmc.fabric.api.event.player.UseItemCallback",
        ["AttackEntityCallback"]        = "net.fabricmc.fabric.api.event.player.AttackEntityCallback",
        ["AttackBlockCallback"]         = "net.fabricmc.fabric.api.event.player.AttackBlockCallback",
        ["UseEntityCallback"]           = "net.fabricmc.fabric.api.event.player.UseEntityCallback",
        ["ServerMessageEvents"]         = "net.fabricmc.fabric.api.message.v1.ServerMessageEvents",
        ["ServerLifecycleEvents"]       = "net.fabricmc.fabric.api.event.lifecycle.v1.ServerLifecycleEvents",
        ["ServerTickEvents"]            = "net.fabricmc.fabric.api.event.lifecycle.v1.ServerTickEvents",
        ["ServerEntityEvents"]          = "net.fabricmc.fabric.api.event.lifecycle.v1.ServerEntityEvents",
        ["ServerLivingEntityEvents"]    = "net.fabricmc.fabric.api.entity.event.v1.ServerLivingEntityEvents",
        ["ServerChunkEvents"]           = "net.fabricmc.fabric.api.event.lifecycle.v1.ServerChunkEvents",
        ["EntityPickupItemEvents"]      = "net.fabricmc.fabric.api.entity.event.v1.EntityPickupItemEvents",
        ["ServerPlayerEvents"]          = "net.fabricmc.fabric.api.entity.event.v1.ServerPlayerEvents",
        ["CommandRegistrationCallback"] = "net.fabricmc.fabric.api.command.v2.CommandRegistrationCallback",
        ["CommandManager"]              = "net.fabricmc.fabric.api.command.v2.CommandManager",
        ["ItemGroupEvents"]             = "net.fabricmc.fabric.api.itemgroup.v1.ItemGroupEvents",
        ["EntitySleepEvents"]           = "net.fabricmc.fabric.api.entity.event.v1.EntitySleepEvents",
        ["ServerWorldEvents"]           = "net.fabricmc.fabric.api.event.lifecycle.v1.ServerWorldEvents",
        ["ServerBlockEntityEvents"]     = "net.fabricmc.fabric.api.event.lifecycle.v1.ServerBlockEntityEvents",

        // Minecraft core
        ["Text"]                    = "net.minecraft.text.Text",
        ["Identifier"]              = "net.minecraft.util.Identifier",
        ["Registries"]              = "net.minecraft.registry.Registries",
        ["Registry"]                = "net.minecraft.registry.Registry",
        ["RegistryKey"]             = "net.minecraft.registry.RegistryKey",
        ["World"]                   = "net.minecraft.world.World",
        ["GameMode"]                = "net.minecraft.world.GameMode",
        ["GameRules"]               = "net.minecraft.world.GameRules",
        ["SoundCategory"]           = "net.minecraft.sound.SoundCategory",
        ["SoundEvent"]              = "net.minecraft.sound.SoundEvent",
        ["BlockPos"]                = "net.minecraft.util.math.BlockPos",
        ["BlockState"]              = "net.minecraft.block.BlockState",
        ["Block"]                   = "net.minecraft.block.Block",
        ["AbstractBlock"]           = "net.minecraft.block.AbstractBlock",
        ["BlockItem"]               = "net.minecraft.item.BlockItem",
        ["Heightmap"]               = "net.minecraft.world.Heightmap",
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
        ["FoodComponent"]           = "net.minecraft.item.FoodComponent",
        ["Rarity"]                  = "net.minecraft.util.Rarity",
        ["Entity"]                  = "net.minecraft.entity.Entity",
        ["LivingEntity"]            = "net.minecraft.entity.LivingEntity",
        ["EntityType"]              = "net.minecraft.entity.EntityType",
        ["EntityAttribute"]         = "net.minecraft.entity.attribute.EntityAttribute",
        ["EntityAttributes"]        = "net.minecraft.entity.attribute.EntityAttributes",
        ["EntityAttributeModifier"] = "net.minecraft.entity.attribute.EntityAttributeModifier",
        ["ServerPlayerEntity"]      = "net.minecraft.server.network.ServerPlayerEntity",
        ["ServerWorld"]             = "net.minecraft.server.world.ServerWorld",
        ["MinecraftServer"]         = "net.minecraft.server.MinecraftServer",
        ["ServerCommandSource"]     = "net.minecraft.server.command.ServerCommandSource",
        ["StatusEffect"]            = "net.minecraft.entity.effect.StatusEffect",
        ["StatusEffectInstance"]    = "net.minecraft.entity.effect.StatusEffectInstance",
        ["Enchantment"]             = "net.minecraft.enchantment.Enchantment",
        ["EnchantmentHelper"]       = "net.minecraft.enchantment.EnchantmentHelper",
        ["DamageSource"]            = "net.minecraft.entity.damage.DamageSource",
        ["NbtCompound"]             = "net.minecraft.nbt.NbtCompound",
        ["TypeFilter"]              = "net.minecraft.util.TypeFilter",
        ["MapColor"]                = "net.minecraft.block.MapColor",
        ["BlockSoundGroup"]         = "net.minecraft.sound.BlockSoundGroup",
        ["ScoreboardCriterion"]     = "net.minecraft.scoreboard.ScoreboardCriterion",

        // Attributes (extended)
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
        // Potions
        ["PotionUtil"]              = "net.minecraft.potion.PotionUtil",
        ["BrewingRecipeRegistry"]   = "net.minecraft.recipe.BrewingRecipeRegistry",
        // Java stdlib
        ["UUID"]                = "java.util.UUID",
        ["List"]                = "java.util.List",
        ["ArrayList"]           = "java.util.ArrayList",
        ["HashMap"]             = "java.util.HashMap",
        ["HashSet"]             = "java.util.HashSet",
        ["Optional"]            = "java.util.Optional",
        ["CompletableFuture"]   = "java.util.concurrent.CompletableFuture",
    };

    public static string? Get(string javaSimpleName)
        => WellKnown.TryGetValue(javaSimpleName, out var v) ? v : null;
}