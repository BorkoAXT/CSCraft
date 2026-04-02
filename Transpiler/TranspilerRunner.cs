using Microsoft.CodeAnalysis.CSharp;

namespace Transpiler;

public static class TranspilerRunner
{
    /// <summary>
    /// Transpiles a C# mod source string into a Java Fabric mod source string.
    /// </summary>
    /// <param name="csSource">The full C# source text.</param>
    /// <param name="packageName">Java package name, e.g. "com.yourmod".</param>
    public static TranspileResult Transpile(string csSource, string packageName)
    {
        var tree     = CSharpSyntaxTree.ParseText(csSource);
        var tracker  = new ImportTracker();
        var writer   = new JavaWriter();
        var reporter = new DiagnosticReporter();
        var emitter  = new JavaEmitter(writer, tracker, reporter);

        emitter.Visit(tree.GetRoot());

        string header = $"package {packageName};\n\n"
                      + tracker.GetImportBlock()
                      + "\n\n";

        return new TranspileResult(
            JavaSource:      header + writer.GetOutput(),
            Errors:          reporter.Errors.ToList(),
            Warnings:        reporter.Warnings.ToList(),
            RequiredImports: tracker.GetImports()
        );
    }
}
