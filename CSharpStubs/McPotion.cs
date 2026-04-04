namespace CSCraft;

/// <summary>
/// Facade for Minecraft potions (PotionUtil / PotionItem).
/// Transpiles to Java's PotionUtil / Potions / BrewingRecipeRegistry.
/// </summary>
public static class McPotion
{
    // ── Potion item lookup ────────────────────────────────────────────────────

    /// <summary>Get the potion ID from a potion ItemStack.</summary>
    [JavaMethod("PotionUtil.getPotion({0}).getId(Registries.POTION)")]
    public static string GetPotionId(McItemStack stack) => null!;

    /// <summary>Check whether an ItemStack is a potion with the given effect.</summary>
    [JavaMethod("PotionUtil.getPotionEffects({0}).stream().anyMatch(e -> e.getEffectType() == Registries.STATUS_EFFECT.get(Identifier.of({1})))")]
    public static bool HasEffect(McItemStack stack, string effectId) => false;

    /// <summary>Get all status effects on a potion ItemStack.</summary>
    [JavaMethod("PotionUtil.getPotionEffects({0})")]
    public static List<McStatusEffectInstance> GetEffects(McItemStack stack) => null!;

    // ── Brewing recipe registration ───────────────────────────────────────────

    /// <summary>
    /// Register a custom brewing recipe.
    /// input: the potion to start with (e.g. "minecraft:awkward").
    /// ingredient: the item that triggers the brew.
    /// output: the resulting potion (e.g. "minecraft:strength").
    /// </summary>
    public static void RegisterBrewingRecipe(string inputPotionId, McItem ingredient, string outputPotionId) { }
}
