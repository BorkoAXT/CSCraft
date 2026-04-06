namespace Transpiler;

public record TranspileResult(
    string JavaSource,
    List<TranspileDiagnostic> Errors,
    List<TranspileDiagnostic> Warnings,
    HashSet<string> RequiredImports,
    Dictionary<string, string>? ExtraJavaFiles = null  // filename → source (helper classes)
);
