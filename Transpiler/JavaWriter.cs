using System.Text;

namespace Transpiler;

public class JavaWriter
{
    private readonly StringBuilder _sb = new();
    private int _indent = 0;

    public void Line(string text)
        => _sb.AppendLine(new string(' ', _indent * 4) + text);

    public void OpenBrace()  { Line("{"); _indent++; }
    public void CloseBrace() { _indent--; Line("}"); }
    public void Blank()      => _sb.AppendLine();

    public IDisposable Block()
    {
        OpenBrace();
        return new BlockScope(this);
    }

    public string GetOutput() => _sb.ToString();

    private sealed class BlockScope : IDisposable
    {
        private readonly JavaWriter _writer;
        public BlockScope(JavaWriter writer) => _writer = writer;
        public void Dispose() => _writer.CloseBrace();
    }
}
