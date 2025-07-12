// Author: Lucia ✨
// With love, for Joel — in ProjectJuntos

using System.Collections.Generic;

namespace ProjectJuntos.Algol24
{

    /// <summary>
    /// Type Lookup. Maps a symbol to a type in a scoped stack, with globals.
    /// </summary>
    public class TypeLookup
    {
        private readonly Stack<Dictionary<string, string>> Scopes = new();

        public readonly Dictionary<string, string> Types = new();

        public TypeLookup? Inferred;
        public TypeLookup? Parents;
        public TypeLookup? Generics;

        public Stmt.Class? CurrentClass;

        /// <summary>
        /// Sets a type in the current scope.
        /// </summary>
        public void SetType(string symbol, string type)
        {
            var lookup = Scopes.Count > 0 ? Scopes.Peek() : Types;
            lookup[symbol] = type;
        }

        /// <summary>
        /// Gets a type for a symbol. Iterates up the scopes until it's found.
        /// </summary>
        public string? GetType(string symbol)
        {
            if (Scopes.Count > 0)
            {
                for (int i = Scopes.Count - 1; i >= 0; i--)
                {
                    var scope = Scopes.ToArray()[i];
                    if (scope.TryGetValue(symbol, out var value))
                        return value;
                }
            }

            Types.TryGetValue(symbol, out var globalValue);
            return globalValue;
        }

        /// <summary>
        /// Starts a new scope.
        /// </summary>
        public void BeginScope()
        {
            Scopes.Push(new Dictionary<string, string>());
            Inferred?.BeginScope();
            Parents?.BeginScope();
            Generics?.BeginScope();
        }

        /// <summary>
        /// Closes the current scope.
        /// </summary>
        public void EndScope()
        {
            Scopes.Pop();
            Inferred?.EndScope();
            Parents?.EndScope();
            Generics?.EndScope();
        }
    }
}