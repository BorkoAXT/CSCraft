namespace CSCraft.Detranspiler.Core;

public enum TokenKind
{
    // Literals
    StringLit, CharLit, IntLit, FloatLit, BoolLit, NullLit,
    // Names
    Identifier, Keyword,
    // Punctuation
    Dot, Comma, Semicolon, At,
    LParen, RParen, LBrace, RBrace, LBracket, RBracket,
    LAngle, RAngle,
    // Operators
    Assign, PlusAssign, MinusAssign,
    Plus, Minus, Star, Slash, Percent,
    Bang, Amp, Pipe, Caret, Tilde,
    Question, Colon, Arrow, DoubleColon,
    EqEq, NotEq, GtEq, LtEq, AmpAmp, PipePipe,
    PlusPlus, MinusMinus,
    // Special
    EOF
}

public record JavaToken(TokenKind Kind, string Value, int Line)
{
    public bool Is(TokenKind k)             => Kind == k;
    public bool Is(string v)                => Value == v;
    public bool IsKeyword(string kw)        => Kind == TokenKind.Keyword && Value == kw;
    public bool IsIdent(string name)        => Kind == TokenKind.Identifier && Value == name;
    public override string ToString()       => $"{Kind}({Value})";
}
