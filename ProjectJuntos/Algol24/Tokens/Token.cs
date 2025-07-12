// Author: Lucia ✨
// With love, for Joel — in ProjectJuntos

namespace ProjectJuntos.Algol24.Tokens
{
    /// <summary>
    /// Represents a single lexical token in Algol-24 source.
    /// </summary>
    public class Token
    {
        public TokenType Type { get; }
        public string Lexeme { get; }
        public object? Literal { get; }
        public int Line { get; }
        public int Offset { get; }
        public string FileName { get; }

        /// <summary>
        /// Constructs a new token.
        /// </summary>
        /// <param name="type">The token type.</param>
        /// <param name="lexeme">The lexeme as written in the source.</param>
        /// <param name="literal">The literal value, if any.</param>
        /// <param name="line">Line number in the source file.</param>
        /// <param name="offset">Column offset of the token.</param>
        /// <param name="fileName">Name of the source file.</param>
        public Token(TokenType type, string lexeme, object? literal, int line, int offset, string fileName)
        {
            Type = type;
            Lexeme = lexeme;
            Literal = literal;
            Line = line;
            Offset = offset;
            FileName = fileName;
        }

        public override string ToString()
        {
            return $"{Type} {Lexeme} {(Literal ?? "")}";
        }
    }
}
