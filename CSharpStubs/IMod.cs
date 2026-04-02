namespace CSCraft;

/// <summary>
/// Implement this interface on your mod's main class.
/// Transpiles to Java's ModInitializer.
/// The OnInitialize method maps to onInitialize().
/// </summary>
[JavaClass("net.fabricmc.api.ModInitializer")]
public interface IMod
{
    [JavaMethod("onInitialize")]
    void OnInitialize();
}
