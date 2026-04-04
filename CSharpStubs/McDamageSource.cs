namespace CSCraft;

/// <summary>
/// Predefined damage source IDs and helpers for dealing typed damage to entities.
/// </summary>
public static class McDamage
{
    // ── Built-in damage type IDs ───────────────────────────────────────────────

    public const string Generic        = "minecraft:generic";
    public const string Fire           = "minecraft:in_fire";
    public const string Lava           = "minecraft:lava";
    public const string Drown          = "minecraft:drown";
    public const string Starve         = "minecraft:starve";
    public const string Cactus         = "minecraft:cactus";
    public const string Fall           = "minecraft:fall";
    public const string OutOfWorld     = "minecraft:out_of_world";
    public const string Magic          = "minecraft:magic";
    public const string Wither         = "minecraft:wither";
    public const string Lightning      = "minecraft:lightning_bolt";
    public const string Explosion      = "minecraft:explosion";
    public const string Arrow          = "minecraft:arrow";
    public const string Fireball       = "minecraft:fireball";
    public const string Thorns         = "minecraft:thorns";
    public const string FallingSand    = "minecraft:falling_sand";
    public const string Cramming       = "minecraft:cramming";
    public const string Freeze         = "minecraft:freeze";
    public const string Stalagmite     = "minecraft:stalagmite";
    public const string Sting          = "minecraft:sting";
    public const string SonicBoom      = "minecraft:sonic_boom";

    // ── Damage helpers ────────────────────────────────────────────────────────

    /// <summary>Deal typed damage to a living entity using a damage type registry key.</summary>
    [JavaMethod("if ({0} instanceof LivingEntity _le) _le.damage({0}.getWorld().getDamageSources().create(net.minecraft.registry.RegistryKey.of(net.minecraft.registry.RegistryKeys.DAMAGE_TYPE, Identifier.of({1}))), {2})")]
    public static void DealDamage(McEntity entity, string damageTypeId, float amount) { }

    /// <summary>Deal generic damage to a player.</summary>
    [JavaMethod("{0}.damage(server.getDamageSources().generic(), {1})")]
    public static void DealDamage(McPlayer player, float amount) { }

    /// <summary>Deal typed damage to a player using a damage type registry key.</summary>
    [JavaMethod("{0}.damage({0}.getWorld().getDamageSources().create(net.minecraft.registry.RegistryKey.of(net.minecraft.registry.RegistryKeys.DAMAGE_TYPE, Identifier.of({1}))), {2})")]
    public static void DealDamage(McPlayer player, string damageTypeId, float amount) { }

    /// <summary>Deal fire damage to an entity.</summary>
    [JavaMethod("if ({0} instanceof LivingEntity _lf) _lf.damage({0}.getWorld().getDamageSources().inFire(), {1})")]
    public static void DealFireDamage(McEntity entity, float amount) { }

    /// <summary>Deal fall damage to an entity.</summary>
    [JavaMethod("if ({0} instanceof LivingEntity _lfall) _lfall.damage({0}.getWorld().getDamageSources().fall(), {1})")]
    public static void DealFallDamage(McEntity entity, float amount) { }

    /// <summary>Deal magic damage to an entity.</summary>
    [JavaMethod("if ({0} instanceof LivingEntity _lm) _lm.damage({0}.getWorld().getDamageSources().magic(), {1})")]
    public static void DealMagicDamage(McEntity entity, float amount) { }
}
