using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Transpiler;

/// <summary>
/// Walks a C# syntax tree and emits Java source via JavaWriter.
/// </summary>
public class JavaEmitter : CSharpSyntaxWalker
{
    private readonly JavaWriter         _w;
    private readonly ImportTracker      _imports;
    private readonly DiagnosticReporter _diag;

    // Track the C# type name of the current receiver so property/method lookups work.
    private readonly Dictionary<string, string> _localTypes = new();

    // Indentation state — updated as we enter/leave class and method bodies.
    // _memberIndent : indentation for class members (methods, fields)
    // _stmtIndent   : indentation for statements inside the current method/lambda
    private string _memberIndent = "";
    private string _stmtIndent   = "    ";

    public JavaEmitter(JavaWriter writer, ImportTracker imports, DiagnosticReporter diag)
    {
        _w       = writer;
        _imports = imports;
        _diag    = diag;
    }

    // ── Class ─────────────────────────────────────────────────────────────────

    public override void VisitClassDeclaration(ClassDeclarationSyntax node)
    {
        var mods = node.Modifiers
            .Select(m => ModifierMapper.MapModifier(m.Text))
            .Where(m => !string.IsNullOrWhiteSpace(m))
            .ToList();

        string baseClause = "";
        if (node.BaseList is { } bl)
        {
            var bases = bl.Types.Select(t => MapTypeName(t.Type.ToString())).ToList();
            var interfaces = bases.Where(b => b == "ModInitializer" ||
                                              (b.StartsWith("I") && b != "Iterable")).ToList();
            var superclass  = bases.Except(interfaces).FirstOrDefault();

            if (superclass != null)
                baseClause += $" extends {superclass}";
            if (interfaces.Count > 0)
                baseClause += $" implements {string.Join(", ", interfaces)}";
        }

        string modStr = mods.Count > 0 ? string.Join(" ", mods) + " " : "";
        _w.Line($"{modStr}class {node.Identifier.Text}{baseClause} {{");
        _w.Blank();

        string prevMember = _memberIndent;
        string prevStmt   = _stmtIndent;
        _memberIndent = "    ";
        _stmtIndent   = "        ";

        _w.Line($"{_memberIndent}public static final Logger LOGGER = LoggerFactory.getLogger(\"{node.Identifier.Text}\");");
        _imports.Add("org.slf4j.Logger");
        _imports.Add("org.slf4j.LoggerFactory");
        _w.Blank();

        foreach (var member in node.Members)
            Visit(member);

        _memberIndent = prevMember;
        _stmtIndent   = prevStmt;

        _w.Line("}");
    }

    // ── Field ─────────────────────────────────────────────────────────────────

    public override void VisitFieldDeclaration(FieldDeclarationSyntax node)
    {
        bool isStatic   = node.Modifiers.Any(m => m.Text == "static");
        bool isReadonly = node.Modifiers.Any(m => m.Text is "readonly" or "const");

        var accessMods = node.Modifiers
            .Select(m => ModifierMapper.MapModifier(m.Text))
            .Where(m => !string.IsNullOrWhiteSpace(m)
                        && m != "static" && m != "final" && m != "@Override")
            .ToList();

        string access = accessMods.Count > 0 ? string.Join(" ", accessMods) : "public";
        string staticPart   = isStatic   ? " static" : "";
        string finalPart    = isReadonly ? " final"  : "";
        string modStr       = $"{access}{staticPart}{finalPart}";

        foreach (var v in node.Declaration.Variables)
        {
            string csType  = node.Declaration.Type.ToString();
            string javaType = MapTypeName(csType);
            string name    = v.Identifier.Text;

            _localTypes[name] = csType;

            if (v.Initializer != null)
            {
                string init = EmitExpression(v.Initializer.Value);
                _w.Line($"{_memberIndent}{modStr} {javaType} {name} = {init};");
            }
            else
            {
                _w.Line($"{_memberIndent}{modStr} {javaType} {name};");
            }
        }
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

        if (isOverride) _w.Line($"{_memberIndent}@Override");

        string modStr = mods.Count > 0 ? string.Join(" ", mods) + " " : "public ";
        _w.Line($"{_memberIndent}{modStr}{retType} {name}({paramList}) {{");

        // Push body indent
        string prevStmt = _stmtIndent;
        _stmtIndent = _memberIndent + "    ";

        if (node.Body != null)
            foreach (var stmt in node.Body.Statements)
                Visit(stmt);
        else if (node.ExpressionBody != null)
        {
            string expr = EmitExpression(node.ExpressionBody.Expression);
            _w.Line($"{_stmtIndent}{expr};");
        }

        _stmtIndent = prevStmt;

        _w.Line($"{_memberIndent}}}");
        _w.Blank();
    }

    // ── Constructor ───────────────────────────────────────────────────────────

    public override void VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
    {
        var mods = node.Modifiers
            .Select(m => ModifierMapper.MapModifier(m.Text))
            .Where(m => !string.IsNullOrWhiteSpace(m))
            .ToList();

        string paramList = string.Join(", ", node.ParameterList.Parameters.Select(p =>
        {
            string pType = MapTypeName(p.Type?.ToString() ?? "Object");
            string pName = p.Identifier.Text;
            _localTypes[pName] = p.Type?.ToString() ?? "";
            return $"{pType} {pName}";
        }));

        string modStr = mods.Count > 0 ? string.Join(" ", mods) + " " : "public ";
        _w.Line($"{_memberIndent}{modStr}{node.Identifier.Text}({paramList}) {{");

        string prevStmt = _stmtIndent;
        _stmtIndent = _memberIndent + "    ";

        if (node.Initializer != null)
        {
            string keyword = node.Initializer.ThisOrBaseKeyword.Text == "base" ? "super" : "this";
            var args = node.Initializer.ArgumentList.Arguments
                .Select(a => EmitExpression(a.Expression));
            _w.Line($"{_stmtIndent}{keyword}({string.Join(", ", args)});");
        }

        if (node.Body != null)
            foreach (var stmt in node.Body.Statements)
                Visit(stmt);

        _stmtIndent = prevStmt;
        _w.Line($"{_memberIndent}}}");
        _w.Blank();
    }

    // ── Statements ────────────────────────────────────────────────────────────

    public override void VisitExpressionStatement(ExpressionStatementSyntax node)
    {
        if (node.Expression is AssignmentExpressionSyntax assign &&
            assign.IsKind(SyntaxKind.AddAssignmentExpression) &&
            assign.Left is MemberAccessExpressionSyntax mae &&
            mae.Expression.ToString() == "Events")
        {
            EmitEventSubscription(mae.Name.Identifier.Text, assign.Right);
            return;
        }

        string expr = EmitExpression(node.Expression);
        _w.Line($"{_stmtIndent}{expr};");
    }

    public override void VisitLocalDeclarationStatement(LocalDeclarationStatementSyntax node)
    {
        foreach (var v in node.Declaration.Variables)
        {
            string csType   = node.Declaration.Type.ToString();
            string javaType = csType == "var" ? "var" : MapTypeName(csType);
            string name     = v.Identifier.Text;
            _localTypes[name] = csType;

            string line = v.Initializer != null
                ? $"{_stmtIndent}{javaType} {name} = {EmitExpression(v.Initializer.Value)};"
                : $"{_stmtIndent}{javaType} {name};";
            _w.Line(line);
        }
    }

    public override void VisitReturnStatement(ReturnStatementSyntax node)
    {
        if (node.Expression is null)
            _w.Line($"{_stmtIndent}return;");
        else
            _w.Line($"{_stmtIndent}return {EmitExpression(node.Expression)};");
    }

    public override void VisitIfStatement(IfStatementSyntax node)
    {
        string cond = EmitExpression(node.Condition);
        _w.Line($"{_stmtIndent}if ({cond}) {{");

        string outer = _stmtIndent;
        _stmtIndent = outer + "    ";
        Visit(node.Statement);
        _stmtIndent = outer;

        if (node.Else != null)
        {
            _w.Line($"{outer}}} else {{");
            _stmtIndent = outer + "    ";
            Visit(node.Else.Statement);
            _stmtIndent = outer;
        }
        _w.Line($"{outer}}}");
    }

    public override void VisitForEachStatement(ForEachStatementSyntax node)
    {
        string javaType = MapTypeName(node.Type.ToString());
        string varName  = node.Identifier.Text;
        string expr     = EmitExpression(node.Expression);
        _w.Line($"{_stmtIndent}for ({javaType} {varName} : {expr}) {{");

        string outer = _stmtIndent;
        _stmtIndent = outer + "    ";
        Visit(node.Statement);
        _stmtIndent = outer;

        _w.Line($"{outer}}}");
    }

    public override void VisitBlock(BlockSyntax node)
    {
        foreach (var stmt in node.Statements)
            Visit(stmt);
    }

    // ── Expression emitter ────────────────────────────────────────────────────

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
        SyntaxKind.StringLiteralExpression    => lit.Token.Text,
        SyntaxKind.CharacterLiteralExpression => lit.Token.Text,
        SyntaxKind.TrueLiteralExpression      => "true",
        SyntaxKind.FalseLiteralExpression     => "false",
        SyntaxKind.NullLiteralExpression      => "null",
        _                                     => lit.Token.Text,
    };

    private string EmitIdentifier(IdentifierNameSyntax id)
    {
        string mapped = TypeMapper.Map(id.Identifier.Text);
        return mapped == id.Identifier.Text ? id.Identifier.Text : mapped;
    }

    private string EmitMemberAccess(MemberAccessExpressionSyntax mae)
    {
        string target = EmitExpression(mae.Expression);
        string member = mae.Name.Identifier.Text;
        string csType = ResolveType(mae.Expression);

        string? prop = MethodMapper.GetProperty(csType, member);
        if (prop != null)
        {
            _imports.AddForCsType(csType);
            return MethodMapper.Apply(prop, target);
        }

        string fullName = $"{mae.Expression}.{member}";
        string? staticM = MethodMapper.GetStatic(fullName);
        if (staticM != null)
            return staticM;

        return $"{target}.{ToCamelCase(member)}";
    }

    private string EmitInvocation(InvocationExpressionSyntax inv)
    {
        var args = inv.ArgumentList.Arguments.Select(a => EmitExpression(a.Expression)).ToArray();

        string fullName = inv.Expression.ToString();
        string? staticM = MethodMapper.GetStatic(fullName);
        if (staticM != null)
            return FillArgs(staticM, null, args);

        if (inv.Expression is MemberAccessExpressionSyntax mae)
        {
            string target = EmitExpression(mae.Expression);
            string method = mae.Name.Identifier.Text;
            string csType = ResolveType(mae.Expression);

            var mapping = MethodMapper.GetMethod(csType, method);
            if (mapping != null)
            {
                _imports.AddFromMethod(mapping);
                _imports.AddForCsType(csType);
                return FillArgs(mapping.Template, target, args);
            }

            _diag.Warn(inv, $"Unknown method {csType}.{method} — emitting as-is");
            string argStr = string.Join(", ", args);
            return $"{target}.{ToCamelCase(method)}({argStr})";
        }

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

        // String equality: == and != with a string literal must use .equals() in Java
        if (bin.IsKind(SyntaxKind.EqualsExpression) || bin.IsKind(SyntaxKind.NotEqualsExpression))
        {
            bool leftIsString  = bin.Left  is LiteralExpressionSyntax ls1 && ls1.IsKind(SyntaxKind.StringLiteralExpression);
            bool rightIsString = bin.Right is LiteralExpressionSyntax ls2 && ls2.IsKind(SyntaxKind.StringLiteralExpression);
            if (leftIsString || rightIsString)
            {
                // Put the non-literal on the left for null safety (literal.equals avoids NPE on literal side)
                string obj = rightIsString ? left : right;
                string lit = rightIsString ? right : left;
                return bin.IsKind(SyntaxKind.EqualsExpression)
                    ? $"{obj}.equals({lit})"
                    : $"!{obj}.equals({lit})";
            }
        }

        string op = bin.Kind() switch
        {
            SyntaxKind.IsExpression                 => "instanceof",
            SyntaxKind.AsExpression                 => "instanceof",
            SyntaxKind.CoalesceExpression           => "!=",
            SyntaxKind.AddExpression                => "+",
            SyntaxKind.SubtractExpression           => "-",
            SyntaxKind.MultiplyExpression           => "*",
            SyntaxKind.DivideExpression             => "/",
            SyntaxKind.ModuloExpression             => "%",
            SyntaxKind.EqualsExpression             => "==",
            SyntaxKind.NotEqualsExpression          => "!=",
            SyntaxKind.LessThanExpression           => "<",
            SyntaxKind.LessThanOrEqualExpression    => "<=",
            SyntaxKind.GreaterThanExpression        => ">",
            SyntaxKind.GreaterThanOrEqualExpression => ">=",
            SyntaxKind.LogicalAndExpression         => "&&",
            SyntaxKind.LogicalOrExpression          => "||",
            SyntaxKind.BitwiseAndExpression         => "&",
            SyntaxKind.BitwiseOrExpression          => "|",
            SyntaxKind.ExclusiveOrExpression        => "^",
            SyntaxKind.LeftShiftExpression          => "<<",
            SyntaxKind.RightShiftExpression         => ">>",
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
        var formatParts = new System.Text.StringBuilder();
        var args        = new List<string>();

        foreach (var content in istr.Contents)
        {
            if (content is InterpolatedStringTextSyntax text)
                formatParts.Append(text.TextToken.Text.Replace("%", "%%"));
            else if (content is InterpolationSyntax hole)
            {
                args.Add(EmitExpression(hole.Expression));
                formatParts.Append("%s");
            }
        }

        if (args.Count == 0)
            return $"\"{formatParts}\"";

        return $"String.format(\"{formatParts}\", {string.Join(", ", args)})";
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
        _imports.Add("net.minecraft.server.MinecraftServer");

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

        // Register lambda param types using CsParamTypes from the event mapping
        for (int i = 0; i < paramNames.Length; i++)
        {
            if (mapping.CsParamTypes != null && i < mapping.CsParamTypes.Length)
                _localTypes[paramNames[i]] = mapping.CsParamTypes[i];
            else if (i == 0)
                _localTypes[paramNames[i]] = "McPlayer";
        }

        // Build preamble with actual param names substituted
        string preamble = mapping.Preamble;
        for (int i = 0; i < paramNames.Length; i++)
            preamble = preamble.Replace($"{{{i}}}", paramNames[i]);

        string innerIndent = _stmtIndent + "    ";

        _w.Line($"{_stmtIndent}{mapping.FabricClass}.{mapping.FabricEvent}.register({mapping.JavaArgs} -> {{");

        if (!string.IsNullOrWhiteSpace(preamble))
        {
            foreach (var stmt in preamble.Split(';').Select(s => s.Trim()).Where(s => s.Length > 0))
            {
                var eqIdx = stmt.IndexOf('=');
                if (eqIdx < 0) { _w.Line($"{innerIndent}{stmt};"); continue; }

                string lhs = stmt[..eqIdx].Trim();
                string rhs = stmt[(eqIdx + 1)..].Trim();
                string declaredName = lhs.Split(' ').Last();

                // Skip pure self-assignments (e.g. "ServerPlayerEntity player = player"
                // when player is already a typed lambda arg of the correct type)
                if (rhs == declaredName) continue;

                _w.Line($"{innerIndent}{stmt};");
            }
        }

        if (body != null)
        {
            string prevStmt = _stmtIndent;
            _stmtIndent = innerIndent;
            foreach (var stmt in body.Statements)
                Visit(stmt);
            _stmtIndent = prevStmt;
        }

        _w.Line($"{_stmtIndent}}});");
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

    private string EmitStatementInline(StatementSyntax stmt) => stmt switch
    {
        ExpressionStatementSyntax es => EmitExpression(es.Expression) + ";",
        ReturnStatementSyntax rs     => rs.Expression != null
            ? $"return {EmitExpression(rs.Expression)};"
            : "return;",
        _ => stmt.ToString().Trim(),
    };
}
