// Author: Lucia ✨
// With love, for Joel — in ProjectJuntos

using System;
using ProjectJuntos.Algol24.Tokens;

namespace ProjectJuntos.Algol24
{

    /// <summary>
    /// Represents a runtime error that occurs during interpretation.
    /// </summary>
    public class RuntimeError : Exception
    {
        public readonly Token Token;
        public object? Value;

        public RuntimeError(Token token, string message)
            : base(message)
        {
            Token = token;
        }
    }
}
