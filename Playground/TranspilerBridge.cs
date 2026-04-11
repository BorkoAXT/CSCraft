using Microsoft.JSInterop;
using Transpiler;

namespace Playground;

public static class TranspilerBridge
{
    /// <summary>
    /// Called from JavaScript via DotNet.invokeMethodAsync('Playground', 'Transpile', csCode, packageName).
    /// </summary>
    [JSInvokable]
    public static TranspileWebResult Transpile(string csCode, string packageName)
    {
        try
        {
            var result = TranspilerRunner.Transpile(csCode, packageName);
            return new TranspileWebResult
            {
                Java     = result.JavaSource,
                Errors   = result.Errors  .Select(e => new DiagnosticItem { Message = e.Message, Line = e.Line }).ToArray(),
                Warnings = result.Warnings.Select(w => new DiagnosticItem { Message = w.Message, Line = w.Line }).ToArray(),
            };
        }
        catch (Exception ex)
        {
            return new TranspileWebResult
            {
                Java   = "",
                Errors = [new DiagnosticItem { Message = ex.Message, Line = 0 }],
            };
        }
    }
}

public class TranspileWebResult
{
    public string           Java     { get; set; } = "";
    public DiagnosticItem[] Errors   { get; set; } = [];
    public DiagnosticItem[] Warnings { get; set; } = [];
}

public class DiagnosticItem
{
    public string Message { get; set; } = "";
    public int    Line    { get; set; }
}
