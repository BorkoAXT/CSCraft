namespace CSCraft;

/// <summary>
/// Static helpers for registering mod content (blocks, items, effects, etc.)
/// All methods transpile to Registry.register(...) calls in Java.
/// </summary>
public static class McRegistry
{
    // ── Blocks ────────────────────────────────────────────────────────────────

    /// <summary>Register a basic block. id example: "mymod:my_block"</summary>
    public static McBlock RegisterBlock(string id, McBlockSettings settings) => null!;

    /// <summary>Register a block using a hardness value (resistance = hardness * 3).</summary>
    public static McBlock RegisterBlock(string id, float hardness = 1.0f) => null!;

    // ── Items ─────────────────────────────────────────────────────────────────

    /// <summary>Register a plain item.</summary>
    public static McItem RegisterItem(string id, McItemSettings settings) => null!;

    /// <summary>Register a plain item with default settings.</summary>
    public static McItem RegisterItem(string id) => null!;

    /// <summary>Register a BlockItem (item that places a block).</summary>
    public static McItem RegisterBlockItem(string id, McBlock block) => null!;

    /// <summary>Register a sword item.</summary>
    public static McItem RegisterSword(string id, McToolMaterial material, int bonusDamage = 3, float attackSpeed = -2.4f) => null!;

    /// <summary>Register a pickaxe item.</summary>
    public static McItem RegisterPickaxe(string id, McToolMaterial material, int bonusDamage = 1, float attackSpeed = -2.8f) => null!;

    /// <summary>Register an axe item.</summary>
    public static McItem RegisterAxe(string id, McToolMaterial material, float bonusDamage = 6.0f, float attackSpeed = -3.1f) => null!;

    /// <summary>Register a shovel item.</summary>
    public static McItem RegisterShovel(string id, McToolMaterial material, float bonusDamage = 1.5f, float attackSpeed = -3.0f) => null!;

    /// <summary>Register a hoe item.</summary>
    public static McItem RegisterHoe(string id, McToolMaterial material, int bonusDamage = 0, float attackSpeed = -3.0f) => null!;

    /// <summary>Register a food item.</summary>
    public static McItem RegisterFood(string id, int hunger, float saturation, bool meat = false) => null!;

    /// <summary>Register a helmet (armor) item.</summary>
    public static McItem RegisterHelmet(string id, McArmorMaterial material) => null!;

    /// <summary>Register a chestplate (armor) item.</summary>
    public static McItem RegisterChestplate(string id, McArmorMaterial material) => null!;

    /// <summary>Register leggings (armor) item.</summary>
    public static McItem RegisterLeggings(string id, McArmorMaterial material) => null!;

    /// <summary>Register boots (armor) item.</summary>
    public static McItem RegisterBoots(string id, McArmorMaterial material) => null!;

    // ── Creative tabs ─────────────────────────────────────────────────────────

    /// <summary>Add items to an existing vanilla creative tab.</summary>
    public static void AddToCreativeTab(string tabId, params McItem[] items) { }

    // ── Effects ───────────────────────────────────────────────────────────────

    /// <summary>Register a custom status effect.</summary>
    public static McStatusEffect RegisterStatusEffect(string id, McStatusEffect.EffectType type, int color) => null!;

    // ── Sounds ────────────────────────────────────────────────────────────────

    /// <summary>Register a custom sound event (the .ogg file must be in assets/modid/sounds/).</summary>
    public static McSoundEvent RegisterSound(string id) => null!;

    // ── Attributes ────────────────────────────────────────────────────────────

    /// <summary>Register a custom entity attribute.</summary>
    public static McAttribute RegisterAttribute(string id, double defaultValue, double min = 0, double max = 1024) => null!;

    // ── Villager trades ───────────────────────────────────────────────────────

    /// <summary>Add a buy trade to a villager profession (player buys from villager).</summary>
    public static void AddVillagerBuyTrade(string professionId, McItem item, int price, int maxUses = 8) { }

    /// <summary>Add a sell trade to a villager profession (player sells to villager).</summary>
    public static void AddVillagerSellTrade(string professionId, McItem item, int emeraldPrice, int maxUses = 8) { }

    // ── Entity types ──────────────────────────────────────────────────────────

    /// <summary>
    /// Register a custom entity type backed by an existing Java entity class.
    /// spawnGroup: "creature", "monster", "ambient", "water_creature", "misc"
    /// </summary>
    public static McEntityType RegisterEntity(string id, string spawnGroup, float width, float height) => null!;

    // ── Block entities ────────────────────────────────────────────────────────

    /// <summary>
    /// Register a block entity type linked to one or more blocks.
    /// The Java BlockEntity class is emitted automatically by the transpiler
    /// when you define a class inheriting McBlockEntity.
    /// </summary>
    public static McBlockEntityType RegisterBlockEntity(string id, params McBlock[] blocks) => null!;

    // ── Game rules ────────────────────────────────────────────────────────────

    /// <summary>Register a custom boolean game rule (e.g. "mymod:my_rule").</summary>
    public static McGameRule<bool> RegisterBoolRule(string id, bool defaultValue, GameRuleCategory category = GameRuleCategory.Misc) => null!;

    /// <summary>Register a custom integer game rule.</summary>
    public static McGameRule<int> RegisterIntRule(string id, int defaultValue, GameRuleCategory category = GameRuleCategory.Misc) => null!;
}

/// <summary>Represents a registered custom entity type.</summary>
[JavaClass("net.minecraft.entity.EntityType")]
public class McEntityType
{
    [JavaMethod("EntityType.getId({target}).toString()")]
    public string Id { get; } = null!;
}

/// <summary>Represents a registered custom block entity type.</summary>
[JavaClass("net.minecraft.block.entity.BlockEntityType")]
public class McBlockEntityType
{
    [JavaMethod("net.minecraft.registry.Registries.BLOCK_ENTITY_TYPE.getId({target}).toString()")]
    public string Id { get; } = null!;
}

/// <summary>Represents a registered custom game rule.</summary>
[JavaClass("net.minecraft.world.GameRules.Key")]
public class McGameRule<T>
{
    /// <summary>Get the current value of this rule on the server.</summary>
    [JavaMethod("{0}.getGameRules().get({target})")]
    public T GetValue(McServer server) => default!;

    /// <summary>Set the value of this rule.</summary>
    [JavaMethod("{0}.getGameRules().get({target}).set({1}, {0})")]
    public void SetValue(McServer server, T value) { }
}

/// <summary>Categories for custom game rules.</summary>
public enum GameRuleCategory { Misc, Player, Mobs, Drops, Updates, Chat, Spawning }
