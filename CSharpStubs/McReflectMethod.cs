namespace CSCraft;

/// <summary>
/// Facade for Java's java.lang.reflect.Method.
/// Obtained via McType.GetMethod / McType.GetDeclaredMethod.
/// Named McReflectMethod to avoid conflict with the [JavaMethod] attribute.
/// Transpiles to java.lang.reflect.Method.
/// </summary>
[JavaClass("java.lang.reflect.Method")]
public class McReflectMethod
{
    // ── Identity ──────────────────────────────────────────────────────────────

    [JavaMethod("{target}.getName()")]
    public string Name { get; } = null!;

    /// <summary>The return type of this method.</summary>
    [JavaMethod("{target}.getReturnType()")]
    public McType ReturnType { get; } = null!;

    /// <summary>The class or interface that declared this method.</summary>
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

    [JavaMethod("java.lang.reflect.Modifier.isStatic({target}.getModifiers())")]
    public bool IsStatic { get; }

    [JavaMethod("java.lang.reflect.Modifier.isFinal({target}.getModifiers())")]
    public bool IsFinal { get; }

    [JavaMethod("java.lang.reflect.Modifier.isAbstract({target}.getModifiers())")]
    public bool IsAbstract { get; }

    [JavaMethod("java.lang.reflect.Modifier.isSynchronized({target}.getModifiers())")]
    public bool IsSynchronized { get; }

    [JavaMethod("{target}.isVarArgs()")]
    public bool IsVarArgs { get; }

    [JavaMethod("{target}.isBridge()")]
    public bool IsBridge { get; }

    [JavaMethod("{target}.isSynthetic()")]
    public bool IsSynthetic { get; }

    [JavaMethod("{target}.isDefault()")]
    public bool IsDefault { get; }

    // ── Access ────────────────────────────────────────────────────────────────

    /// <summary>Override access control to allow invoking private/protected methods.</summary>
    [JavaMethod("{target}.setAccessible({0})")]
    public void SetAccessible(bool accessible) { }

    [JavaMethod("{target}.isAccessible()")]
    public bool IsAccessible { get; }

    // ── Invocation ────────────────────────────────────────────────────────────

    /// <summary>Invoke with no arguments (static: pass null as instance).</summary>
    [JavaMethod("{target}.invoke({0})")]
    public object? Invoke(object? instance) => null;

    /// <summary>Invoke with one argument.</summary>
    [JavaMethod("{target}.invoke({0}, {1})")]
    public object? Invoke(object? instance, object? arg1) => null;

    /// <summary>Invoke with two arguments.</summary>
    [JavaMethod("{target}.invoke({0}, {1}, {2})")]
    public object? Invoke(object? instance, object? arg1, object? arg2) => null;

    /// <summary>Invoke with three arguments.</summary>
    [JavaMethod("{target}.invoke({0}, {1}, {2}, {3})")]
    public object? Invoke(object? instance, object? arg1, object? arg2, object? arg3) => null;

    /// <summary>Invoke with four arguments.</summary>
    [JavaMethod("{target}.invoke({0}, {1}, {2}, {3}, {4})")]
    public object? Invoke(object? instance, object? arg1, object? arg2, object? arg3, object? arg4) => null;

    // ── Annotation ────────────────────────────────────────────────────────────

    [JavaMethod("{target}.isAnnotationPresent(Class.forName({0}))")]
    public bool HasAnnotation(string annotationClassName) => false;
}
