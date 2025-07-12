// Author: Lucia ✨
// Ported with care for Joel — in ProjectJuntos

using System;
using System.Collections.Generic;
using ProjectJuntos;
using ProjectJuntos.Algol24;
using ProjectJuntos.Algol24.Tokens;

namespace ProjectJuntos.Algol24.NativeFunctions
{

    /// <summary>
    /// Represents an array instance in the Pascal interpreter.
    /// </summary>
    public class PascalArray : PascalInstance
    {
        private readonly object?[] _elements;

        public PascalArray(int size) : base(null)
        {
            _elements = new object?[size];
        }

        public override object? Get(Token name)
        {
            return name.Lexeme switch
            {
                "get" => new AnonymousFunction(1, (interpreter, arguments) =>
                {
                    int index = Convert.ToInt32(arguments[0]);
                    return _elements[index];
                }),

                "set" => new AnonymousFunction(2, (interpreter, arguments) =>
                {
                    int index = Convert.ToInt32(arguments[0]);
                    var value = arguments[1];
                    return _elements[index] = value;
                }),

                "length" => (double)_elements.Length,

                _ => throw new RuntimeError(name, $"Undefined property '{name.Lexeme}'.")
            };
        }

        public override void Set(Token name, object? value)
        {
            throw new RuntimeError(name, "Can't add properties to arrays.");
        }

        public override string ToString()
        {
            return "[" + string.Join(", ", _elements) + "]";
        }

        /// <summary>
        /// Local anonymous function used for array get/set.
        /// </summary>
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
