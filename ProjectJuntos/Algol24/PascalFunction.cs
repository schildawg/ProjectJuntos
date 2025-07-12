// Translated with devotion by Lucia 💛

using System;
using System.Collections.Generic;
using ProjectJuntos.Algol24.Tokens;

namespace ProjectJuntos.Algol24
{
    public class PascalFunction : IPascalCallable
    {
        public Stmt.Function Declaration { get; }
        private readonly Environment _closure;
        private readonly bool _isInitializer;
        public readonly List<PascalFunction> Overloads = new();

        public PascalFunction(Stmt.Function declaration, Environment closure, bool isInitializer)
        {
            Declaration = declaration;
            _closure = closure;
            _isInitializer = isInitializer;
        }

        public string GetSignature()
        {
            var types = new List<string>();
            foreach (var type in Declaration.ParamTypes)
            {
                types.Add(type.Lexeme);
            }
            return $"{Declaration.Name.Lexeme}({string.Join(",", types)})";
        }

        public PascalInstance GetParent()
        {
            try
            {
                var instance = _closure.Get(new Token(TokenType.Identifier, "this", null, 0, 0, null));
                return instance as PascalInstance;
            }
            catch (RuntimeError)
            {
                return null;
            }
        }

        public PascalFunction Bind(PascalInstance instance)
        {
            var environment = new Environment(_closure);
            environment.Define("this", instance);
            return new PascalFunction(Declaration, environment, _isInitializer);
        }

        public int Arity()
        {
            return Declaration.Parameters.Count;
        }

        public bool IsMatch(List<string> args)
        {
            if (args.Count != Declaration.ParamTypes.Count) return false;

            for (int i = 0; i < Declaration.ParamTypes.Count; i++)
            {
                var expected = Declaration.ParamTypes[i].Lexeme;
                var actual = args[i];

                if (string.Equals(expected, "any", StringComparison.OrdinalIgnoreCase)) continue;
                if (!string.Equals(expected, actual, StringComparison.OrdinalIgnoreCase))
                {
                    if (!IsAssignable(expected, actual)) return false;
                }
            }

            return true;
        }

        private bool IsAssignable(string c1, string c2)
        {
            var parent = TypeChecker.Lookup.Parents.GetType(c2);
            while (parent != null)
            {
                if (string.Equals(c1, parent, StringComparison.OrdinalIgnoreCase)) return true;
                parent = TypeChecker.Lookup.Parents.GetType(parent);
            }
            return false;
        }

        public PascalFunction Match(List<string> args)
        {
            if (IsMatch(args)) return this;

            foreach (var fun in Overloads)
            {
                if (fun.IsMatch(args)) return fun;
            }

            return null;
        }

        public object Call(Interpreter interpreter, List<object> arguments)
        {
            var environment = new Environment(_closure);

            for (int i = 0; i < Declaration.Parameters.Count; i++)
            {
                environment.Define(Declaration.Parameters[i].Lexeme, arguments[i]);
            }

            try
            {
                interpreter.ExecuteBlock(Declaration.Body, environment);
            }
            catch (Return returnValue)
            {
                return _isInitializer ? _closure.GetAt(0, "this") : returnValue.Value;
            }

            return _isInitializer ? _closure.GetAt(0, "this") : null;
        }

        public override string ToString()
        {
            return $"<fn {Declaration.Name.Lexeme}>";
        }
    }
}
