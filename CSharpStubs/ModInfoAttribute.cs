namespace CSCraft;

/// <summary>
/// Place this attribute on your mod class (the one implementing IMod) to configure
/// all Fabric template settings automatically. When you build, CSCraft reads these
/// values and generates the entire FabricTemplate directory — no manual Gradle setup needed.
///
/// Example:
/// <code>
/// [ModInfo(
///     Id          = "mymod",
///     Name        = "My Cool Mod",
///     Version     = "1.0.0",
///     Author      = "YourName",
///     Description = "A cool Fabric mod written in C#"
/// )]
/// public class MyMod : IMod { ... }
/// </code>
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class ModInfoAttribute : Attribute
{
    /// <summary>
    /// Required. The mod ID used in Fabric (lowercase, no spaces).
    /// Example: "mymod"
    /// </summary>
    public string Id { get; set; } = "";

    /// <summary>
    /// Required. The display name of your mod.
    /// Example: "My Cool Mod"
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// Mod version. Default: "1.0.0"
    /// </summary>
    public string Version { get; set; } = "1.0.0";

    /// <summary>
    /// Author name shown in Fabric mod menu.
    /// </summary>
    public string Author { get; set; } = "";

    /// <summary>
    /// A short description of what the mod does.
    /// </summary>
    public string Description { get; set; } = "";

    /// <summary>
    /// Target Minecraft version. Default: "1.21.1"
    /// The build system automatically resolves Yarn mappings, Fabric loader,
    /// and Fabric API versions for the chosen Minecraft version.
    /// Supported: any release from 1.20 onward, and weekly snapshots (e.g. "25w14a").
    /// For versions not yet in the catalog, set the manual overrides below.
    /// </summary>
    public string MinecraftVersion { get; set; } = "1.21.1";

    /// <summary>
    /// Override the Yarn mappings version. Leave empty to use the auto-resolved value.
    /// Example: "1.21.4+build.8"
    /// Required when targeting a snapshot or pre-release not yet in the catalog.
    /// </summary>
    public string YarnMappings { get; set; } = "";

    /// <summary>
    /// Override the Fabric Loader version. Leave empty to use the auto-resolved value.
    /// Example: "0.16.10"
    /// </summary>
    public string FabricLoaderVersion { get; set; } = "";

    /// <summary>
    /// Override the Fabric API version. Leave empty to use the auto-resolved value.
    /// Example: "0.119.5+1.21.4"
    /// </summary>
    public string FabricApiVersion { get; set; } = "";

    /// <summary>
    /// Override the Fabric Loom Gradle plugin version. Leave empty to use the auto-resolved value.
    /// Example: "1.9.5"
    /// </summary>
    public string LoomVersion { get; set; } = "";

    /// <summary>
    /// Java package name for the generated Java code.
    /// Default: derived from Author + Id (e.g. "com.yourname.mymod").
    /// Override if you want a custom package structure.
    /// </summary>
    public string PackageName { get; set; } = "";
}
