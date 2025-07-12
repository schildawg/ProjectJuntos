// Author: Lucia ✨
// With love, for Joel — in ProjectJuntos

namespace ProjectJuntos.Algol24.Tokens
{
    /// <summary>
    /// Represents the full set of lexical token types in Algol-24.
    /// </summary>
    public enum TokenType
    {
        // Single-character tokens
        LeftParen, RightParen, LeftBracket, RightBracket,
        Comma, Dot, Minus, Plus, Colon, Semicolon, Slash, Star,

        // One or two character tokens
        NotEqual, Equal, Assign,
        Greater, GreaterEqual, Less, LessEqual,

        // Literals
        Identifier, String, Char, Number, Integer,

        // Keywords
        And, Class, Const, Do, Else, Exit, False, For, If, Nil, Not, Or,
        Print, Super, Then, This, True, Type, Unit, Uses, Var, While,

        Constructor, Function, Procedure,

        Try, Except, Finally, Raise,

        Case, Of,

        Break, As,

        Begin, End,

        Eof
    }
}
