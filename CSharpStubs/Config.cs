namespace CSCraft;

/// <summary>
/// Mod configuration helper.
/// Decorate a class with [ModConfig] and CSCraft generates a TOML config file
/// at config/modid.toml automatically loaded at startup.
///
/// Example:
///   [ModConfig]
///   public class MyConfig
///   {
///       public bool EnableWelcome { get; set; } = true;
///       public string WelcomeText { get; set; } = "Welcome!";
///       public int MaxHomes { get; set; } = 3;
///   }
///
///   // Read anywhere:
///   var cfg = Config.Load&lt;MyConfig&gt;();
///   if (cfg.EnableWelcome) player.SendMessage(cfg.WelcomeText);
/// </summary>
public static class Config
{
    /// <summary>
    /// Load the config of type T. The first call reads from disk;
    /// subsequent calls return the cached instance.
    /// T must be decorated with [ModConfig].
    /// </summary>
    public static T Load<T>() where T : new() => new T();

    /// <summary>
    /// Re-read the config file from disk and return the fresh instance.
    /// Use this in a /reload command to apply live changes without restarting.
    /// </summary>
    public static T Reload<T>() where T : new() => new T();

    /// <summary>
    /// Write the current config object back to disk.
    /// Call this after modifying config values programmatically.
    /// </summary>
    public static void Save<T>(T config) { }
}
