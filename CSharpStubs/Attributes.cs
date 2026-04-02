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
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
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
