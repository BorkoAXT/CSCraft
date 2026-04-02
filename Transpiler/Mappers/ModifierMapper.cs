namespace Transpiler;

/// <summary>
/// Maps C# access modifiers, keywords, and operators to Java equivalents.
/// </summary>
public static class ModifierMapper
{
    // ── Access modifiers ──────────────────────────────────────────────────────

    public static readonly Dictionary<string, string> AccessModifiers = new()
    {
        ["public"]    = "public",
        ["private"]   = "private",
        ["protected"] = "protected",
        ["internal"]  = "/* package-private */",  // no direct equivalent — drop
    };

    // ── Type modifiers ────────────────────────────────────────────────────────

    public static readonly Dictionary<string, string> TypeModifiers = new()
    {
        ["static"]   = "static",
        ["abstract"] = "abstract",
        ["sealed"]   = "final",       // sealed class → final class
        ["virtual"]  = "",            // all Java methods are virtual by default
        ["override"] = "@Override",   // becomes annotation, not keyword
        ["readonly"] = "final",
        ["const"]    = "static final",
        ["partial"]  = "",            // no equivalent — drop
        ["extern"]   = "native",
        ["unsafe"]   = "",            // no equivalent — drop
        ["volatile"] = "volatile",
        ["async"]    = "",            // handled separately via CompletableFuture
    };

    // ── Operators ─────────────────────────────────────────────────────────────

    public static readonly Dictionary<string, string> Operators = new()
    {
        // Null handling
        ["??"]  = "!= null ?",   // x ?? y  →  x != null ? x : y  (simplified)
        ["?."]  = ".",            // x?.y    →  x.y  (emitter adds null check)
        ["!"]   = "!",

        // Logical
        ["&&"]  = "&&",
        ["||"]  = "||",

        // Bitwise
        ["^"]   = "^",
        ["&"]   = "&",
        ["|"]   = "|",
        ["~"]   = "~",
        ["<<"]  = "<<",
        [">>"]  = ">>",
        [">>>"] = ">>>",
    };

    // ── Literal keywords ──────────────────────────────────────────────────────

    public static readonly Dictionary<string, string> Keywords = new()
    {
        ["true"]    = "true",
        ["false"]   = "false",
        ["null"]    = "null",
        ["this"]    = "this",
        ["base"]    = "super",
        ["typeof"]  = "",         // handled specially in emitter
        ["nameof"]  = "",         // handled specially in emitter
        ["is"]      = "instanceof",
        ["as"]      = "",         // emitter replaces with cast
        ["new"]     = "new",
        ["return"]  = "return",
        ["void"]    = "void",
        ["var"]     = "var",      // Java 10+ supports var
        ["throw"]   = "throw",
        ["try"]     = "try",
        ["catch"]   = "catch",
        ["finally"] = "finally",
        ["using"]   = "",         // using blocks → try-with-resources (complex)
        ["foreach"] = "for",      // foreach (x in y) → for (var x : y)
        ["for"]     = "for",
        ["while"]   = "while",
        ["do"]      = "do",
        ["if"]      = "if",
        ["else"]    = "else",
        ["switch"]  = "switch",
        ["case"]    = "case",
        ["break"]   = "break",
        ["continue"]= "continue",
        ["default"] = "default",
        ["in"]      = ":",        // foreach part
    };

    // ── String interpolation ──────────────────────────────────────────────────
    // C#: $"Hello {name}, you have {count} items"
    // Java: String.format("Hello %s, you have %d items", name, count)
    // This is complex — handled in the emitter, not just a dict lookup.
    // But we keep the format specifier mapping here.

    public static string MapFormatSpecifier(string csType) => csType switch
    {
        "int" or "long" or "short" or "byte" => "%d",
        "float" or "double"                  => "%f",
        "bool" or "boolean"                  => "%b",
        "char"                               => "%c",
        _                                    => "%s",  // default: toString()
    };

    // ── Attribute → Annotation ────────────────────────────────────────────────
    // C# attributes that have Java annotation equivalents

    public static readonly Dictionary<string, string> Attributes = new()
    {
        ["Obsolete"]        = "@Deprecated",
        ["Override"]        = "@Override",
        ["Serializable"]    = "@java.io.Serializable", // actually interface in Java
        ["FunctionalInterface"] = "@FunctionalInterface",
        ["SuppressWarnings"]= "@SuppressWarnings",
    };

    // ── Public API ────────────────────────────────────────────────────────────

    public static string MapModifier(string cs)
    {
        if (TypeModifiers.TryGetValue(cs, out var m)) return m;
        if (AccessModifiers.TryGetValue(cs, out var a)) return a;
        return cs;
    }

    public static string MapKeyword(string cs)
        => Keywords.TryGetValue(cs, out var k) ? k : cs;

    public static string MapOperator(string cs)
        => Operators.TryGetValue(cs, out var op) ? op : cs;
}