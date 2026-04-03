namespace CSCraft;

/// <summary>
/// Facade for Minecraft status effects (StatusEffect / StatusEffectInstance).
/// Use McRegistry.RegisterStatusEffect() for custom effects.
/// Transpiles to Java's StatusEffect / StatusEffectInstance.
/// </summary>
[JavaClass("net.minecraft.entity.effect.StatusEffect")]
public class McStatusEffect
{
    public enum EffectType
    {
        Beneficial,
        Harmful,
        Neutral,
    }

    [JavaMethod("Registries.STATUS_EFFECT.getId({target}).toString()")]
    public string Id { get; } = null!;

    [JavaMethod("{target}.isBeneficial()")]
    public bool IsBeneficial { get; }

    [JavaMethod("{target}.isInstant()")]
    public bool IsInstant { get; }
}

/// <summary>
/// Represents an active status effect instance on an entity.
/// </summary>
[JavaClass("net.minecraft.entity.effect.StatusEffectInstance")]
public class McStatusEffectInstance
{
    public McStatusEffectInstance(McStatusEffect effect, int durationTicks, int amplifier = 0) { }

    [JavaMethod("{target}.getEffectType()")]
    public McStatusEffect Effect { get; } = null!;

    [JavaMethod("{target}.getDuration()")]
    public int Duration { get; }

    [JavaMethod("{target}.getAmplifier()")]
    public int Amplifier { get; }

    [JavaMethod("{target}.isDurationBelow({0})")]
    public bool IsDurationBelow(int ticks) => false;
}
