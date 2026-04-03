namespace CSCraft;

/// <summary>
/// Facade for a Minecraft item type (Item).
/// Use McRegistry.RegisterItem() to create and register custom items.
/// Transpiles to Java's Item.
/// </summary>
[JavaClass("net.minecraft.item.Item")]
public class McItem
{
    [JavaMethod("Registries.ITEM.getId({target}).toString()")]
    public string Id { get; } = null!;

    [JavaMethod("Registries.ITEM.getId({target}).getNamespace()")]
    public string Namespace { get; } = null!;

    [JavaMethod("Registries.ITEM.getId({target}).getPath()")]
    public string Path { get; } = null!;

    [JavaMethod("{target}.getMaxCount()")]
    public int MaxStackSize { get; }

    [JavaMethod("{target}.getMaxDamage()")]
    public int MaxDurability { get; }

    [JavaMethod("{target}.isDamageable()")]
    public bool IsDamageable { get; }

    [JavaMethod("{target}.isFood()")]
    public bool IsFood { get; }

    [JavaMethod("{target}.getDefaultStack()")]
    public McItemStack GetDefaultStack() => null!;
}

/// <summary>
/// Builder for item settings — used inside McRegistry.RegisterItem().
/// </summary>
[JavaClass("net.minecraft.item.Item.Settings")]
public sealed class McItemSettings
{
    [JavaMethod("new Item.Settings()")]
    public static McItemSettings Create() => null!;

    [JavaMethod("{target}.maxCount({0})")]
    public McItemSettings MaxCount(int count) => null!;

    [JavaMethod("{target}.maxDamage({0})")]
    public McItemSettings MaxDamage(int durability) => null!;

    [JavaMethod("{target}.food(new FoodComponent.Builder().nutrition({0}).saturationModifier({1}).build())")]
    public McItemSettings Food(int hunger, float saturation) => null!;

    [JavaMethod("{target}.rarity(Rarity.{0})")]
    public McItemSettings Rarity(string rarity) => null!;

    [JavaMethod("{target}.fireproof()")]
    public McItemSettings Fireproof() => null!;

    [JavaMethod("{target}.recipeRemainder({0}.getDefaultStack().getItem())")]
    public McItemSettings RecipeRemainder(McItem item) => null!;
}

/// <summary>
/// Represents a tool material for custom tools.
/// </summary>
[JavaClass("net.minecraft.item.ToolMaterial")]
public sealed class McToolMaterial
{
    /// <summary>Wood tier: durability 59, mining speed 2, attack damage 0, level 0</summary>
    [JavaMethod("ToolMaterials.WOOD")]
    public static readonly McToolMaterial Wood = null!;

    /// <summary>Stone tier: durability 131, mining speed 4, attack damage 1, level 1</summary>
    [JavaMethod("ToolMaterials.STONE")]
    public static readonly McToolMaterial Stone = null!;

    /// <summary>Iron tier: durability 250, mining speed 6, attack damage 2, level 2</summary>
    [JavaMethod("ToolMaterials.IRON")]
    public static readonly McToolMaterial Iron = null!;

    /// <summary>Gold tier: durability 32, mining speed 12, attack damage 0, level 0</summary>
    [JavaMethod("ToolMaterials.GOLD")]
    public static readonly McToolMaterial Gold = null!;

    /// <summary>Diamond tier: durability 1561, mining speed 8, attack damage 3, level 3</summary>
    [JavaMethod("ToolMaterials.DIAMOND")]
    public static readonly McToolMaterial Diamond = null!;

    /// <summary>Netherite tier: durability 2031, mining speed 9, attack damage 4, level 4</summary>
    [JavaMethod("ToolMaterials.NETHERITE")]
    public static readonly McToolMaterial Netherite = null!;
}

/// <summary>
/// Represents an armor material for custom armor.
/// </summary>
[JavaClass("net.minecraft.item.ArmorMaterial")]
public sealed class McArmorMaterial
{
    [JavaMethod("ArmorMaterials.LEATHER")]
    public static readonly McArmorMaterial Leather = null!;

    [JavaMethod("ArmorMaterials.CHAIN")]
    public static readonly McArmorMaterial Chain = null!;

    [JavaMethod("ArmorMaterials.IRON")]
    public static readonly McArmorMaterial Iron = null!;

    [JavaMethod("ArmorMaterials.GOLD")]
    public static readonly McArmorMaterial Gold = null!;

    [JavaMethod("ArmorMaterials.DIAMOND")]
    public static readonly McArmorMaterial Diamond = null!;

    [JavaMethod("ArmorMaterials.NETHERITE")]
    public static readonly McArmorMaterial Netherite = null!;

    [JavaMethod("ArmorMaterials.TURTLE")]
    public static readonly McArmorMaterial Turtle = null!;
}
