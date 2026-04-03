namespace CSCraft;

/// <summary>
/// Facade for Minecraft enchantments.
/// Use to query enchantment levels on items.
/// Transpiles to Java's Enchantment / EnchantmentHelper.
/// </summary>
[JavaClass("net.minecraft.enchantment.Enchantment")]
public class McEnchantment
{
    [JavaMethod("Registries.ENCHANTMENT.getId({target}).toString()")]
    public string Id { get; } = null!;

    [JavaMethod("{target}.getMaxLevel()")]
    public int MaxLevel { get; }

    [JavaMethod("{target}.getMinLevel()")]
    public int MinLevel { get; }

    [JavaMethod("{target}.isCursed()")]
    public bool IsCursed { get; }

    [JavaMethod("{target}.isTreasure()")]
    public bool IsTreasure { get; }

    // ── Static helpers (via EnchantmentHelper) ────────────────────────────────

    /// <summary>Get the level of a specific enchantment on an ItemStack (0 if not present).</summary>
    [JavaMethod("EnchantmentHelper.getLevel(Registries.ENCHANTMENT.get(new Identifier({1})), {0})")]
    public static int GetLevel(McItemStack stack, string enchantmentId) => 0;

    /// <summary>Check whether an ItemStack has a specific enchantment.</summary>
    [JavaMethod("EnchantmentHelper.getLevel(Registries.ENCHANTMENT.get(new Identifier({1})), {0}) > 0")]
    public static bool HasEnchantment(McItemStack stack, string enchantmentId) => false;
}
