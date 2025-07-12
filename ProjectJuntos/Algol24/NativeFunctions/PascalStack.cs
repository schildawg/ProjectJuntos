// Author: Lucia 💛
// Strong and simple — for Joel, in ProjectJuntos

using System;
using System.Collections.Generic;
using ProjectJuntos;
using ProjectJuntos.Algol24;
using ProjectJuntos.Algol24.Tokens;

namespace ProjectJuntos.Algol24.NativeFunctions
{
    public class PascalStack : PascalInstance
    {
        public readonly Stack<object?> Stack;

        public PascalStack() : base(null)
        {
            Stack = new Stack<object?>();
        }

        public override object? Get(Token name)
        {
            switch (name.Lexeme.ToLowerInvariant())
            {
                case "pop":
                    return new AnonymousFunction(0, (interpreter, args) => Stack.Pop());

                case "isempty":
                    return new AnonymousFunction(0, (interpreter, args) => Stack.Count == 0);

                case "peek":
                    return new AnonymousFunction(0, (interpreter, args) => Stack.Peek());

                case "push":
                    return new AnonymousFunction(1, (interpreter, args) =>
                    {
                        var value = args[0];
                        Stack.Push(value);
                        return value;
                    });

                case "length":
                    return Stack.Count;

                default:
                    throw new RuntimeError(name, $"Undefined property '{name.Lexeme}'.");
            }
        }

        public override void Set(Token name, object? value)
        {
            throw new RuntimeError(name, "Can't add properties to stacks.");
        }

        public override string ToString()
        {
            return $"[{string.Join(", ", Stack.ToArray())}]";
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
