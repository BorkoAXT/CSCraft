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

    // Set to true when player NBT methods are used, so the helper class gets emitted.
    public bool NeedsPlayerDataHelper { get; private set; }

    // Set to true while emitting the body of a command lambda (which returns int in Java).
    // Bare return; statements must become return 1; inside command lambdas.
    private bool _inCommandLambda;

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
            _imports.AddForCsType(csType);

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
            if (csType != "var") _imports.AddForCsType(csType);

            string line = v.Initializer != null
                ? $"{_stmtIndent}{javaType} {name} = {EmitExpression(v.Initializer.Value)};"
                : $"{_stmtIndent}{javaType} {name};";
            _w.Line(line);
        }
    }

    public override void VisitReturnStatement(ReturnStatementSyntax node)
    {
        if (node.Expression is null)
            // Inside command lambdas the Java lambda returns int, so bare return → return 1
            _w.Line($"{_stmtIndent}return {(_inCommandLambda ? "1" : "")};");
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

        string csType = node.Type.ToString();
        bool hadType = _localTypes.TryGetValue(varName, out var prevType);
        _localTypes[varName] = csType;

        string outer = _stmtIndent;
        _stmtIndent = outer + "    ";
        Visit(node.Statement);
        _stmtIndent = outer;

        if (hadType) _localTypes[varName] = prevType!;
        else _localTypes.Remove(varName);

        _w.Line($"{outer}}}");
    }

    public override void VisitBlock(BlockSyntax node)
    {
        foreach (var stmt in node.Statements)
            Visit(stmt);
    }

    // ── Loops / switch / try ──────────────────────────────────────────────────

    public override void VisitWhileStatement(WhileStatementSyntax node)
    {
        string cond = EmitExpression(node.Condition);
        _w.Line($"{_stmtIndent}while ({cond}) {{");
        string outer = _stmtIndent;
        _stmtIndent = outer + "    ";
        Visit(node.Statement);
        _stmtIndent = outer;
        _w.Line($"{outer}}}");
    }

    public override void VisitDoStatement(DoStatementSyntax node)
    {
        _w.Line($"{_stmtIndent}do {{");
        string outer = _stmtIndent;
        _stmtIndent = outer + "    ";
        Visit(node.Statement);
        _stmtIndent = outer;
        string cond = EmitExpression(node.Condition);
        _w.Line($"{outer}}} while ({cond});");
    }

    public override void VisitForStatement(ForStatementSyntax node)
    {
        string init = "";
        if (node.Declaration != null)
        {
            string csType = node.Declaration.Type.ToString();
            string javaType = csType == "var" ? "var" : MapTypeName(csType);
            var vars = node.Declaration.Variables.Select(v =>
            {
                _localTypes[v.Identifier.Text] = csType;
                return v.Initializer != null
                    ? $"{v.Identifier.Text} = {EmitExpression(v.Initializer.Value)}"
                    : v.Identifier.Text;
            });
            init = $"{javaType} {string.Join(", ", vars)}";
        }
        else if (node.Initializers.Count > 0)
            init = string.Join(", ", node.Initializers.Select(i => EmitExpression(i)));

        string cond  = node.Condition != null ? EmitExpression(node.Condition) : "";
        string incrs = string.Join(", ", node.Incrementors.Select(i => EmitExpression(i)));

        _w.Line($"{_stmtIndent}for ({init}; {cond}; {incrs}) {{");
        string outer2 = _stmtIndent;
        _stmtIndent = outer2 + "    ";
        Visit(node.Statement);
        _stmtIndent = outer2;
        _w.Line($"{outer2}}}");
    }

    public override void VisitBreakStatement(BreakStatementSyntax node)
        => _w.Line($"{_stmtIndent}break;");

    public override void VisitContinueStatement(ContinueStatementSyntax node)
        => _w.Line($"{_stmtIndent}continue;");

    public override void VisitThrowStatement(ThrowStatementSyntax node)
    {
        string expr = node.Expression != null ? $" {EmitExpression(node.Expression)}" : "";
        _w.Line($"{_stmtIndent}throw{expr};");
    }

    public override void VisitSwitchStatement(SwitchStatementSyntax node)
    {
        string expr = EmitExpression(node.Expression);
        _w.Line($"{_stmtIndent}switch ({expr}) {{");
        string outer = _stmtIndent;

        foreach (var section in node.Sections)
        {
            foreach (var label in section.Labels)
            {
                if (label is CaseSwitchLabelSyntax caseLabel)
                    _w.Line($"{outer}    case {EmitExpression(caseLabel.Value)}:");
                else if (label is DefaultSwitchLabelSyntax)
                    _w.Line($"{outer}    default:");
            }
            _stmtIndent = outer + "        ";
            foreach (var stmt in section.Statements)
                Visit(stmt);
        }

        _stmtIndent = outer;
        _w.Line($"{outer}}}");
    }

    public override void VisitTryStatement(TryStatementSyntax node)
    {
        _w.Line($"{_stmtIndent}try {{");
        string outer = _stmtIndent;
        _stmtIndent = outer + "    ";
        Visit(node.Block);
        _stmtIndent = outer;

        foreach (var c in node.Catches)
        {
            string param = "Exception _ex";
            if (c.Declaration != null)
            {
                string exType = MapTypeName(c.Declaration.Type.ToString());
                string exVar  = c.Declaration.Identifier.Text;
                if (!string.IsNullOrEmpty(exVar) && exVar != "_")
                {
                    _localTypes[exVar] = c.Declaration.Type.ToString();
                    param = $"{exType} {exVar}";
                }
                else param = exType;
            }
            _w.Line($"{outer}}} catch ({param}) {{");
            _stmtIndent = outer + "    ";
            Visit(c.Block);
            _stmtIndent = outer;
        }

        if (node.Finally != null)
        {
            _w.Line($"{outer}}} finally {{");
            _stmtIndent = outer + "    ";
            Visit(node.Finally.Block);
            _stmtIndent = outer;
        }

        _w.Line($"{outer}}}");
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
        CastExpressionSyntax cast               => $"(({MapTypeName(cast.Type.ToString())}){EmitExpression(cast.Expression)})",
        ConditionalExpressionSyntax cond        => $"{EmitExpression(cond.Condition)} ? {EmitExpression(cond.WhenTrue)} : {EmitExpression(cond.WhenFalse)}",
        IsPatternExpressionSyntax isp           => EmitIsPattern(isp),
        LambdaExpressionSyntax lam              => EmitLambda(lam),
        ThrowExpressionSyntax thr               => $"throw {EmitExpression(thr.Expression)}",
        ArrayCreationExpressionSyntax arr           => EmitArrayCreation(arr),
        ImplicitArrayCreationExpressionSyntax iarr  => EmitImplicitArrayCreation(iarr),
        ElementAccessExpressionSyntax ea            => EmitElementAccess(ea),
        ConditionalAccessExpressionSyntax ca        => EmitConditionalAccess(ca),
        SwitchExpressionSyntax sw                   => EmitSwitchExpression(sw),
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
            string result = MethodMapper.Apply(prop, target);
            AddImportsFromTemplate(result);
            return result;
        }

        string fullName = $"{mae.Expression}.{member}";
        string? staticM = MethodMapper.GetStatic(fullName);
        if (staticM != null)
        {
            AddImportsFromTemplate(staticM);
            return staticM;
        }

        return $"{target}.{ToCamelCase(member)}";
    }

    private string EmitInvocation(InvocationExpressionSyntax inv)
    {
        // Special handling for McCommand.* — lambdas need to be inlined, not invoked
        string fullExpr = inv.Expression.ToString();
        if (fullExpr.StartsWith("McCommand."))
            return EmitCommandRegistration(inv);

        // Special handling for McScheduler.RunLater / RunRepeating
        if (fullExpr is "McScheduler.RunLater" or "McScheduler.RunRepeating")
            return EmitSchedulerCall(inv, fullExpr);

        // Special handling for McRegistry.AddToCreativeTab — variadic items
        if (fullExpr == "McRegistry.AddToCreativeTab")
            return EmitAddToCreativeTab(inv);

        var args = inv.ArgumentList.Arguments.Select(a => EmitExpression(a.Expression)).ToArray();

        string fullName = fullExpr;
        string? staticM = MethodMapper.GetStatic(fullName, args.Length);
        if (staticM != null)
        {
            string filled = FillArgs(staticM, null, args);
            AddImportsFromTemplate(filled);
            return filled;
        }

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
                string filled = FillArgs(mapping.Template, target, args);
                AddImportsFromTemplate(filled);
                return filled;
            }

            _diag.Warn(inv, $"Unknown method {csType}.{method} — emitting as-is");
            string argStr = string.Join(", ", args);
            return $"{target}.{ToCamelCase(method)}({argStr})";
        }

        string argList = string.Join(", ", args);
        return $"{ToCamelCase(inv.Expression.ToString())}({argList})";
    }

    /// <summary>
    /// Special-case: McCommand.Register/RegisterOp/RegisterWithPlayer etc.
    /// Extracts the lambda body and inlines it into the Fabric command registration boilerplate,
    /// so we get proper Java instead of trying to "invoke" a lambda literal.
    /// </summary>
    private string EmitCommandRegistration(InvocationExpressionSyntax inv)
    {
        var mae = (MemberAccessExpressionSyntax)inv.Expression;
        string method = mae.Name.Identifier.Text;
        var rawArgs = inv.ArgumentList.Arguments;

        _imports.Add("net.fabricmc.fabric.api.command.v2.CommandRegistrationCallback");
        _imports.Add("net.minecraft.server.command.CommandManager");
        _imports.Add("net.minecraft.server.command.ServerCommandSource");

        // Extract the command name (always the first arg)
        string cmdName = EmitExpression(rawArgs[0].Expression);

        // Find the lambda argument (always the last one)
        var lambdaArg = rawArgs.Last().Expression as LambdaExpressionSyntax;
        if (lambdaArg == null)
        {
            // Fallback: emit as TODO comment
            return $"/* TODO: McCommand.{method}({string.Join(", ", rawArgs.Select(a => EmitExpression(a.Expression)))}) */";
        }

        // Extract lambda parameter names
        var paramNames = lambdaArg switch
        {
            SimpleLambdaExpressionSyntax sl => new[] { sl.Parameter.Identifier.Text },
            ParenthesizedLambdaExpressionSyntax pl =>
                pl.ParameterList.Parameters.Select(p => p.Identifier.Text).ToArray(),
            _ => new[] { "src" },
        };

        string srcVar = paramNames[0];

        // Register all lambda parameter types BEFORE emitting the body so method resolution works
        _localTypes[srcVar] = "McCommandSource";
        if (paramNames.Length > 1)
        {
            string secondParam = paramNames[1];
            // For WithPlayer variants, second param is McPlayer; for others it's a primitive
            if (method is "RegisterWithPlayer" or "RegisterOpWithPlayer")
                _localTypes[secondParam] = "McPlayer";
            // String/Int/Sub variants: second param is string or int — no special mapping needed
        }

        // Emit the lambda body as inline statements (flag active so return; → return 1;)
        _inCommandLambda = true;
        string body = "";
        if (lambdaArg.Block != null)
            body = string.Join(" ", lambdaArg.Block.Statements.Select(s => EmitStatementInline(s)));
        else if (lambdaArg.ExpressionBody != null)
            body = EmitExpression(lambdaArg.ExpressionBody) + ";";
        _inCommandLambda = false;
        string requires = "";

        switch (method)
        {
            case "Register":
                // McCommand.Register("name", (src) => { body })
                return $"CommandRegistrationCallback.EVENT.register((dispatcher, registryAccess, environment) -> dispatcher.register(CommandManager.literal({cmdName}).executes(ctx -> {{ ServerCommandSource {srcVar} = ctx.getSource(); {body} return 1; }})))";

            case "RegisterOp":
                return $"CommandRegistrationCallback.EVENT.register((dispatcher, registryAccess, environment) -> dispatcher.register(CommandManager.literal({cmdName}).requires(src -> src.hasPermissionLevel(2)).executes(ctx -> {{ ServerCommandSource {srcVar} = ctx.getSource(); {body} return 1; }})))";

            case "RegisterWithPlayer":
            case "RegisterOpWithPlayer":
            {
                // McCommand.RegisterWithPlayer("name", "argName", (src, target) => { body })
                string argName = EmitExpression(rawArgs[1].Expression);
                string targetVar = paramNames.Length > 1 ? paramNames[1] : "target";
                _localTypes[targetVar] = "McPlayer";
                requires = method == "RegisterOpWithPlayer" ? ".requires(src -> src.hasPermissionLevel(2))" : "";
                _imports.Add("net.minecraft.command.argument.EntityArgumentType");
                return $"CommandRegistrationCallback.EVENT.register((dispatcher, registryAccess, environment) -> dispatcher.register(CommandManager.literal({cmdName}){requires}.then(CommandManager.argument({argName}, EntityArgumentType.player()).executes(ctx -> {{ ServerCommandSource {srcVar} = ctx.getSource(); ServerPlayerEntity {targetVar} = EntityArgumentType.getPlayer(ctx, {argName}); {body} return 1; }}))))";
            }

            case "RegisterWithString":
            {
                string argName = EmitExpression(rawArgs[1].Expression);
                string strVar = paramNames.Length > 1 ? paramNames[1] : "value";
                _imports.Add("com.mojang.brigadier.arguments.StringArgumentType");
                return $"CommandRegistrationCallback.EVENT.register((dispatcher, registryAccess, environment) -> dispatcher.register(CommandManager.literal({cmdName}).then(CommandManager.argument({argName}, StringArgumentType.string()).executes(ctx -> {{ ServerCommandSource {srcVar} = ctx.getSource(); String {strVar} = StringArgumentType.getString(ctx, {argName}); {body} return 1; }}))))";
            }

            case "RegisterWithInt":
            {
                string argName = EmitExpression(rawArgs[1].Expression);
                string intVar = paramNames.Length > 1 ? paramNames[1] : "value";
                _imports.Add("com.mojang.brigadier.arguments.IntegerArgumentType");
                return $"CommandRegistrationCallback.EVENT.register((dispatcher, registryAccess, environment) -> dispatcher.register(CommandManager.literal({cmdName}).then(CommandManager.argument({argName}, IntegerArgumentType.integer()).executes(ctx -> {{ ServerCommandSource {srcVar} = ctx.getSource(); int {intVar} = IntegerArgumentType.getInteger(ctx, {argName}); {body} return 1; }}))))";
            }

            case "RegisterSub":
            {
                string subName = EmitExpression(rawArgs[1].Expression);
                // Detect overload: 3 raw args = no-arg sub, 4 = with-string-arg sub, 4+int = with-int-arg
                if (rawArgs.Count == 3)
                {
                    // McCommand.RegisterSub("parent", "sub", (src) => { body }) — no arg
                    return $"CommandRegistrationCallback.EVENT.register((dispatcher, registryAccess, environment) -> dispatcher.register(CommandManager.literal({cmdName}).then(CommandManager.literal({subName}).executes(ctx -> {{ ServerCommandSource {srcVar} = ctx.getSource(); {body} return 1; }}))))";
                }
                // McCommand.RegisterSub("parent", "sub", "argName", (src, val) => { body })
                string argName = EmitExpression(rawArgs[2].Expression);
                string valVar = paramNames.Length > 1 ? paramNames[1] : "value";
                _imports.Add("com.mojang.brigadier.arguments.StringArgumentType");
                return $"CommandRegistrationCallback.EVENT.register((dispatcher, registryAccess, environment) -> dispatcher.register(CommandManager.literal({cmdName}).then(CommandManager.literal({subName}).then(CommandManager.argument({argName}, StringArgumentType.string()).executes(ctx -> {{ ServerCommandSource {srcVar} = ctx.getSource(); String {valVar} = StringArgumentType.getString(ctx, {argName}); {body} return 1; }})))))";
            }

            case "RegisterSubWithInt":
            {
                string subName2 = EmitExpression(rawArgs[1].Expression);
                string argName2 = EmitExpression(rawArgs[2].Expression);
                string intVar2 = paramNames.Length > 1 ? paramNames[1] : "value";
                _imports.Add("com.mojang.brigadier.arguments.IntegerArgumentType");
                return $"CommandRegistrationCallback.EVENT.register((dispatcher, registryAccess, environment) -> dispatcher.register(CommandManager.literal({cmdName}).then(CommandManager.literal({subName2}).then(CommandManager.argument({argName2}, IntegerArgumentType.integer()).executes(ctx -> {{ ServerCommandSource {srcVar} = ctx.getSource(); int {intVar2} = IntegerArgumentType.getInteger(ctx, {argName2}); {body} return 1; }})))))";
            }

            case "RegisterWithFloat":
            {
                string argName = EmitExpression(rawArgs[1].Expression);
                string floatVar = paramNames.Length > 1 ? paramNames[1] : "value";
                _imports.Add("com.mojang.brigadier.arguments.FloatArgumentType");
                return $"CommandRegistrationCallback.EVENT.register((dispatcher, registryAccess, environment) -> dispatcher.register(CommandManager.literal({cmdName}).then(CommandManager.argument({argName}, FloatArgumentType.floatArg()).executes(ctx -> {{ ServerCommandSource {srcVar} = ctx.getSource(); float {floatVar} = FloatArgumentType.getFloat(ctx, {argName}); {body} return 1; }}))))";
            }

            case "RegisterWithDouble":
            {
                string argName = EmitExpression(rawArgs[1].Expression);
                string dblVar = paramNames.Length > 1 ? paramNames[1] : "value";
                _imports.Add("com.mojang.brigadier.arguments.DoubleArgumentType");
                return $"CommandRegistrationCallback.EVENT.register((dispatcher, registryAccess, environment) -> dispatcher.register(CommandManager.literal({cmdName}).then(CommandManager.argument({argName}, DoubleArgumentType.doubleArg()).executes(ctx -> {{ ServerCommandSource {srcVar} = ctx.getSource(); double {dblVar} = DoubleArgumentType.getDouble(ctx, {argName}); {body} return 1; }}))))";
            }

            case "RegisterWithBool":
            {
                string argName = EmitExpression(rawArgs[1].Expression);
                string boolVar = paramNames.Length > 1 ? paramNames[1] : "value";
                _imports.Add("com.mojang.brigadier.arguments.BoolArgumentType");
                return $"CommandRegistrationCallback.EVENT.register((dispatcher, registryAccess, environment) -> dispatcher.register(CommandManager.literal({cmdName}).then(CommandManager.argument({argName}, BoolArgumentType.bool()).executes(ctx -> {{ ServerCommandSource {srcVar} = ctx.getSource(); boolean {boolVar} = BoolArgumentType.getBool(ctx, {argName}); {body} return 1; }}))))";
            }

            default:
                return $"/* TODO: McCommand.{method} — not yet supported */";
        }
    }

    private string EmitSchedulerCall(InvocationExpressionSyntax inv, string method)
    {
        var rawArgs = inv.ArgumentList.Arguments;
        if (rawArgs.Count < 3)
            return $"/* {method}: expected 3 arguments */";

        string ticksArg = EmitExpression(rawArgs[1].Expression);

        var lambdaArg = rawArgs[2].Expression as LambdaExpressionSyntax;
        if (lambdaArg == null)
            return $"/* {method}: third argument must be a lambda */";

        string[] paramNames = lambdaArg switch
        {
            SimpleLambdaExpressionSyntax sl => new[] { sl.Parameter.Identifier.Text },
            ParenthesizedLambdaExpressionSyntax pl =>
                pl.ParameterList.Parameters.Select(p => p.Identifier.Text).ToArray(),
            _ => new[] { "s" },
        };

        string sVar = paramNames.Length > 0 ? paramNames[0] : "s";
        _localTypes[sVar] = "McServer";

        string body = "";
        if (lambdaArg.Block != null)
            body = string.Join(" ", lambdaArg.Block.Statements.Select(s => EmitStatementInline(s)));
        else if (lambdaArg.ExpressionBody != null)
            body = EmitExpression(lambdaArg.ExpressionBody) + ";";

        _imports.Add("net.fabricmc.fabric.api.event.lifecycle.v1.ServerTickEvents");
        _imports.Add("net.minecraft.server.MinecraftServer");

        if (method == "McScheduler.RunLater")
        {
            return $"{{ final int[] _rltick = new int[1]; ServerTickEvents.END_SERVER_TICK.register(_rlserv -> {{ if (++_rltick[0] == {ticksArg}) {{ MinecraftServer {sVar} = _rlserv; {body} }} }}); }}";
        }
        else // RunRepeating
        {
            string cancelVar = paramNames.Length > 1 ? paramNames[1] : "_cancel";
            return $"{{ final int[] _rrtick = new int[1]; final boolean[] _rrcancelled = new boolean[1]; ServerTickEvents.END_SERVER_TICK.register(_rrserv -> {{ if (_rrcancelled[0]) return; if (++_rrtick[0] >= {ticksArg}) {{ _rrtick[0] = 0; MinecraftServer {sVar} = _rrserv; Runnable {cancelVar} = () -> _rrcancelled[0] = true; {body} }} }}); }}";
        }
    }

    private string EmitAddToCreativeTab(InvocationExpressionSyntax inv)
    {
        var rawArgs = inv.ArgumentList.Arguments;
        if (rawArgs.Count < 2)
            return "/* McRegistry.AddToCreativeTab: expected tab id + at least one item */";

        string tabId = EmitExpression(rawArgs[0].Expression);
        var items = rawArgs.Skip(1).Select(a => EmitExpression(a.Expression)).ToList();

        _imports.Add("net.fabricmc.fabric.api.itemgroup.v1.ItemGroupEvents");
        _imports.Add("net.minecraft.item.ItemGroups");
        _imports.Add("net.minecraft.registry.RegistryKey");
        _imports.Add("net.minecraft.registry.RegistryKeys");
        _imports.Add("net.minecraft.util.Identifier");

        string groupExpr = GetItemGroupExpr(tabId);
        string adds = string.Join(" ", items.Select(item => $"e.add({item});"));
        return $"ItemGroupEvents.modifyEntriesEvent({groupExpr}).register(e -> {{ {adds} }})";
    }

    private static string GetItemGroupExpr(string tabIdExpr)
    {
        // tabIdExpr is a Java string literal like "minecraft:combat" — strip surrounding quotes
        string inner = tabIdExpr.Length >= 2 && tabIdExpr[0] == '"' ? tabIdExpr[1..^1] : tabIdExpr;
        return inner switch
        {
            "minecraft:building_blocks"     => "ItemGroups.BUILDING_BLOCKS",
            "minecraft:natural"             => "ItemGroups.NATURAL",
            "minecraft:functional"          => "ItemGroups.FUNCTIONAL",
            "minecraft:redstone"            => "ItemGroups.REDSTONE",
            "minecraft:hotbar"              => "ItemGroups.HOTBAR",
            "minecraft:search"              => "ItemGroups.SEARCH",
            "minecraft:tools_and_utilities" => "ItemGroups.TOOLS",
            "minecraft:tools"               => "ItemGroups.TOOLS",
            "minecraft:combat"              => "ItemGroups.COMBAT",
            "minecraft:food_and_drink"      => "ItemGroups.FOOD_AND_DRINK",
            "minecraft:ingredients"         => "ItemGroups.INGREDIENTS",
            "minecraft:spawn_eggs"          => "ItemGroups.SPAWN_EGGS",
            "minecraft:operator"            => "ItemGroups.OPERATOR",
            _ => $"RegistryKey.of(RegistryKeys.ITEM_GROUP, Identifier.of({tabIdExpr}))",
        };
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
            string filled = FillArgs(ctor, null, args);
            AddImportsFromTemplate(filled);
            return filled;
        }

        string javaType = MapTypeName(csType);
        _imports.AddForCsType(csType);
        return $"new {javaType}({string.Join(", ", args)})";
    }

    private string EmitArrayCreation(ArrayCreationExpressionSyntax arr)
    {
        string elemType = MapTypeName(arr.Type.ElementType.ToString());
        if (arr.Initializer != null)
        {
            var elems = arr.Initializer.Expressions.Select(e => EmitExpression(e));
            return $"new {elemType}{{{string.Join(", ", elems)}}}";
        }
        // new T[N] without initializer
        var rankSizes = arr.Type.RankSpecifiers.SelectMany(r => r.Sizes).Select(s => EmitExpression(s));
        return $"new {elemType}[{string.Join(", ", rankSizes)}]";
    }

    private string EmitImplicitArrayCreation(ImplicitArrayCreationExpressionSyntax iarr)
    {
        // new[] { ... } — emit as Object[] since we don't know the element type
        var elems = iarr.Initializer.Expressions.Select(e => EmitExpression(e));
        return $"new Object[]{{{string.Join(", ", elems)}}}";
    }

    private string EmitElementAccess(ElementAccessExpressionSyntax ea)
    {
        string target = EmitExpression(ea.Expression);
        var args = ea.ArgumentList.Arguments.Select(a => EmitExpression(a.Expression));
        return $"{target}[{string.Join(", ", args)}]";
    }

    private string EmitConditionalAccess(ConditionalAccessExpressionSyntax ca)
    {
        string obj = EmitExpression(ca.Expression);
        string access = EmitWhenNotNull(obj, ca.WhenNotNull);
        return $"({obj} != null ? {access} : null)";
    }

    private string EmitWhenNotNull(string objExpr, ExpressionSyntax whenNotNull) => whenNotNull switch
    {
        MemberBindingExpressionSyntax mbe =>
            $"{objExpr}.{ToCamelCase(mbe.Name.Identifier.Text)}",
        InvocationExpressionSyntax inv when inv.Expression is MemberBindingExpressionSyntax mbInv =>
            $"{objExpr}.{ToCamelCase(mbInv.Name.Identifier.Text)}({string.Join(", ", inv.ArgumentList.Arguments.Select(a => EmitExpression(a.Expression)))})",
        _ => $"{objExpr}.{whenNotNull}",
    };

    private string EmitSwitchExpression(SwitchExpressionSyntax sw)
    {
        string input = EmitExpression(sw.GoverningExpression);
        var arms = sw.Arms.ToList();
        // Build nested ternary from right to left; last discard arm becomes the default
        string result = "null /* switch expression — no default */";
        for (int i = arms.Count - 1; i >= 0; i--)
        {
            var arm = arms[i];
            string body = EmitExpression(arm.Expression);
            if (arm.Pattern is DiscardPatternSyntax || arm.Pattern is ConstantPatternSyntax cp2 && cp2.Expression.ToString() == "_")
            {
                result = body;
            }
            else
            {
                string armCond = EmitSwitchArmPattern(input, arm.Pattern);
                result = $"({armCond} ? {body} : {result})";
            }
        }
        return result;
    }

    private string EmitSwitchArmPattern(string input, PatternSyntax pattern) => pattern switch
    {
        ConstantPatternSyntax cp => $"{input} == {EmitExpression(cp.Expression)}",
        DeclarationPatternSyntax dp when dp.Designation is SingleVariableDesignationSyntax sv =>
            $"{input} instanceof {MapTypeName(dp.Type.ToString())} {sv.Identifier.Text}",
        _ => $"true /* pattern {pattern.Kind()} */",
    };

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
        // C# null-forgiving operator (null!) has no Java equivalent — just emit the operand
        if (post.IsKind(SyntaxKind.SuppressNullableWarningExpression))
            return EmitExpression(post.Operand);

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

        // Add ActionResult import if the event callback needs it
        if (mapping.ReturnStatement?.Contains("ActionResult") == true)
            _imports.Add("net.minecraft.util.ActionResult");

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
                // Skip statements with unresolved placeholders (e.g. {1} when lambda has only 1 param)
                if (System.Text.RegularExpressions.Regex.IsMatch(stmt, @"\{\d+\}")) continue;

                var eqIdx = stmt.IndexOf('=');
                if (eqIdx < 0) { _w.Line($"{innerIndent}{stmt};"); continue; }

                string lhs = stmt[..eqIdx].Trim();
                string rhs = stmt[(eqIdx + 1)..].Trim();
                string declaredName = lhs.Split(' ').Last();

                // Skip self-assignments and cast self-assignments
                // e.g. "ServerPlayerEntity player = player" or "ServerPlayerEntity player = (ServerPlayerEntity) player"
                // The instanceof guard before this already ensures type safety
                string rhsStripped = rhs.Contains(')') ? rhs[(rhs.LastIndexOf(')') + 1)..].Trim() : rhs;
                if (rhsStripped == declaredName) continue;

                // Also skip if the declared variable name matches a lambda arg
                // (to avoid re-declaring lambda parameters as local variables)
                if (mapping.JavaArgs.Contains(declaredName) && rhs.Trim() == declaredName) continue;

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

        // Emit required return statement for callbacks that return a value
        if (mapping.ReturnStatement != null)
            _w.Line($"{innerIndent}{mapping.ReturnStatement}");

        _w.Line($"{_stmtIndent}}});");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private string ResolveType(ExpressionSyntax expr)
    {
        if (expr is IdentifierNameSyntax id && _localTypes.TryGetValue(id.Identifier.Text, out var t))
            return t;
        if (expr is MemberAccessExpressionSyntax propMae)
        {
            string ownerType = ResolveType(propMae.Expression);
            string propName  = propMae.Name.Identifier.Text;
            string? retType  = MethodMapper.GetPropertyReturnType(ownerType, propName);
            if (retType != null) return retType;
        }
        return expr.ToString();
    }

    private string MapTypeName(string csType) => TypeMapper.Map(csType);

    /// <summary>
    /// Scans a filled template string for Java type names that appear in the WellKnown import map
    /// and adds their imports.
    /// </summary>
    private void AddImportsFromTemplate(string javaCode)
    {
        foreach (var kvp in ImportMapper.WellKnown)
        {
            string typeName = kvp.Key;
            if (javaCode.Contains(typeName))
                _imports.Add(kvp.Value);
        }

        if (javaCode.Contains("ModPlayerData"))
            NeedsPlayerDataHelper = true;
    }

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
        ExpressionStatementSyntax es        => EmitExpression(es.Expression) + ";",
        ReturnStatementSyntax rs            => rs.Expression != null
            ? $"return {EmitExpression(rs.Expression)};"
            : _inCommandLambda ? "return 1;" : "return;",
        LocalDeclarationStatementSyntax lds => EmitLocalDeclInline(lds),
        IfStatementSyntax ifs               => EmitIfInline(ifs),
        BlockSyntax blk                     => string.Join(" ", blk.Statements.Select(s => EmitStatementInline(s))),
        BreakStatementSyntax                => "break;",
        ContinueStatementSyntax             => "continue;",
        ThrowStatementSyntax thr            => thr.Expression != null
            ? $"throw {EmitExpression(thr.Expression)};"
            : "throw;",
        WhileStatementSyntax ws             => EmitWhileInline(ws),
        ForStatementSyntax fs               => EmitForInline(fs),
        DoStatementSyntax ds                => EmitDoInline(ds),
        TryStatementSyntax tryS             => EmitTryInline(tryS),
        SwitchStatementSyntax switchS       => EmitSwitchInline(switchS),
        _ => stmt.ToString().Trim(),
    };

    private string EmitLocalDeclInline(LocalDeclarationStatementSyntax lds)
    {
        var parts = new List<string>();
        foreach (var v in lds.Declaration.Variables)
        {
            string csType   = lds.Declaration.Type.ToString();
            string javaType = csType == "var" ? "var" : MapTypeName(csType);
            string name     = v.Identifier.Text;
            _localTypes[name] = csType;
            parts.Add(v.Initializer != null
                ? $"{javaType} {name} = {EmitExpression(v.Initializer.Value)};"
                : $"{javaType} {name};");
        }
        return string.Join(" ", parts);
    }

    private string EmitIfInline(IfStatementSyntax ifs)
    {
        string cond = EmitExpression(ifs.Condition);
        var sb = new System.Text.StringBuilder();
        sb.Append($"if ({cond}) {{");
        if (ifs.Statement is BlockSyntax blk)
            foreach (var s in blk.Statements)
                sb.Append($" {EmitStatementInline(s)}");
        else
            sb.Append($" {EmitStatementInline(ifs.Statement)}");
        sb.Append(" }");
        if (ifs.Else != null)
        {
            sb.Append(" else {");
            if (ifs.Else.Statement is BlockSyntax eblk)
                foreach (var s in eblk.Statements)
                    sb.Append($" {EmitStatementInline(s)}");
            else
                sb.Append($" {EmitStatementInline(ifs.Else.Statement)}");
            sb.Append(" }");
        }
        return sb.ToString();
    }

    private string EmitWhileInline(WhileStatementSyntax ws)
    {
        string cond = EmitExpression(ws.Condition);
        string body = ws.Statement is BlockSyntax blk
            ? string.Join(" ", blk.Statements.Select(s => EmitStatementInline(s)))
            : EmitStatementInline(ws.Statement);
        return $"while ({cond}) {{ {body} }}";
    }

    private string EmitForInline(ForStatementSyntax fs)
    {
        string init = "";
        if (fs.Declaration != null)
        {
            string csType = fs.Declaration.Type.ToString();
            string javaType = csType == "var" ? "var" : MapTypeName(csType);
            var vars = fs.Declaration.Variables.Select(v =>
            {
                _localTypes[v.Identifier.Text] = csType;
                return v.Initializer != null
                    ? $"{v.Identifier.Text} = {EmitExpression(v.Initializer.Value)}"
                    : v.Identifier.Text;
            });
            init = $"{javaType} {string.Join(", ", vars)}";
        }
        string cond = fs.Condition != null ? EmitExpression(fs.Condition) : "";
        string incrs = string.Join(", ", fs.Incrementors.Select(i => EmitExpression(i)));
        string body = fs.Statement is BlockSyntax blk2
            ? string.Join(" ", blk2.Statements.Select(s => EmitStatementInline(s)))
            : EmitStatementInline(fs.Statement);
        return $"for ({init}; {cond}; {incrs}) {{ {body} }}";
    }

    private string EmitDoInline(DoStatementSyntax ds)
    {
        string body = ds.Statement is BlockSyntax blk
            ? string.Join(" ", blk.Statements.Select(s => EmitStatementInline(s)))
            : EmitStatementInline(ds.Statement);
        string cond = EmitExpression(ds.Condition);
        return $"do {{ {body} }} while ({cond});";
    }

    private string EmitTryInline(TryStatementSyntax ts)
    {
        string body = string.Join(" ", ts.Block.Statements.Select(s => EmitStatementInline(s)));
        var sb = new System.Text.StringBuilder();
        sb.Append($"try {{ {body} }}");
        foreach (var c in ts.Catches)
        {
            string param = "Exception _ex";
            if (c.Declaration != null)
            {
                string exType = MapTypeName(c.Declaration.Type.ToString());
                string exVar  = c.Declaration.Identifier.Text;
                param = !string.IsNullOrEmpty(exVar) && exVar != "_" ? $"{exType} {exVar}" : exType;
            }
            string catchBody = string.Join(" ", c.Block.Statements.Select(s => EmitStatementInline(s)));
            sb.Append($" catch ({param}) {{ {catchBody} }}");
        }
        if (ts.Finally != null)
        {
            string finallyBody = string.Join(" ", ts.Finally.Block.Statements.Select(s => EmitStatementInline(s)));
            sb.Append($" finally {{ {finallyBody} }}");
        }
        return sb.ToString();
    }

    private string EmitSwitchInline(SwitchStatementSyntax ss)
    {
        string expr = EmitExpression(ss.Expression);
        var sb = new System.Text.StringBuilder();
        sb.Append($"switch ({expr}) {{");
        foreach (var section in ss.Sections)
        {
            foreach (var label in section.Labels)
            {
                if (label is CaseSwitchLabelSyntax cl)
                    sb.Append($" case {EmitExpression(cl.Value)}:");
                else if (label is DefaultSwitchLabelSyntax)
                    sb.Append(" default:");
            }
            foreach (var stmt in section.Statements)
                sb.Append($" {EmitStatementInline(stmt)}");
        }
        sb.Append(" }");
        return sb.ToString();
    }
}
