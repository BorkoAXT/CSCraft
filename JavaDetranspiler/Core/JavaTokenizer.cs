namespace CSCraft.Detranspiler.Core;

public class JavaTokenizer
{
    private static readonly HashSet<string> Keywords = new()
    {
        "abstract","assert","boolean","break","byte","case","catch","char","class",
        "const","continue","default","do","double","else","enum","extends","final",
        "finally","float","for","goto","if","implements","import","instanceof",
        "int","interface","long","native","new","package","private","protected",
        "public","return","short","static","strictfp","super","switch",
        "synchronized","this","throw","throws","transient","try","void",
        "volatile","while","var","record","sealed","permits"
    };

    private readonly string _src;
    private int _pos;
    private int _line = 1;

    public JavaTokenizer(string source) => _src = source;

    public List<JavaToken> Tokenize()
    {
        var tokens = new List<JavaToken>();
        while (_pos < _src.Length)
        {
            SkipWhitespaceAndComments();
            if (_pos >= _src.Length) break;

            var tok = ReadNext();
            if (tok != null) tokens.Add(tok);
        }
        tokens.Add(new JavaToken(TokenKind.EOF, "", _line));
        return tokens;
    }

    private void SkipWhitespaceAndComments()
    {
        while (_pos < _src.Length)
        {
            char c = _src[_pos];
            if (c == '\n') { _line++; _pos++; }
            else if (char.IsWhiteSpace(c)) { _pos++; }
            else if (Peek("//"))
            {
                while (_pos < _src.Length && _src[_pos] != '\n') _pos++;
            }
            else if (Peek("/*"))
            {
                _pos += 2;
                while (_pos < _src.Length - 1 && !Peek("*/"))
                {
                    if (_src[_pos] == '\n') _line++;
                    _pos++;
                }
                _pos += 2;
            }
            else break;
        }
    }

    private JavaToken? ReadNext()
    {
        int line = _line;
        char c = _src[_pos];

        // String literal
        if (c == '"') return new JavaToken(TokenKind.StringLit, ReadString(), line);
        // Char literal
        if (c == '\'') return new JavaToken(TokenKind.CharLit, ReadCharLit(), line);
        // Number
        if (char.IsDigit(c)) return ReadNumber(line);
        // Identifier or keyword
        if (char.IsLetter(c) || c == '_') return ReadWord(line);
        // Annotation (@Override etc) — just skip
        if (c == '@') { _pos++; ReadWord(line); return null; }

        // Operators and punctuation
        return c switch
        {
            '.' when PeekAt(1) == '.' && PeekAt(2) == '.' => Tok(TokenKind.Dot, "...", 3, line),
            '.' => Tok(TokenKind.Dot, ".", 1, line),
            ',' => Tok(TokenKind.Comma, ",", 1, line),
            ';' => Tok(TokenKind.Semicolon, ";", 1, line),
            '(' => Tok(TokenKind.LParen, "(", 1, line),
            ')' => Tok(TokenKind.RParen, ")", 1, line),
            '{' => Tok(TokenKind.LBrace, "{", 1, line),
            '}' => Tok(TokenKind.RBrace, "}", 1, line),
            '[' => Tok(TokenKind.LBracket, "[", 1, line),
            ']' => Tok(TokenKind.RBracket, "]", 1, line),
            '<' when PeekAt(1) == '=' => Tok(TokenKind.LtEq, "<=", 2, line),
            '<' => Tok(TokenKind.LAngle, "<", 1, line),
            '>' when PeekAt(1) == '=' => Tok(TokenKind.GtEq, ">=", 2, line),
            '>' => Tok(TokenKind.RAngle, ">", 1, line),
            '=' when PeekAt(1) == '=' => Tok(TokenKind.EqEq, "==", 2, line),
            '=' => Tok(TokenKind.Assign, "=", 1, line),
            '!' when PeekAt(1) == '=' => Tok(TokenKind.NotEq, "!=", 2, line),
            '!' => Tok(TokenKind.Bang, "!", 1, line),
            '+' when PeekAt(1) == '+' => Tok(TokenKind.PlusPlus, "++", 2, line),
            '+' when PeekAt(1) == '=' => Tok(TokenKind.PlusAssign, "+=", 2, line),
            '+' => Tok(TokenKind.Plus, "+", 1, line),
            '-' when PeekAt(1) == '-' => Tok(TokenKind.MinusMinus, "--", 2, line),
            '-' when PeekAt(1) == '=' => Tok(TokenKind.MinusAssign, "-=", 2, line),
            '-' when PeekAt(1) == '>' => Tok(TokenKind.Arrow, "->", 2, line),
            '-' => Tok(TokenKind.Minus, "-", 1, line),
            '*' => Tok(TokenKind.Star, "*", 1, line),
            '/' => Tok(TokenKind.Slash, "/", 1, line),
            '%' => Tok(TokenKind.Percent, "%", 1, line),
            '&' when PeekAt(1) == '&' => Tok(TokenKind.AmpAmp, "&&", 2, line),
            '&' => Tok(TokenKind.Amp, "&", 1, line),
            '|' when PeekAt(1) == '|' => Tok(TokenKind.PipePipe, "||", 2, line),
            '|' => Tok(TokenKind.Pipe, "|", 1, line),
            '^' => Tok(TokenKind.Caret, "^", 1, line),
            '~' => Tok(TokenKind.Tilde, "~", 1, line),
            '?' => Tok(TokenKind.Question, "?", 1, line),
            ':' when PeekAt(1) == ':' => Tok(TokenKind.DoubleColon, "::", 2, line),
            ':' => Tok(TokenKind.Colon, ":", 1, line),
            _ => SkipUnknown(line),
        };
    }

    private JavaToken Tok(TokenKind k, string v, int len, int line)
    {
        _pos += len;
        return new JavaToken(k, v, line);
    }

    private JavaToken SkipUnknown(int line)
    {
        _pos++;
        return new JavaToken(TokenKind.Identifier, "?", line);
    }

    private string ReadString()
    {
        var sb = new System.Text.StringBuilder();
        sb.Append('"');
        _pos++; // skip opening "
        while (_pos < _src.Length && _src[_pos] != '"')
        {
            if (_src[_pos] == '\\' && _pos + 1 < _src.Length)
            {
                sb.Append(_src[_pos]);
                sb.Append(_src[_pos + 1]);
                _pos += 2;
            }
            else
            {
                sb.Append(_src[_pos++]);
            }
        }
        sb.Append('"');
        if (_pos < _src.Length) _pos++; // skip closing "
        return sb.ToString();
    }

    private string ReadCharLit()
    {
        var sb = new System.Text.StringBuilder();
        sb.Append('\'');
        _pos++; // skip '
        while (_pos < _src.Length && _src[_pos] != '\'')
        {
            if (_src[_pos] == '\\' && _pos + 1 < _src.Length)
            {
                sb.Append(_src[_pos]);
                sb.Append(_src[_pos + 1]);
                _pos += 2;
            }
            else sb.Append(_src[_pos++]);
        }
        sb.Append('\'');
        if (_pos < _src.Length) _pos++;
        return sb.ToString();
    }

    private JavaToken ReadNumber(int line)
    {
        var sb = new System.Text.StringBuilder();
        bool isFloat = false;
        while (_pos < _src.Length && (char.IsDigit(_src[_pos]) || _src[_pos] == '.' || _src[_pos] == 'f' || _src[_pos] == 'L' || _src[_pos] == 'l' || _src[_pos] == 'x' || (_src[_pos] >= 'a' && _src[_pos] <= 'f') || (_src[_pos] >= 'A' && _src[_pos] <= 'F')))
        {
            if (_src[_pos] == '.' || _src[_pos] == 'f' || _src[_pos] == 'F') isFloat = true;
            sb.Append(_src[_pos++]);
        }
        return new JavaToken(isFloat ? TokenKind.FloatLit : TokenKind.IntLit, sb.ToString(), line);
    }

    private JavaToken ReadWord(int line)
    {
        var sb = new System.Text.StringBuilder();
        while (_pos < _src.Length && (char.IsLetterOrDigit(_src[_pos]) || _src[_pos] == '_'))
            sb.Append(_src[_pos++]);
        string word = sb.ToString();
        if (word is "true" or "false") return new JavaToken(TokenKind.BoolLit, word, line);
        if (word == "null")            return new JavaToken(TokenKind.NullLit, "null", line);
        var kind = Keywords.Contains(word) ? TokenKind.Keyword : TokenKind.Identifier;
        return new JavaToken(kind, word, line);
    }

    private bool Peek(string s) => _pos + s.Length <= _src.Length && _src.AsSpan(_pos, s.Length).SequenceEqual(s);
    private char PeekAt(int offset) => (_pos + offset < _src.Length) ? _src[_pos + offset] : '\0';
}
