namespace CSCraft;

/// <summary>
/// Marks a C# class as mapping to a specific Java class.
/// The transpiler reads this to know what Java type to emit.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
public sealed class JavaClassAttribute : Attribute
{
    public string FullyQualifiedName { get; }
    public JavaClassAttribute(string fullyQualifiedName)
        => FullyQualifiedName = fullyQualifiedName;
}

/// <summary>
/// Marks a C# method as mapping to a specific Java method template.
/// {target} = receiver, {0},{1}... = arguments.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field)]
public sealed class JavaMethodAttribute : Attribute
{
    public string Template { get; }
    public JavaMethodAttribute(string template) => Template = template;
}

/// <summary>
/// Marks a C# event as mapping to a Fabric event registration.
/// </summary>
[AttributeUsage(AttributeTargets.Event)]
public sealed class JavaEventAttribute : Attribute
{
    public string FabricClass { get; }
    public string FabricEvent { get; }
    public JavaEventAttribute(string fabricClass, string fabricEvent)
    {
        FabricClass = fabricClass;
        FabricEvent = fabricEvent;
    }
}

/// <summary>
/// Decorates a class with mod configuration metadata.
/// CSCraft generates a TOML config file at config/modid.toml,
/// registers a /modid reload command, and hot-reloads on file change.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class ModConfigAttribute : Attribute { }

/// <summary>
/// Declares the texture for a CustomItem subclass.
/// textureName matches a file in Assets/textures/item/ (without .png extension).
/// CSCraft generates the item model JSON automatically.
/// Example: [ItemTexture("my_gem")] → Assets/textures/item/my_gem.png
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class ItemTextureAttribute : Attribute
{
    public string TextureName { get; }
    public ItemTextureAttribute(string textureName) => TextureName = textureName;
}

/// <summary>
/// Declares the texture(s) for a CustomBlock subclass.
/// CSCraft generates the block model JSON and blockstate JSON automatically.
/// Single texture (cube_all): [BlockTexture("magic_stone")]
/// Per-face textures: [BlockTexture(side: "stone_side", top: "stone_top", bottom: "stone_bottom")]
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class BlockTextureAttribute : Attribute
{
    /// <summary>Texture name used for all sides (cube_all model).</summary>
    public string? All { get; }
    public string? Side   { get; }
    public string? Top    { get; }
    public string? Bottom { get; }
    public string? Front  { get; }
    public string? Back   { get; }

    /// <summary>All sides use the same texture.</summary>
    public BlockTextureAttribute(string all) => All = all;

    /// <summary>Directional block (like a furnace): different side, top, and bottom textures.</summary>
    public BlockTextureAttribute(string side, string top, string bottom)
    {
        Side = side; Top = top; Bottom = bottom;
    }

    /// <summary>Full per-face control.</summary>
    public BlockTextureAttribute(string side, string top, string bottom, string front, string back)
    {
        Side = side; Top = top; Bottom = bottom; Front = front; Back = back;
    }
}

/// <summary>
/// Registers a custom sound event backed by a .ogg file in Assets/sounds/.
/// CSCraft generates sounds.json automatically.
/// Example: [CustomSound("mymod:my_explosion")] → Assets/sounds/my_explosion.ogg
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Field, AllowMultiple = true)]
public sealed class CustomSoundAttribute : Attribute
{
    public string SoundId { get; }
    public CustomSoundAttribute(string soundId) => SoundId = soundId;
}
