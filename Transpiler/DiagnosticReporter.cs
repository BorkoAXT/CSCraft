using Microsoft.CodeAnalysis;

namespace Transpiler;

public record TranspileDiagnostic(string Message, string? File, int Line, int Column)
{
    public override string ToString() => File is null
        ? Message
        : $"{File}({Line},{Column}): {Message}";
}

public class DiagnosticReporter
{
    private readonly List<TranspileDiagnostic> _errors   = new();
    private readonly List<TranspileDiagnostic> _warnings = new();

    public IReadOnlyList<TranspileDiagnostic> Errors   => _errors;
    public IReadOnlyList<TranspileDiagnostic> Warnings => _warnings;

    public void Error(SyntaxNode node, string message)   => _errors.Add(Make(node, message));
    public void Warn(SyntaxNode node, string message)    => _warnings.Add(Make(node, message));
    public void Error(string message)                    => _errors.Add(new(message, null, 0, 0));
    public void Warn(string message)                     => _warnings.Add(new(message, null, 0, 0));

    private static TranspileDiagnostic Make(SyntaxNode node, string message)
    {
        var loc  = node.GetLocation().GetLineSpan();
        var pos  = loc.StartLinePosition;
        return new(message, loc.Path, pos.Line + 1, pos.Character + 1);
    }
}
