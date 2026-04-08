namespace CSCraft.Detranspiler.Core;

/// <summary>
/// Wraps a token list with a position cursor and helper methods.
/// </summary>
public class TokenCursor
{
    private readonly List<JavaToken> _tokens;
    private int _pos;

    public TokenCursor(List<JavaToken> tokens) => _tokens = tokens;

    public JavaToken Current    => _pos < _tokens.Count ? _tokens[_pos] : _tokens[^1];
    public JavaToken Peek(int offset = 1) => (_pos + offset < _tokens.Count) ? _tokens[_pos + offset] : _tokens[^1];
    public bool      AtEnd      => Current.Kind == TokenKind.EOF;
    public int       Position   => _pos;

    public JavaToken Consume()
    {
        var t = Current;
        if (_pos < _tokens.Count - 1) _pos++;
        return t;
    }

    public JavaToken Expect(TokenKind k)
    {
        if (Current.Kind != k)
            throw new InvalidOperationException($"Expected {k} but got {Current} at line {Current.Line}");
        return Consume();
    }

    public bool TryConsume(TokenKind k)
    {
        if (Current.Kind != k) return false;
        Consume(); return true;
    }

    public bool TryConsume(TokenKind k, string v)
    {
        if (Current.Kind != k || Current.Value != v) return false;
        Consume(); return true;
    }

    public void SeekTo(int pos) => _pos = pos;

    /// <summary>
    /// Reads tokens until the brace/paren depth returns to 0 (after consuming the opening bracket).
    /// Returns the raw token list between the brackets (not including them).
    /// </summary>
    public List<JavaToken> ReadBlock(TokenKind open, TokenKind close)
    {
        Expect(open);
        int depth = 1;
        var result = new List<JavaToken>();
        while (!AtEnd && depth > 0)
        {
            var t = Consume();
            if (t.Kind == open) depth++;
            else if (t.Kind == close) { depth--; if (depth == 0) break; }
            result.Add(t);
        }
        return result;
    }

    /// <summary>
    /// Reads everything up to (but not including) the next token of the given kind at depth 0.
    /// </summary>
    public List<JavaToken> ReadUntil(TokenKind stopKind)
    {
        var result = new List<JavaToken>();
        int depth = 0;
        while (!AtEnd)
        {
            var t = Current;
            if ((t.Kind == TokenKind.LParen || t.Kind == TokenKind.LBrace || t.Kind == TokenKind.LBracket)) depth++;
            else if ((t.Kind == TokenKind.RParen || t.Kind == TokenKind.RBrace || t.Kind == TokenKind.RBracket)) depth--;
            if (depth == 0 && t.Kind == stopKind) break;
            result.Add(Consume());
        }
        return result;
    }

    /// <summary>
    /// Joins tokens back to a string (roughly — no perfect spacing).
    /// </summary>
    public static string Stringify(IEnumerable<JavaToken> tokens)
    {
        var sb = new System.Text.StringBuilder();
        JavaToken? prev = null;
        foreach (var t in tokens)
        {
            if (prev != null && NeedsSpace(prev, t))
                sb.Append(' ');
            sb.Append(t.Value);
            prev = t;
        }
        return sb.ToString();
    }

    private static bool NeedsSpace(JavaToken prev, JavaToken cur)
    {
        if (prev.Kind is TokenKind.Dot or TokenKind.LParen or TokenKind.LBracket) return false;
        if (cur.Kind  is TokenKind.Dot or TokenKind.RParen or TokenKind.RBracket or
                         TokenKind.Comma or TokenKind.Semicolon or TokenKind.LParen or
                         TokenKind.LBracket) return false;
        if (prev.Kind == TokenKind.LBrace || cur.Kind == TokenKind.RBrace) return true;
        return true;
    }
}
