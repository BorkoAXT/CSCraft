using System.Text;
using System.Text.RegularExpressions;
using CSCraft.Detranspiler.Maps;

namespace CSCraft.Detranspiler.Translators;

/// <summary>
/// Converts a Java Fabric mod class to a C# CSCraft class.
/// Works at the text level — line-by-line with regex pattern matching.
/// Produces a "first draft" that compiles with minor manual cleanup.
/// </summary>
public class ClassTranslator
{
    private readonly string _java;
    private readonly string _modId;
    private readonly string _className;

    public ClassTranslator(string javaSource, string modId, string className)
    {
        _java      = javaSource;
        _modId     = modId;
        _className = className;
    }

    public string Translate()
    {
        var sb = new StringBuilder();
        sb.AppendLine("using CSCraft;");
        sb.AppendLine();

        // Find class-level fields
        var fields = ExtractFields();

        // Find class name and write class declaration
        sb.AppendLine($"public class {_className} : IMod");
        sb.AppendLine("{");

        // Write fields
        foreach (var f in fields)
            sb.AppendLine($"    {f}");
        if (fields.Count > 0) sb.AppendLine();

        // Write OnInitialize
        sb.AppendLine("    public void OnInitialize()");
        sb.AppendLine("    {");
        var initBody = ExtractMethodBody("onInitialize");
        foreach (var line in TranslateBody(initBody, 2))
            sb.AppendLine(line);
        sb.AppendLine("    }");

        // Write other methods (registerContent, registerCommands, registerEvents, etc.)
        foreach (var method in ExtractMethods())
        {
            if (method.Name == "onInitialize") continue;
            sb.AppendLine();
            string csName = ToPascalCase(method.Name);
            sb.AppendLine($"    private void {csName}()");
            sb.AppendLine("    {");
            foreach (var line in TranslateBody(method.Body, 2))
                sb.AppendLine(line);
            sb.AppendLine("    }");
        }

        sb.AppendLine("}");
        return sb.ToString();
    }

    // ── Field extraction ──────────────────────────────────────────────────────

    private List<string> ExtractFields()
    {
        var fields = new List<string>();
        var fieldRx = new Regex(@"^\s{4}public\s+(\w+)\s+(\w+)\s*=\s*null\s*;", RegexOptions.Multiline);
        foreach (Match m in fieldRx.Matches(_java))
        {
            string javaType = m.Groups[1].Value;
            string name     = m.Groups[2].Value;
            string csType   = ReverseTypeMapper.Map(javaType);
            fields.Add($"{csType} {name} = null!;");
        }
        return fields;
    }

    // ── Method extraction ─────────────────────────────────────────────────────

    private record MethodInfo(string Name, string Body);

    private string ExtractMethodBody(string methodName)
    {
        var rx = new Regex($@"public\s+void\s+{Regex.Escape(methodName)}\s*\(\s*\)\s*\{{");
        var m = rx.Match(_java);
        if (!m.Success) return "";
        int start = m.Index + m.Length;
        return ExtractBraceBlock(_java, start);
    }

    private List<MethodInfo> ExtractMethods()
    {
        var result  = new List<MethodInfo>();
        var methodRx = new Regex(@"public\s+void\s+(\w+)\s*\(\s*\)\s*\{");
        foreach (Match m in methodRx.Matches(_java))
        {
            string name = m.Groups[1].Value;
            string body = ExtractBraceBlock(_java, m.Index + m.Length);
            result.Add(new MethodInfo(name, body));
        }
        return result;
    }

    private static string ExtractBraceBlock(string src, int afterOpenBrace)
    {
        int depth = 1;
        int i = afterOpenBrace;
        var sb = new StringBuilder();
        while (i < src.Length && depth > 0)
        {
            char c = src[i];
            if (c == '{') depth++;
            else if (c == '}') { depth--; if (depth == 0) break; }
            else if (c == '"') // skip string contents
            {
                sb.Append(c); i++;
                while (i < src.Length && src[i] != '"')
                {
                    if (src[i] == '\\') { sb.Append(src[i++]); }
                    sb.Append(src[i++]);
                }
            }
            if (depth > 0) sb.Append(c);
            i++;
        }
        return sb.ToString();
    }

    // ── Body translation ──────────────────────────────────────────────────────

    private IEnumerable<string> TranslateBody(string body, int indentLevel)
    {
        string indent = new string(' ', indentLevel * 4);
        // Split into logical statements (semicolon-terminated or brace blocks)
        var statements = SplitStatements(body);
        foreach (var stmt in statements)
        {
            string trimmed = stmt.Trim();
            if (string.IsNullOrWhiteSpace(trimmed)) continue;

            // Skip Java boilerplate we don't need
            if (ShouldSkip(trimmed)) continue;

            string translated = TranslateStatement(trimmed, indentLevel);
            if (translated.Contains('\n'))
            {
                // Multi-line output — indent each line
                foreach (var l in translated.Split('\n'))
                    if (!string.IsNullOrWhiteSpace(l))
                        yield return $"{indent}{l.TrimStart()}";
            }
            else
            {
                yield return $"{indent}{translated}";
            }
        }
    }

    private string TranslateStatement(string stmt, int indentLevel)
    {
        string indent = new string(' ', indentLevel * 4);
        string inner  = new string(' ', (indentLevel + 1) * 4);

        // ── Event registration ────────────────────────────────────────────────
        var eventMap = ReverseEventMapper.Match(stmt);
        if (eventMap != null)
            return TranslateEventRegistration(stmt, eventMap, indentLevel);

        // ── Command registration ──────────────────────────────────────────────
        if (stmt.Contains("CommandRegistrationCallback.EVENT.register"))
            return TranslateCommandRegistration(stmt, indentLevel);

        // ── RunLater / RunRepeating tick handlers ─────────────────────────────
        if (stmt.Contains("ServerTickEvents.END_SERVER_TICK.register") && stmt.Contains("_rltick"))
            return TranslateRunLater(stmt, indentLevel);
        if (stmt.Contains("ServerTickEvents.END_SERVER_TICK.register") && stmt.Contains("_rrtick"))
            return TranslateRunRepeating(stmt, indentLevel);

        // ── if statement ──────────────────────────────────────────────────────
        if (stmt.StartsWith("if ") || stmt.StartsWith("if("))
            return TranslateIf(stmt, indentLevel);

        // ── Local variable declaration ────────────────────────────────────────
        var localDecl = TryTranslateLocalDecl(stmt);
        if (localDecl != null) return localDecl;

        // ── Method call / expression statement ───────────────────────────────
        // ── Private method calls (registerX() → RegisterX()) ─────────────────
        var methodCallRx = new Regex(@"^([a-z]\w+)\(\);$");
        var methodCallM = methodCallRx.Match(stmt);
        if (methodCallM.Success)
            return ToPascalCase(methodCallM.Groups[1].Value) + "();";

        string expr = ReverseMethodMapper.Translate(stmt.TrimEnd(';'));
        if (expr != stmt.TrimEnd(';'))
            return expr + ";";

        // ── TODO for unrecognized patterns ────────────────────────────────────
        // If it looks like Java-specific boilerplate, comment it out
        if (IsJavaBoilerplate(stmt))
            return $"// TODO: {EscapeComment(stmt)}";

        return stmt;
    }

    // ── Event registration translation ────────────────────────────────────────

    private string TranslateEventRegistration(string stmt, ReverseEventMapper.EventMapping map, int indentLevel)
    {
        string indent = new string(' ', indentLevel * 4);
        string inner  = new string(' ', (indentLevel + 1) * 4);

        // Extract the lambda body from the registration
        string body = ExtractLambdaBody(stmt);

        // Remove known preamble lines
        body = StripEventPreamble(body, map.PreambleVars);

        // Build lambda parameter list
        string paramList = string.Join(", ", map.CsParams
            .Zip(map.CsParamTypes)
            .Select(p => $"{p.Second} {p.First}"));
        if (map.CsParams.Length == 1) paramList = map.CsParams[0];

        var sb = new StringBuilder();
        sb.AppendLine($"Events.{map.CsEvent} += ({paramList}) =>");
        sb.AppendLine($"{indent}{{");
        foreach (var line in TranslateBody(body, indentLevel + 1))
            sb.AppendLine(line);
        sb.AppendLine($"{indent}}};");
        return sb.ToString().TrimEnd();
    }

    // ── Command registration translation ─────────────────────────────────────

    private string TranslateCommandRegistration(string stmt, int indentLevel)
    {
        string indent = new string(' ', indentLevel * 4);

        // Extract command name
        var nameMatch = Regex.Match(stmt, @"CommandManager\.literal\(""(.+?)""\)");
        if (!nameMatch.Success)
            return $"// TODO: command registration\n{indent}// {EscapeComment(stmt)}";

        string cmdName = nameMatch.Groups[1].Value;
        bool isOp = stmt.Contains("hasPermissionLevel(2)");

        // Determine variant based on argument types
        string variant = "Register";
        string extraParam = "";
        string extraType  = "";
        string extraVar   = "";

        var intArgMatch    = Regex.Match(stmt, @"CommandManager\.argument\(""(\w+)"",\s*IntegerArgumentType");
        var strArgMatch    = Regex.Match(stmt, @"CommandManager\.argument\(""(\w+)"",\s*StringArgumentType");
        var playerArgMatch = Regex.Match(stmt, @"CommandManager\.argument\(""(\w+)"",\s*EntityArgumentType\.player");

        if (playerArgMatch.Success)
        {
            variant    = isOp ? "RegisterOpWithPlayer" : "RegisterWithPlayer";
            extraParam = $", \"{playerArgMatch.Groups[1].Value}\"";
        }
        else if (intArgMatch.Success)
        {
            variant    = "RegisterWithInt";
            extraParam = $", \"{intArgMatch.Groups[1].Value}\"";
            extraVar   = IntArgVarName(stmt);
            extraType  = "int";
        }
        else if (strArgMatch.Success)
        {
            variant    = "RegisterWithString";
            extraParam = $", \"{strArgMatch.Groups[1].Value}\"";
            extraVar   = StrArgVarName(stmt);
            extraType  = "string";
        }
        else if (isOp)
        {
            variant = "RegisterOp";
        }

        // Extract body and strip preamble
        string body = ExtractCommandBody(stmt);
        body = StripCommandPreamble(body, extraVar, extraType);

        // Build source var name
        string srcVar = ExtractSrcVar(stmt);

        var sb = new StringBuilder();
        if (extraType.Length > 0)
        {
            string argVar  = string.IsNullOrEmpty(extraVar) ? "arg" : extraVar;
            sb.AppendLine($"McCommand.{variant}(\"{cmdName}\"{extraParam}, ({srcVar}, {argVar}) =>");
        }
        else if (variant.Contains("WithPlayer"))
        {
            string targetVar = ExtractTargetVar(stmt);
            sb.AppendLine($"McCommand.{variant}(\"{cmdName}\"{extraParam}, ({srcVar}, {targetVar}) =>");
        }
        else
        {
            sb.AppendLine($"McCommand.{variant}(\"{cmdName}\", ({srcVar}) =>");
        }

        sb.AppendLine($"{indent}{{");
        foreach (var line in TranslateBody(body, indentLevel + 1))
            sb.AppendLine(line);
        sb.AppendLine($"{indent}}});");
        return sb.ToString().TrimEnd();
    }

    private string TranslateRunLater(string stmt, int indentLevel)
    {
        var tickMatch = Regex.Match(stmt, @"_rltick\[0\]\s*==\s*(\d+)");
        string ticks = tickMatch.Success ? tickMatch.Groups[1].Value : "200";
        string body = ExtractInnerLambdaBody(stmt, "_rlserv");
        string indent = new string(' ', indentLevel * 4);
        var sb = new StringBuilder();
        sb.AppendLine($"McScheduler.RunLater(server, {ticks}, (s) =>");
        sb.AppendLine($"{indent}{{");
        foreach (var line in TranslateBody(body, indentLevel + 1))
            sb.AppendLine(line);
        sb.AppendLine($"{indent}}});");
        return sb.ToString().TrimEnd();
    }

    private string TranslateRunRepeating(string stmt, int indentLevel)
    {
        var tickMatch = Regex.Match(stmt, @"_rrtick\[0\]\s*>=\s*(\d+)");
        string ticks = tickMatch.Success ? tickMatch.Groups[1].Value : "6000";
        string body = ExtractInnerLambdaBody(stmt, "_rrserv");
        string indent = new string(' ', indentLevel * 4);
        var sb = new StringBuilder();
        sb.AppendLine($"McScheduler.RunRepeating(server, {ticks}, (s, cancel) =>");
        sb.AppendLine($"{indent}{{");
        foreach (var line in TranslateBody(body, indentLevel + 1))
            sb.AppendLine(line);
        sb.AppendLine($"{indent}}});");
        return sb.ToString().TrimEnd();
    }

    private string TranslateIf(string stmt, int indentLevel)
    {
        string indent = new string(' ', indentLevel * 4);
        // Simple translation — just map condition and keep structure
        var condMatch = Regex.Match(stmt, @"^if\s*\((.+?)\)\s*\{(.+)\}", RegexOptions.Singleline);
        if (!condMatch.Success) return stmt;

        string cond = TranslateCondition(condMatch.Groups[1].Value.Trim());
        string body = condMatch.Groups[2].Value.Trim().TrimEnd('}');

        var sb = new StringBuilder();
        sb.AppendLine($"if ({cond})");
        sb.AppendLine($"{indent}{{");
        foreach (var line in TranslateBody(body, indentLevel + 1))
            sb.AppendLine(line);
        sb.AppendLine($"{indent}}}");
        return sb.ToString().TrimEnd();
    }

    private string? TryTranslateLocalDecl(string stmt)
    {
        // var x = expr; or Type x = expr;
        var rx = new Regex(@"^(?:var|(\w+))\s+(\w+)\s*=\s*(.+?);$", RegexOptions.Singleline);
        var m = rx.Match(stmt);
        if (!m.Success) return null;

        string javaType = m.Groups[1].Value;
        string varName  = m.Groups[2].Value;
        string expr     = m.Groups[3].Value.Trim();

        // Skip known preamble variables
        if (IsKnownPreambleVar(varName, javaType)) return null;

        string csType = string.IsNullOrEmpty(javaType) ? "var" : ReverseTypeMapper.Map(javaType);
        string csExpr = ReverseMethodMapper.Translate(expr);
        csExpr = ReverseMethodMapper.UnwrapTextLiteral(csExpr);

        return $"{csType} {varName} = {csExpr};";
        NNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNN
    }

    // ── Helper utilities ──────────────────────────────────────────────────────

    private static string ExtractLambdaBody(string stmt)
    {
        // Find the deepest lambda body: the last register( ) call's lambda content
        // Strategy: find -> { and extract until matching }
        int arrowIdx = stmt.LastIndexOf("-> {");
        if (arrowIdx < 0) arrowIdx = stmt.IndexOf("-> {");
        if (arrowIdx < 0) return stmt;

        int start = arrowIdx + 4;
        return ExtractBraceBlock(stmt, start);
    }

    private static string ExtractInnerLambdaBody(string stmt, string lambdaVar)
    {
        // For RunLater/_rrserv patterns, find the inner body
        int idx = stmt.IndexOf($"MinecraftServer {lambdaVar.Replace("_rl", "").Replace("_rr", "")}");
        if (idx < 0) idx = stmt.IndexOf($"MinecraftServer");
        if (idx < 0) return stmt;
        // Take everything after the var declaration to the next matching }
        int semiIdx = stmt.IndexOf(';', idx);
        if (semiIdx < 0) return "";
        string rest = stmt[(semiIdx + 1)..];
        return rest.TrimEnd('}', ';', ' ');
    }

    private static string ExtractCommandBody(string stmt)
    {
        // Find 'executes(ctx -> {' and extract the block by brace depth
        int idx = stmt.IndexOf("executes(ctx");
        if (idx < 0) return "";
        int braceStart = stmt.IndexOf('{', idx);
        if (braceStart < 0) return "";
        return ExtractBraceBlock(stmt, braceStart + 1);
    }

    private static string StripEventPreamble(string body, string[] preambleVars)
    {
        // Remove lines that declare known preamble variables
        var lines = body.Split('\n');
        var result = new List<string>();
        foreach (var line in lines)
        {
            bool skip = false;
            // Check if this line is just setting up a preamble variable
            foreach (var pv in preambleVars)
            {
                if (Regex.IsMatch(line.Trim(), $@"^\w+\s+{Regex.Escape(pv)}\s*=") ||
                    Regex.IsMatch(line.Trim(), $@"^MinecraftServer\s+\w+\s*=") ||
                    line.Trim().StartsWith("if (!(player instanceof") ||
                    Regex.IsMatch(line.Trim(), $@"^ServerPlayerEntity\s+{Regex.Escape(pv)}\s*="))
                {
                    skip = true; break;
                }
            }
            if (!skip) result.Add(line);
        }
        return string.Join('\n', result);
    }

    private static string StripCommandPreamble(string body, string argVar, string argType)
    {
        // Body may be a single long line — split into individual statements first
        var stmts = SplitStatements(body);
        var result = new List<string>();
        foreach (var stmt in stmts)
        {
            string t = stmt.Trim();
            if (string.IsNullOrWhiteSpace(t)) continue;
            // Strip standard preamble statements
            if (t.StartsWith("ServerCommandSource") && t.Contains("ctx.getSource()")) continue;
            if (t.StartsWith("ServerPlayerEntity") && (t.Contains("src.getPlayer()") || t.Contains("getPlayer()"))) continue;
            if (t.StartsWith("MinecraftServer") && t.Contains("getServer()")) continue;
            if (Regex.IsMatch(t, @"^if\s*\(\s*\w+\s*==\s*null\s*\)")) continue;
            // Strip arg extraction lines (will be parameters instead)
            if (!string.IsNullOrEmpty(argVar))
            {
                if (t.Contains("IntegerArgumentType.getInteger") && t.Contains(argVar)) continue;
                if (t.Contains("StringArgumentType.getString") && t.Contains(argVar)) continue;
                if (t.Contains("EntityArgumentType.getPlayer") && t.Contains(argVar)) continue;
            }
            result.Add(t);
        }
        // Re-join with newlines so TranslateBody can process it
        return string.Join('\n', result);
    }

    private static string TranslateCondition(string cond)
    {
        // Map common condition patterns
        cond = Regex.Replace(cond, @"(\w+)\.getName\(\)\.getString\(\)", "$1.Name");
        cond = Regex.Replace(cond, @"(\w+)\.getHealth\(\)", "$1.Health");
        cond = Regex.Replace(cond, @"(\w+)\.getPlayerManager\(\)\.getCurrentPlayerCount\(\)", "$1.PlayerCount");
        cond = Regex.Replace(cond, @"(\w+)\.isAlive\(\)", "$1.IsAlive");
        cond = Regex.Replace(cond, @"(\w+)\.isEmpty\(\)", "$1.IsEmpty");
        cond = Regex.Replace(cond, @"\s*==\s*1\b", " == 1");
        cond = Regex.Replace(cond, @"(\w+) instanceof ServerPlayerEntity", "$1 is McPlayer");
        return cond;
    }

    private static string ExtractSrcVar(string stmt)
    {
        var m = Regex.Match(stmt, @"ServerCommandSource\s+(\w+)\s*=\s*ctx\.getSource");
        return m.Success ? m.Groups[1].Value : "src";
    }

    private static string ExtractTargetVar(string stmt)
    {
        var m = Regex.Match(stmt, @"EntityArgumentType\.getPlayer\(ctx,\s*\""(\w+)\""\)");
        return m.Success ? m.Groups[1].Value : "target";
    }

    private static string IntArgVarName(string stmt)
    {
        var m = Regex.Match(stmt, @"IntegerArgumentType\.getInteger\(ctx,\s*\""(\w+)\""\)");
        return m.Success ? m.Groups[1].Value : "value";
    }

    private static string StrArgVarName(string stmt)
    {
        var m = Regex.Match(stmt, @"StringArgumentType\.getString\(ctx,\s*\""(\w+)\""\)");
        return m.Success ? m.Groups[1].Value : "value";
    }

    private static bool ShouldSkip(string stmt)
    {
        if (string.IsNullOrWhiteSpace(stmt)) return true;
        // Skip trailing Java close-paren artifacts from command/event registrations
        if (Regex.IsMatch(stmt, @"^[\)\];,;]+$")) return true;
        // Skip pure Java boilerplate
        if (stmt == "return 1;") return true;
        if (stmt == "return;") return true;
        if (stmt.StartsWith("package ")) return true;
        if (stmt.StartsWith("import ")) return true;
        return false;
    }

    private static bool IsJavaBoilerplate(string stmt)
    {
        return stmt.Contains("Registries.") && !stmt.Contains("McRegistry") ||
               stmt.Contains("ctx.getSource()") ||
               stmt.Contains("instanceof ServerPlayerEntity") ||
               stmt.Contains("Text.literal") ||
               stmt.Contains("CommandManager.literal");
    }

    private static bool IsKnownPreambleVar(string varName, string javaType)
    {
        if (javaType is "ServerCommandSource" or "MinecraftServer") return true;
        if (varName is "dispatcher" or "registryAccess" or "environment") return true;
        return false;
    }

    private static List<string> SplitStatements(string body)
    {
        // Split on semicolons and braces at depth 0
        var statements = new List<string>();
        var current = new StringBuilder();
        int depth = 0;
        bool inString = false;

        for (int i = 0; i < body.Length; i++)
        {
            char c = body[i];
            if (c == '"' && (i == 0 || body[i - 1] != '\\')) inString = !inString;
            if (!inString)
            {
                if (c == '{') depth++;
                else if (c == '}')
                {
                    depth--;
                    if (depth < 0) depth = 0;
                    if (depth == 0)
                    {
                        current.Append(c);
                        statements.Add(current.ToString().Trim());
                        current.Clear();
                        continue;
                    }
                }
                else if (c == ';' && depth == 0)
                {
                    current.Append(c);
                    statements.Add(current.ToString().Trim());
                    current.Clear();
                    continue;
                }
            }
            current.Append(c);
        }
        if (current.Length > 0)
            statements.Add(current.ToString().Trim());
        return statements;
    }

    private static string EscapeComment(string s) =>
        s.Replace("*/", "* /").Replace("\n", " ").Trim();

    private static string ToPascalCase(string name)
    {
        if (string.IsNullOrEmpty(name)) return name;
        return char.ToUpper(name[0]) + name[1..];
    }
}
