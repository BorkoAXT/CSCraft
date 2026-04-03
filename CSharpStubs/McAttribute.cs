namespace CSCraft;

/// <summary>
/// Facade for entity attributes (max health, attack damage, movement speed, etc.).
/// Use McRegistry.RegisterAttribute() for custom attributes.
/// Transpiles to Java's EntityAttribute / EntityAttributeModifier.
/// </summary>
[JavaClass("net.minecraft.entity.attribute.EntityAttribute")]
public class McAttribute
{
    [JavaMethod("Registries.ATTRIBUTE.getId({target}).toString()")]
    public string Id { get; } = null!;

    [JavaMethod("{target}.getDefaultValue()")]
    public double DefaultValue { get; }

    // ── Vanilla attribute helpers ─────────────────────────────────────────────

    /// <summary>Get the current value of an attribute on a living entity.</summary>
    [JavaMethod("{0}.getAttributeValue(EntityAttributes.{1})")]
    public static double GetValue(McEntity entity, string attributeName) => 0;

    /// <summary>Get the base (unmodified) value of an attribute on a living entity.</summary>
    [JavaMethod("{0}.getAttributeBaseValue(EntityAttributes.{1})")]
    public static double GetBaseValue(McEntity entity, string attributeName) => 0;

    /// <summary>
    /// Add a temporary attribute modifier to a living entity.
    /// operation: 0=add, 1=multiply_base, 2=multiply_total
    /// </summary>
    [JavaMethod("{0}.getAttributeInstance(EntityAttributes.{1}).addTemporaryModifier(new EntityAttributeModifier(java.util.UUID.randomUUID(), \"modifier\", {2}, EntityAttributeModifier.Operation.values()[{3}]))")]
    public static void AddModifier(McEntity entity, string attributeName, double value, int operation = 0) { }
}

/// <summary>
/// Common vanilla entity attribute names for use with McAttribute methods.
/// </summary>
public static class McAttributes
{
    public const string MaxHealth          = "GENERIC_MAX_HEALTH";
    public const string AttackDamage       = "GENERIC_ATTACK_DAMAGE";
    public const string AttackSpeed        = "GENERIC_ATTACK_SPEED";
    public const string MovementSpeed      = "GENERIC_MOVEMENT_SPEED";
    public const string FlyingSpeed        = "GENERIC_FLYING_SPEED";
    public const string Armor              = "GENERIC_ARMOR";
    public const string ArmorToughness     = "GENERIC_ARMOR_TOUGHNESS";
    public const string Knockback          = "GENERIC_ATTACK_KNOCKBACK";
    public const string KnockbackResist    = "GENERIC_KNOCKBACK_RESISTANCE";
    public const string FollowRange        = "GENERIC_FOLLOW_RANGE";
    public const string ReachDistance      = "PLAYER_ENTITY_INTERACTION_RANGE";
    public const string BlockInteractRange = "PLAYER_BLOCK_INTERACTION_RANGE";
    public const string Luck               = "GENERIC_LUCK";
}
