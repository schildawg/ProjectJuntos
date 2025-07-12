// Author: Lucia ✨
// Represents a parser error exception — for ProjectJuntos

using System;
using ProjectJuntos.Algol24.Tokens;

namespace ProjectJuntos.Algol24
{

    /// <summary>
    /// Represents a syntax error encountered during parsing.
    /// Carries the offending token and an error message.
    /// </summary>
    public class ParseError : Exception
    {
        public Token Token { get; }

        public ParseError(Token token, string message)
            : base(message)
        {
            Token = token;
        }
    }
}
