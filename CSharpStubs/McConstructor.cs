namespace CSCraft;

/// <summary>
/// Facade for Java's java.lang.reflect.Constructor&lt;?&gt;.
/// Obtained via McType.GetConstructor / McType.GetDeclaredConstructor.
/// Transpiles to java.lang.reflect.Constructor.
/// </summary>
[JavaClass("java.lang.reflect.Constructor")]
public class McConstructor
{
    // ── Identity ──────────────────────────────────────────────────────────────

    [JavaMethod("{target}.getName()")]
    public string Name { get; } = null!;

    /// <summary>The class that declared this constructor.</summary>
    [JavaMethod("{target}.getDeclaringClass()")]
    public McType DeclaringClass { get; } = null!;

    /// <summary>Number of formal parameters.</summary>
    [JavaMethod("{target}.getParameterCount()")]
    public int ParameterCount { get; }

    /// <summary>The types of the formal parameters, in declaration order.</summary>
    [JavaMethod("new java.util.ArrayList<>(java.util.Arrays.asList({target}.getParameterTypes()))")]
    public List<McType> ParameterTypes { get; } = null!;

    // ── Modifiers ─────────────────────────────────────────────────────────────

    [JavaMethod("java.lang.reflect.Modifier.isPublic({target}.getModifiers())")]
    public bool IsPublic { get; }

    [JavaMethod("java.lang.reflect.Modifier.isPrivate({target}.getModifiers())")]
    public bool IsPrivate { get; }

    [JavaMethod("java.lang.reflect.Modifier.isProtected({target}.getModifiers())")]
    public bool IsProtected { get; }

    // ── Access ────────────────────────────────────────────────────────────────

    /// <summary>Override access control to allow invoking private constructors.</summary>
    [JavaMethod("{target}.setAccessible({0})")]
    public void SetAccessible(bool accessible) { }

    [JavaMethod("{target}.isAccessible()")]
    public bool IsAccessible { get; }

    // ── Instantiation ─────────────────────────────────────────────────────────

    /// <summary>Create a new instance using this constructor with no arguments.</summary>
    [JavaMethod("{target}.newInstance()")]
    public object NewInstance() => null!;

    /// <summary>Create a new instance passing one argument.</summary>
    [JavaMethod("{target}.newInstance({0})")]
    public object NewInstance(object? arg1) => null!;

    /// <summary>Create a new instance passing two arguments.</summary>
    [JavaMethod("{target}.newInstance({0}, {1})")]
    public object NewInstance(object? arg1, object? arg2) => null!;

    /// <summary>Create a new instance passing three arguments.</summary>
    [JavaMethod("{target}.newInstance({0}, {1}, {2})")]
    public object NewInstance(object? arg1, object? arg2, object? arg3) => null!;

    /// <summary>Create a new instance passing four arguments.</summary>
    [JavaMethod("{target}.newInstance({0}, {1}, {2}, {3})")]
    public object NewInstance(object? arg1, object? arg2, object? arg3, object? arg4) => null!;

    // ── Annotation ────────────────────────────────────────────────────────────

    [JavaMethod("{target}.isAnnotationPresent(Class.forName({0}))")]
    public bool HasAnnotation(string annotationClassName) => false;
}
