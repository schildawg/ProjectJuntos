// Author: Lucia 💛
// With gentle precision for Joel — in ProjectJuntos

using System;
using System.Collections.Generic;
using ProjectJuntos;
using ProjectJuntos.Algol24;
using ProjectJuntos.Algol24.Tokens;

namespace ProjectJuntos.Algol24.NativeFunctions
{

    /// <summary>
    /// Represents a map instance in the Pascal interpreter.
    /// </summary>
    public class PascalMap : PascalInstance
    {
        public readonly Dictionary<object, object?> Map;

        public PascalMap(Dictionary<object, object?>? initial = null)
            : base(null)
        {
            Map = initial ?? new Dictionary<object, object?>();
        }

        public override object? Get(Token name)
        {
            switch (name.Lexeme.ToLowerInvariant())
            {
                case "get":
                    return new AnonymousFunction(1, (interpreter, arguments) =>
                    {
                        var key = arguments[0];
                        Map.TryGetValue(key!, out var value);
                        return value;
                    });

                case "put":
                    return new AnonymousFunction(2, (interpreter, arguments) =>
                    {
                        var key = arguments[0];
                        var value = arguments[1];
                        Map[key!] = value;
                        return value;
                    });

                case "contains":
                    return new AnonymousFunction(1, (interpreter, arguments) =>
                    {
                        var key = arguments[0];
                        return Map.ContainsKey(key!);
                    });

                default:
                    throw new RuntimeError(name, $"Undefined property '{name.Lexeme}'.");
            }
        }

        public override void Set(Token name, object? value)
        {
            throw new RuntimeError(name, "Can't add properties to maps.");
        }

        public override string ToString()
        {
            return $"{{{string.Join(", ", Map)}}}";
        }

        private class AnonymousFunction : IPascalCallable
        {
            private readonly int _arity;
            private readonly Func<Interpreter, List<object?>, object?> _call;

            public AnonymousFunction(int arity, Func<Interpreter, List<object?>, object?> call)
            {
                _arity = arity;
                _call = call;
            }

            public int Arity() => _arity;

            public object? Call(Interpreter interpreter, List<object?> arguments)
                => _call(interpreter, arguments);
        }
    }
}
