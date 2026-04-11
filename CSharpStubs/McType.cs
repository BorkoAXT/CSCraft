namespace CSCraft;

/// <summary>
/// Facade for Java's java.lang.Class&lt;?&gt;.
/// Obtain via obj.GetType(), typeof(SomeClass), or McType.ForName("full.class.Name").
/// Transpiles to java.lang.Class.
/// </summary>
[JavaClass("java.lang.Class")]
public class McType
{
    // ── Identity ──────────────────────────────────────────────────────────────

    /// <summary>Fully-qualified class name, e.g. "net.minecraft.entity.Entity".</summary>
    [JavaMethod("{target}.getName()")]
    public string Name { get; } = null!;

    /// <summary>Simple (unqualified) class name, e.g. "Entity".</summary>
    [JavaMethod("{target}.getSimpleName()")]
    public string SimpleName { get; } = null!;

    /// <summary>Canonical name (e.g. "net.minecraft.entity.Entity").</summary>
    [JavaMethod("{target}.getCanonicalName()")]
    public string CanonicalName { get; } = null!;

    // ── Hierarchy ─────────────────────────────────────────────────────────────

    /// <summary>The direct superclass, or null for Object and interfaces.</summary>
    [JavaMethod("{target}.getSuperclass()")]
    public McType? Superclass { get; }

    /// <summary>All interfaces directly implemented by this class.</summary>
    [JavaMethod("new java.util.ArrayList<>(java.util.Arrays.asList({target}.getInterfaces()))")]
    public List<McType> Interfaces { get; } = null!;

    // ── Class kind ────────────────────────────────────────────────────────────

    [JavaMethod("{target}.isInterface()")]
    public bool IsInterface { get; }

    [JavaMethod("{target}.isArray()")]
    public bool IsArray { get; }

    [JavaMethod("{target}.isPrimitive()")]
    public bool IsPrimitive { get; }

    [JavaMethod("{target}.isEnum()")]
    public bool IsEnum { get; }

    [JavaMethod("{target}.isAnnotation()")]
    public bool IsAnnotation { get; }

    [JavaMethod("{target}.isAnonymousClass()")]
    public bool IsAnonymous { get; }

    [JavaMethod("{target}.isSynthetic()")]
    public bool IsSynthetic { get; }

    // ── Type checking ─────────────────────────────────────────────────────────

    /// <summary>Returns true if otherType is the same as, or a subtype of, this type.</summary>
    [JavaMethod("{target}.isAssignableFrom({0})")]
    public bool IsAssignableFrom(McType otherType) => false;

    /// <summary>Returns true if obj is an instance of this class.</summary>
    [JavaMethod("{target}.isInstance({0})")]
    public bool IsInstance(object obj) => false;

    /// <summary>Casts obj to this type and returns it.</summary>
    [JavaMethod("{target}.cast({0})")]
    public object Cast(object obj) => null!;

    // ── Fields ────────────────────────────────────────────────────────────────

    /// <summary>Get a public field by name (searches superclasses too).</summary>
    [JavaMethod("{target}.getField({0})")]
    public McField GetField(string name) => null!;

    /// <summary>Get a declared field by name (any access, this class only).</summary>
    [JavaMethod("{target}.getDeclaredField({0})")]
    public McField GetDeclaredField(string name) => null!;

    /// <summary>All public fields including inherited ones.</summary>
    [JavaMethod("new java.util.ArrayList<>(java.util.Arrays.asList({target}.getFields()))")]
    public List<McField> GetFields() => null!;

    /// <summary>All fields declared directly in this class (any access level).</summary>
    [JavaMethod("new java.util.ArrayList<>(java.util.Arrays.asList({target}.getDeclaredFields()))")]
    public List<McField> GetDeclaredFields() => null!;

    // ── Methods ───────────────────────────────────────────────────────────────

    /// <summary>Get a public method by name (no-arg overload).</summary>
    [JavaMethod("{target}.getMethod({0})")]
    public McReflectMethod GetMethod(string name) => null!;

    /// <summary>Get a declared method by name (no-arg, any access).</summary>
    [JavaMethod("{target}.getDeclaredMethod({0})")]
    public McReflectMethod GetDeclaredMethod(string name) => null!;

    /// <summary>All public methods including inherited ones.</summary>
    [JavaMethod("new java.util.ArrayList<>(java.util.Arrays.asList({target}.getMethods()))")]
    public List<McReflectMethod> GetMethods() => null!;

    /// <summary>All methods declared directly in this class (any access level).</summary>
    [JavaMethod("new java.util.ArrayList<>(java.util.Arrays.asList({target}.getDeclaredMethods()))")]
    public List<McReflectMethod> GetDeclaredMethods() => null!;

    // ── Constructors ──────────────────────────────────────────────────────────

    /// <summary>Get the public no-arg constructor.</summary>
    [JavaMethod("{target}.getConstructor()")]
    public McConstructor GetConstructor() => null!;

    /// <summary>Get the declared no-arg constructor (any access).</summary>
    [JavaMethod("{target}.getDeclaredConstructor()")]
    public McConstructor GetDeclaredConstructor() => null!;

    /// <summary>All public constructors.</summary>
    [JavaMethod("new java.util.ArrayList<>(java.util.Arrays.asList({target}.getConstructors()))")]
    public List<McConstructor> GetConstructors() => null!;

    /// <summary>All declared constructors (any access level).</summary>
    [JavaMethod("new java.util.ArrayList<>(java.util.Arrays.asList({target}.getDeclaredConstructors()))")]
    public List<McConstructor> GetDeclaredConstructors() => null!;

    // ── Instantiation ─────────────────────────────────────────────────────────

    /// <summary>Create a new instance using the no-arg constructor.</summary>
    [JavaMethod("{target}.getDeclaredConstructor().newInstance()")]
    public object NewInstance() => null!;

    // ── Annotations ───────────────────────────────────────────────────────────

    /// <summary>Check whether this class has a specific annotation (by fully-qualified name).</summary>
    [JavaMethod("{target}.isAnnotationPresent(Class.forName({0}))")]
    public bool HasAnnotation(string annotationClassName) => false;

    // ── Static factory ────────────────────────────────────────────────────────

    /// <summary>Load a class by its fully-qualified name. Throws ClassNotFoundException.</summary>
    [JavaMethod("Class.forName({0})")]
    public static McType ForName(string className) => null!;

    /// <summary>Load a class using a specific ClassLoader.</summary>
    [JavaMethod("Class.forName({0}, true, {1}.getClass().getClassLoader())")]
    public static McType ForName(string className, object classLoader) => null!;
}
