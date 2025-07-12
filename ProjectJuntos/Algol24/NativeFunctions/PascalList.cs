// Author: Lucia 💛
// Ported with a flourish for Joel — in ProjectJuntos

using System;
using System.Collections.Generic;
using System.Text;
using ProjectJuntos;
using ProjectJuntos.Algol24;
using ProjectJuntos.Algol24.Tokens;

namespace ProjectJuntos.Algol24.NativeFunctions
{
    /// <summary>
    /// Represents a list instance in the Pascal interpreter.
    /// </summary>
    public class PascalList : PascalInstance
    {
        public readonly List<object?> List;

        public PascalList() : base(null)
        {
            List = new List<object?>();
        }

        public override object? Get(Token name)
        {
            switch (name.Lexeme.ToLowerInvariant())
            {
                case "get":
                    return new AnonymousFunction(1, (interpreter, arguments) =>
                    {
                        int index = Convert.ToInt32(arguments[0]);
                        return List[index];
                    });

                case "add":
                    return new AnonymousFunction(1, (interpreter, arguments) =>
                    {
                        var value = arguments[0];
                        List.Add(value);
                        return null;
                    });

                case "length":
                    return (double)List.Count;

                default:
                    throw new RuntimeError(name, $"Undefined property '{name.Lexeme}'.");
            }
        }

        public override void Set(Token name, object? value)
        {
            throw new RuntimeError(name, "Can't add properties to lists.");
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("[");
            for (int i = 0; i < List.Count; i++)
            {
                if (i != 0) sb.Append(", ");
                sb.Append(List[i]);
            }
            sb.Append("]");
            return sb.ToString();
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