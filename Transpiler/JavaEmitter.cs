using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Transpiler;

/// <summary>
/// Walks a C# syntax tree and emits Java source via JavaWriter.
/// Add Visit* overrides here as you need more syntax support.
/// </summary>
public class JavaEmitter : CSharpSyntaxWalker
{
    private readonly JavaWriter        _w;
    private readonly ImportTracker     _imports;
    private readonly DiagnosticReporter _diag;

    // Track the C# type name of the current receiver so property/method lookups work.
    // e.g. when we see  player.SendMessage(...)  we need to know "player" is McPlayer.
    private readonly Dictionary<string, string> _localTypes = new();

    public JavaEmitter(JavaWriter writer, ImportTracker imports, DiagnosticReporter diag)
    {
        _w       = writer;
        _imports = imports;
        _diag    = diag;
    }

    // ── Class ─────────────────────────────────────────────────────────────────

    public override void VisitClassDeclaration(ClassDeclarationSyntax node)
    {
        // Modifiers
        var mods = node.Modifiers
            .Select(m => ModifierMapper.MapModifier(m.Text))
            .Where(m => !string.IsNullOrWhiteSpace(m))
            .ToList();

        // Base types / interfaces
        string baseClause = "";
        if (node.BaseList is { } bl)
        {
            var bases = bl.Types.Select(t => MapTypeName(t.Type.ToString())).ToList();
            // IMod → implements ModInitializer
            var interfaces = bases.Where(b => b.StartsWith("I") || b == "ModInitializer").ToList();
            var superclass  = bases.Except(interfaces).FirstOrDefault();

            if (superclass != null)
                baseClause += $" extends {superclass}";
            if (interfaces.Count > 0)
                baseClause += $" implements {string.Join(", ", interfaces)}";
        }

        string modStr = mods.Count > 0 ? string.Join(" ", mods) + " " : "";
        _w.Line($"{modStr}class {node.Identifier.Text}{baseClause} {{");
        _w.Blank();

        // Logger field — emit automatically for mod classes
        _w.Line($"    public static final Logger LOGGER = LoggerFactory.getLogger(\"{node.Identifier.Text}\");");
        _imports.Add("org.slf4j.Logger");
        _imports.Add("org.slf4j.LoggerFactory");
        _w.Blank();

        foreach (var member in node.Members)
            Visit(member);

        _w.Line("}");
    }

    // ── Method ────────────────────────────────────────────────────────────────

    public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
        bool isOverride = node.Modifiers.Any(m => m.Text == "override");

        var mods = node.Modifiers
            .Select(m => ModifierMapper.MapModifier(m.Text))
            .Where(m => !string.IsNullOrWhiteSpace(m) && m != "@Override")
            .ToList();

        string retType = MapTypeName(node.ReturnType.ToString());
        string name    = ToCamelCase(node.Identifier.Text);

        string paramList = string.Join(", ", node.ParameterList.Parameters.Select(p =>
        {
            string pType = MapTypeName(p.Type?.ToString() ?? "Object");
            string pName = p.Identifier.Text;
            _localTypes[pName] = p.Type?.ToString() ?? "";
            return $"{pType} {pName}";
        }));

        if (isOverride) _w.Line("@Override");

        string modStr = mods.Count > 0 ? string.Join(" ", mods) + " " : "public ";
        _w.Line($"{modStr}{retType} {name}({paramList}) {{");

        if (node.Body != null)
            foreach (var stmt in node.Body.Statements)
                Visit(stmt);
        else if (node.ExpressionBody != null)
        {
            // expression-bodied method: void Foo() => expr;
            string expr = EmitExpression(node.ExpressionBody.Expression);
            _w.Line($"    {expr};");
        }

        _w.Line("}");
        _w.Blank();
    }

    // ── Statements ────────────────────────────────────────────────────────────

    public override void VisitExpressionStatement(ExpressionStatementSyntax node)
    {
        // Check for event subscription: Events.X += handler
        if (node.Expression is AssignmentExpressionSyntax assign &&
            assign.IsKind(SyntaxKind.AddAssignmentExpression) &&
            assign.Left is MemberAccessExpressionSyntax mae &&
            mae.Expression.ToString() == "Events")
        {
            EmitEventSubscription(mae.Name.Identifier.Text, assign.Right);
            return;
        }

        string expr = EmitExpression(node.Expression);
        _w.Line($"    {expr};");
    }

    public override void VisitLocalDeclarationStatement(LocalDeclarationStatementSyntax node)
    {
        foreach (var v in node.Declaration.Variables)
        {
            string csType  = node.Declaration.Type.ToString();
            string javaType = csType == "var" ? "var" : MapTypeName(csType);
            string name    = v.Identifier.Text;

            _localTypes[name] = csType;

            string line = v.Initializer != null
                ? $"    {javaType} {name} = {EmitExpression(v.Initializer.Value)};"
                : $"    {javaType} {name};";

            _w.Line(line);
        }
    }

    public override void VisitReturnStatement(ReturnStatementSyntax node)
    {
        if (node.Expression is null)
            _w.Line("    return;");
        else
            _w.Line($"    return {EmitExpression(node.Expression)};");
    }

    public override void VisitIfStatement(IfStatementSyntax node)
    {
        string cond = EmitExpression(node.Condition);
        _w.Line($"    if ({cond}) {{");
        Visit(node.Statement);
        if (node.Else != null)
        {
            _w.Line("    } else {");
            Visit(node.Else.Statement);
        }
        _w.Line("    }");
    }

    public override void VisitForEachStatement(ForEachStatementSyntax node)
    {
        string javaType = MapTypeName(node.Type.ToString());
        string varName  = node.Identifier.Text;
        string expr     = EmitExpression(node.Expression);
        _w.Line($"    for ({javaType} {varName} : {expr}) {{");
        Visit(node.Statement);
        _w.Line("    }");
    }

    public override void VisitBlock(BlockSyntax node)
    {
        foreach (var stmt in node.Statements)
            Visit(stmt);
    }

    // ── Expression emitter ────────────────────────────────────────────────────
    // Returns a Java expression string without a trailing semicolon.

    private string EmitExpression(ExpressionSyntax expr) => expr switch
    {
        LiteralExpressionSyntax lit             => EmitLiteral(lit),
        IdentifierNameSyntax id                 => EmitIdentifier(id),
        MemberAccessExpressionSyntax mae        => EmitMemberAccess(mae),
        InvocationExpressionSyntax inv          => EmitInvocation(inv),
        ObjectCreationExpressionSyntax oc       => EmitObjectCreation(oc),
        AssignmentExpressionSyntax asgn         => EmitAssignment(asgn),
        BinaryExpressionSyntax bin              => EmitBinary(bin),
        PrefixUnaryExpressionSyntax pre         => EmitPrefixUnary(pre),
        PostfixUnaryExpressionSyntax post       => EmitPostfixUnary(post),
        InterpolatedStringExpressionSyntax istr => EmitInterpolatedString(istr),
        ParenthesizedExpressionSyntax paren     => $"({EmitExpression(paren.Expression)})",
        CastExpressionSyntax cast               => $"({MapTypeName(cast.Type.ToString())}){EmitExpression(cast.Expression)}",
        ConditionalExpressionSyntax cond        => $"{EmitExpression(cond.Condition)} ? {EmitExpression(cond.WhenTrue)} : {EmitExpression(cond.WhenFalse)}",
        IsPatternExpressionSyntax isp           => EmitIsPattern(isp),
        LambdaExpressionSyntax lam              => EmitLambda(lam),
        ThrowExpressionSyntax thr               => $"throw {EmitExpression(thr.Expression)}",
        _ => UnknownExpr(expr),
    };

    private string EmitLiteral(LiteralExpressionSyntax lit) => lit.Kind() switch
    {
        SyntaxKind.StringLiteralExpression  => lit.Token.Text,
        SyntaxKind.CharacterLiteralExpression => lit.Token.Text,
        SyntaxKind.TrueLiteralExpression    => "true",
        SyntaxKind.FalseLiteralExpression   => "false",
        SyntaxKind.NullLiteralExpression    => "null",
        _                                   => lit.Token.Text,
    };

    private string EmitIdentifier(IdentifierNameSyntax id)
    {
        // Map known C# type names used as expressions (e.g. "Math", "Console")
        string mapped = TypeMapper.Map(id.Identifier.Text);
        return mapped == id.Identifier.Text ? id.Identifier.Text : mapped;
    }

    private string EmitMemberAccess(MemberAccessExpressionSyntax mae)
    {
        string target   = EmitExpression(mae.Expression);
        string member   = mae.Name.Identifier.Text;
        string csType   = ResolveType(mae.Expression);

        // Property mapping
        string? prop = MethodMapper.GetProperty(csType, member);
        if (prop != null)
        {
            _imports.AddForCsType(csType);
            return MethodMapper.Apply(prop, target);
        }

        // Static methods on known statics (e.g. Math.Abs → used as expression chain)
        string fullName = $"{mae.Expression}.{member}";
        string? staticM = MethodMapper.GetStatic(fullName);
        if (staticM != null)
            return staticM; // args filled in by InvocationExpression visitor

        return $"{target}.{ToCamelCase(member)}";
    }

    private string EmitInvocation(InvocationExpressionSyntax inv)
    {
        var args = inv.ArgumentList.Arguments.Select(a => EmitExpression(a.Expression)).ToArray();

        // Static method: Console.WriteLine, Math.Abs, etc.
        string fullName = inv.Expression.ToString();
        string? staticM = MethodMapper.GetStatic(fullName);
        if (staticM != null)
            return FillArgs(staticM, null, args);

        // Member call: target.Method(...)
        if (inv.Expression is MemberAccessExpressionSyntax mae)
        {
            string target  = EmitExpression(mae.Expression);
            string method  = mae.Name.Identifier.Text;
            string csType  = ResolveType(mae.Expression);

            var mapping = MethodMapper.GetMethod(csType, method);
            if (mapping != null)
            {
                _imports.AddFromMethod(mapping);
                _imports.AddForCsType(csType);
                return FillArgs(mapping.Template, target, args);
            }

            // Unknown method — emit camelCase version with a warning
            _diag.Warn(inv, $"Unknown method {csType}.{method} — emitting as-is");
            string argStr = string.Join(", ", args);
            return $"{target}.{ToCamelCase(method)}({argStr})";
        }

        // Simple call: FooBar(...)
        string argList = string.Join(", ", args);
        return $"{ToCamelCase(inv.Expression.ToString())}({argList})";
    }

    private string EmitObjectCreation(ObjectCreationExpressionSyntax oc)
    {
        string csType = oc.Type.ToString();
        var args = oc.ArgumentList?.Arguments.Select(a => EmitExpression(a.Expression)).ToArray()
                   ?? Array.Empty<string>();

        string? ctor = MethodMapper.GetConstructor(csType);
        if (ctor != null)
        {
            _imports.AddForCsType(csType);
            return FillArgs(ctor, null, args);
        }

        // Unknown — emit new JavaType(args)
        string javaType = MapTypeName(csType);
        _imports.AddForCsType(csType);
        return $"new {javaType}({string.Join(", ", args)})";
    }

    private string EmitAssignment(AssignmentExpressionSyntax asgn)
    {
        string left  = EmitExpression(asgn.Left);
        string right = EmitExpression(asgn.Right);
        string op    = asgn.Kind() switch
        {
            SyntaxKind.AddAssignmentExpression      => "+=",
            SyntaxKind.SubtractAssignmentExpression => "-=",
            SyntaxKind.MultiplyAssignmentExpression => "*=",
            SyntaxKind.DivideAssignmentExpression   => "/=",
            _                                       => "=",
        };
        return $"{left} {op} {right}";
    }

    private string EmitBinary(BinaryExpressionSyntax bin)
    {
        string left  = EmitExpression(bin.Left);
        string right = EmitExpression(bin.Right);
        string op    = bin.Kind() switch
        {
            SyntaxKind.IsExpression              => "instanceof",
            SyntaxKind.AsExpression              => "instanceof", // emitter only; cast separately
            SyntaxKind.CoalesceExpression        => "!=",        // x ?? y → simplified
            SyntaxKind.AddExpression             => "+",
            SyntaxKind.SubtractExpression        => "-",
            SyntaxKind.MultiplyExpression        => "*",
            SyntaxKind.DivideExpression          => "/",
            SyntaxKind.ModuloExpression          => "%",
            SyntaxKind.EqualsExpression          => "==",
            SyntaxKind.NotEqualsExpression       => "!=",
            SyntaxKind.LessThanExpression        => "<",
            SyntaxKind.LessThanOrEqualExpression => "<=",
            SyntaxKind.GreaterThanExpression     => ">",
            SyntaxKind.GreaterThanOrEqualExpression => ">=",
            SyntaxKind.LogicalAndExpression      => "&&",
            SyntaxKind.LogicalOrExpression       => "||",
            SyntaxKind.BitwiseAndExpression      => "&",
            SyntaxKind.BitwiseOrExpression       => "|",
            SyntaxKind.ExclusiveOrExpression     => "^",
            SyntaxKind.LeftShiftExpression       => "<<",
            SyntaxKind.RightShiftExpression      => ">>",
            _ => bin.OperatorToken.Text,
        };

        if (bin.IsKind(SyntaxKind.IsExpression))
            return $"{left} instanceof {MapTypeName(right)}";

        if (bin.IsKind(SyntaxKind.CoalesceExpression))
            return $"({left} != null ? {left} : {right})";

        return $"{left} {op} {right}";
    }

    private string EmitPrefixUnary(PrefixUnaryExpressionSyntax pre)
    {
        string op   = pre.OperatorToken.Text;
        string expr = EmitExpression(pre.Operand);
        return $"{op}{expr}";
    }

    private string EmitPostfixUnary(PostfixUnaryExpressionSyntax post)
    {
        string expr = EmitExpression(post.Operand);
        string op   = post.OperatorToken.Text;
        return $"{expr}{op}";
    }

    private string EmitInterpolatedString(InterpolatedStringExpressionSyntax istr)
    {
        // $"Hello {name}!" → String.format("Hello %s!", name)
        var formatParts = new System.Text.StringBuilder();
        var args        = new List<string>();

        foreach (var content in istr.Contents)
        {
            if (content is InterpolatedStringTextSyntax text)
            {
                // Escape % signs in literal text
                formatParts.Append(text.TextToken.Text.Replace("%", "%%"));
            }
            else if (content is InterpolationSyntax hole)
            {
                string argExpr = EmitExpression(hole.Expression);
                args.Add(argExpr);
                formatParts.Append("%s");
            }
        }

        if (args.Count == 0)
            return $"\"{formatParts}\"";

        string argList = string.Join(", ", args);
        return $"String.format(\"{formatParts}\", {argList})";
    }

    private string EmitIsPattern(IsPatternExpressionSyntax isp)
    {
        string expr = EmitExpression(isp.Expression);
        if (isp.Pattern is DeclarationPatternSyntax decl)
        {
            string javaType = MapTypeName(decl.Type.ToString());
            string varName  = decl.Designation is SingleVariableDesignationSyntax sv
                ? sv.Identifier.Text : "_";
            return $"{expr} instanceof {javaType} {varName}";
        }
        if (isp.Pattern is ConstantPatternSyntax cp)
        {
            string val = EmitExpression(cp.Expression);
            return val == "null" ? $"{expr} == null" : $"{expr} == {val}";
        }
        return $"{expr} instanceof Object /* pattern not supported */";
    }

    private string EmitLambda(LambdaExpressionSyntax lam)
    {
        string paramList = lam switch
        {
            SimpleLambdaExpressionSyntax sl   => sl.Parameter.Identifier.Text,
            ParenthesizedLambdaExpressionSyntax pl =>
                string.Join(", ", pl.ParameterList.Parameters.Select(p => p.Identifier.Text)),
            _ => "_",
        };

        if (lam.ExpressionBody != null)
            return $"({paramList}) -> {EmitExpression(lam.ExpressionBody)}";

        // Block-bodied lambda — emit as multi-line (best effort, caller handles indentation)
        var sb = new System.Text.StringBuilder();
        sb.Append($"({paramList}) -> {{");
        if (lam.Block != null)
            foreach (var stmt in lam.Block.Statements)
                sb.Append($" {EmitStatementInline(stmt)}");
        sb.Append(" }");
        return sb.ToString();
    }

    // ── Event subscription ────────────────────────────────────────────────────

    private void EmitEventSubscription(string csEventName, ExpressionSyntax handler)
    {
        var mapping = EventMapper.Get(csEventName);
        if (mapping == null)
        {
            _diag.Warn($"Unknown event: Events.{csEventName} — skipped");
            return;
        }

        _imports.AddForEvent(mapping);
        _imports.Add("net.minecraft.server.network.ServerPlayerEntity");

        // Extract lambda param names and body
        string[] paramNames;
        BlockSyntax? body = null;

        switch (handler)
        {
            case SimpleLambdaExpressionSyntax sl:
                paramNames = [sl.Parameter.Identifier.Text];
                body = sl.Block;
                break;
            case ParenthesizedLambdaExpressionSyntax pl:
                paramNames = pl.ParameterList.Parameters
                    .Select(p => p.Identifier.Text).ToArray();
                body = pl.Block;
                break;
            default:
                paramNames = [];
                break;
        }

        // Register lambda param types for body resolution
        if (paramNames.Length > 0)
            _localTypes[paramNames[0]] = "McPlayer";
        if (paramNames.Length > 1)
            _localTypes[paramNames[1]] = "BlockPos";

        // Build preamble with actual param names
        string preamble = mapping.Preamble;
        for (int i = 0; i < paramNames.Length; i++)
            preamble = preamble.Replace($"{{{i}}}", paramNames[i]);

        _w.Line($"    {mapping.FabricClass}.{mapping.FabricEvent}.register({mapping.JavaArgs} -> {{");

        if (!string.IsNullOrWhiteSpace(preamble))
            _w.Line($"        {preamble}");

        if (body != null)
            foreach (var stmt in body.Statements)
                EmitStatementIndented(stmt, "        ");

        _w.Line("    });");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private string ResolveType(ExpressionSyntax expr)
    {
        if (expr is IdentifierNameSyntax id && _localTypes.TryGetValue(id.Identifier.Text, out var t))
            return t;
        return expr.ToString();
    }

    private string MapTypeName(string csType) => TypeMapper.Map(csType);

    private static string ToCamelCase(string name)
    {
        if (string.IsNullOrEmpty(name)) return name;
        // If already camelCase (first char lowercase) leave it
        if (char.IsLower(name[0])) return name;
        return char.ToLowerInvariant(name[0]) + name[1..];
    }

    private static string FillArgs(string template, string? target, string[] args)
    {
        string result = template;
        if (target != null)
            result = result.Replace("{target}", target);
        for (int i = 0; i < args.Length; i++)
            result = result.Replace($"{{{i}}}", args[i]);
        return result;
    }

    private string UnknownExpr(ExpressionSyntax expr)
    {
        _diag.Warn(expr, $"Unsupported expression kind: {expr.Kind()} — emitting raw");
        return expr.ToString();
    }

    /// <summary>Emits a statement as a single inline string (for lambdas).</summary>
    private string EmitStatementInline(StatementSyntax stmt) => stmt switch
    {
        ExpressionStatementSyntax es => EmitExpression(es.Expression) + ";",
        ReturnStatementSyntax rs     => rs.Expression != null
            ? $"return {EmitExpression(rs.Expression)};"
            : "return;",
        _ => stmt.ToString().Trim(),
    };

    /// <summary>Emits a statement with explicit indentation (used inside event lambda bodies).</summary>
    private void EmitStatementIndented(StatementSyntax stmt, string indent)
    {
        switch (stmt)
        {
            case ExpressionStatementSyntax es:
                if (es.Expression is AssignmentExpressionSyntax asgn &&
                    asgn.IsKind(SyntaxKind.AddAssignmentExpression) &&
                    asgn.Left is MemberAccessExpressionSyntax mae &&
                    mae.Expression.ToString() == "Events")
                {
                    // nested event subscription inside another handler
                    EmitEventSubscription(mae.Name.Identifier.Text, asgn.Right);
                    return;
                }
                _w.Line($"{indent}{EmitExpression(es.Expression)};");
                break;
            case ReturnStatementSyntax rs:
                _w.Line(rs.Expression != null
                    ? $"{indent}return {EmitExpression(rs.Expression)};"
                    : $"{indent}return;");
                break;
            case LocalDeclarationStatementSyntax ld:
                foreach (var v in ld.Declaration.Variables)
                {
                    string csType  = ld.Declaration.Type.ToString();
                    string javaType = csType == "var" ? "var" : MapTypeName(csType);
                    _localTypes[v.Identifier.Text] = csType;
                    string init = v.Initializer != null
                        ? $" = {EmitExpression(v.Initializer.Value)}" : "";
                    _w.Line($"{indent}{javaType} {v.Identifier.Text}{init};");
                }
                break;
            case IfStatementSyntax ifs:
                _w.Line($"{indent}if ({EmitExpression(ifs.Condition)}) {{");
                if (ifs.Statement is BlockSyntax blk)
                    foreach (var s in blk.Statements) EmitStatementIndented(s, indent + "    ");
                else EmitStatementIndented(ifs.Statement, indent + "    ");
                if (ifs.Else != null)
                {
                    _w.Line($"{indent}}} else {{");
                    EmitStatementIndented(ifs.Else.Statement, indent + "    ");
                }
                _w.Line($"{indent}}}");
                break;
            case ForEachStatementSyntax fe:
                _w.Line($"{indent}for ({MapTypeName(fe.Type.ToString())} {fe.Identifier.Text} : {EmitExpression(fe.Expression)}) {{");
                if (fe.Statement is BlockSyntax feb)
                    foreach (var s in feb.Statements) EmitStatementIndented(s, indent + "    ");
                _w.Line($"{indent}}}");
                break;
            case BlockSyntax blk2:
                foreach (var s in blk2.Statements) EmitStatementIndented(s, indent);
                break;
            default:
                _w.Line($"{indent}{stmt.ToString().Trim()}");
                break;
        }
    }
}
