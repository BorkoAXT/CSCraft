namespace CSCraft;

/// <summary>
/// Facade for Minecraft sound events.
/// Use McRegistry.RegisterSound() for custom sounds.
/// Transpiles to Java's SoundEvent + SoundCategory.
/// </summary>
[JavaClass("net.minecraft.sound.SoundEvent")]
public class McSoundEvent
{
    [JavaMethod("Registries.SOUND_EVENT.getId({target}).toString()")]
    public string Id { get; } = null!;
}

/// <summary>
/// Sound category constants for controlling which audio channel plays the sound.
/// </summary>
public static class McSoundCategory
{
    [JavaMethod("SoundCategory.MASTER")]   public static readonly string Master  = "MASTER";
    [JavaMethod("SoundCategory.MUSIC")]    public static readonly string Music   = "MUSIC";
    [JavaMethod("SoundCategory.RECORDS")]  public static readonly string Records = "RECORDS";
    [JavaMethod("SoundCategory.WEATHER")]  public static readonly string Weather = "WEATHER";
    [JavaMethod("SoundCategory.BLOCKS")]   public static readonly string Blocks  = "BLOCKS";
    [JavaMethod("SoundCategory.HOSTILE")]  public static readonly string Hostile = "HOSTILE";
    [JavaMethod("SoundCategory.NEUTRAL")]  public static readonly string Neutral = "NEUTRAL";
    [JavaMethod("SoundCategory.PLAYERS")]  public static readonly string Players = "PLAYERS";
    [JavaMethod("SoundCategory.AMBIENT")]  public static readonly string Ambient = "AMBIENT";
    [JavaMethod("SoundCategory.VOICE")]    public static readonly string Voice   = "VOICE";
}
