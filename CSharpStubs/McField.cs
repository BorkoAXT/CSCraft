namespace CSCraft;

/// <summary>
/// Facade for Java's java.lang.reflect.Field.
/// Obtained via McType.GetField / McType.GetDeclaredField.
/// Transpiles to java.lang.reflect.Field.
/// </summary>
[JavaClass("java.lang.reflect.Field")]
public class McField
{
    // ── Identity ──────────────────────────────────────────────────────────────

    [JavaMethod("{target}.getName()")]
    public string Name { get; } = null!;

    /// <summary>The declared type of this field.</summary>
    [JavaMethod("{target}.getType()")]
    public McType FieldType { get; } = null!;

    /// <summary>The class that declared this field.</summary>
    [JavaMethod("{target}.getDeclaringClass()")]
    public McType DeclaringClass { get; } = null!;

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

    [JavaMethod("java.lang.reflect.Modifier.isTransient({target}.getModifiers())")]
    public bool IsTransient { get; }

    [JavaMethod("java.lang.reflect.Modifier.isVolatile({target}.getModifiers())")]
    public bool IsVolatile { get; }

    // ── Access ────────────────────────────────────────────────────────────────

    /// <summary>Override Java access control so private/protected fields can be read/written.</summary>
    [JavaMethod("{target}.setAccessible({0})")]
    public void SetAccessible(bool accessible) { }

    [JavaMethod("{target}.isAccessible()")]
    public bool IsAccessible { get; }

    // ── Object get/set ────────────────────────────────────────────────────────

    /// <summary>Get the value of this field on the given object (null for static fields).</summary>
    [JavaMethod("{target}.get({0})")]
    public object? Get(object? instance) => null;

    /// <summary>Set the value of this field on the given object (null for static fields).</summary>
    [JavaMethod("{target}.set({0}, {1})")]
    public void Set(object? instance, object? value) { }

    // ── Primitive get/set ─────────────────────────────────────────────────────

    [JavaMethod("{target}.getInt({0})")]
    public int GetInt(object? instance) => 0;

    [JavaMethod("{target}.setInt({0}, {1})")]
    public void SetInt(object? instance, int value) { }

    [JavaMethod("{target}.getLong({0})")]
    public long GetLong(object? instance) => 0;

    [JavaMethod("{target}.setLong({0}, {1})")]
    public void SetLong(object? instance, long value) { }

    [JavaMethod("{target}.getFloat({0})")]
    public float GetFloat(object? instance) => 0f;

    [JavaMethod("{target}.setFloat({0}, {1})")]
    public void SetFloat(object? instance, float value) { }

    [JavaMethod("{target}.getDouble({0})")]
    public double GetDouble(object? instance) => 0.0;

    [JavaMethod("{target}.setDouble({0}, {1})")]
    public void SetDouble(object? instance, double value) { }

    [JavaMethod("{target}.getBoolean({0})")]
    public bool GetBool(object? instance) => false;

    [JavaMethod("{target}.setBoolean({0}, {1})")]
    public void SetBool(object? instance, bool value) { }

    [JavaMethod("{target}.getByte({0})")]
    public byte GetByte(object? instance) => 0;

    [JavaMethod("{target}.setByte({0}, {1})")]
    public void SetByte(object? instance, byte value) { }

    [JavaMethod("{target}.getShort({0})")]
    public short GetShort(object? instance) => 0;

    [JavaMethod("{target}.setShort({0}, {1})")]
    public void SetShort(object? instance, short value) { }

    [JavaMethod("{target}.getChar({0})")]
    public char GetChar(object? instance) => '\0';

    [JavaMethod("{target}.setChar({0}, {1})")]
    public void SetChar(object? instance, char value) { }

    // ── Annotation ────────────────────────────────────────────────────────────

    [JavaMethod("{target}.isAnnotationPresent(Class.forName({0}))")]
    public bool HasAnnotation(string annotationClassName) => false;
}
