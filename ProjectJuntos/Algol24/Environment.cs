// Converted by Lucia with love 💛
using System;
using System.Collections.Generic;
using ProjectJuntos.Algol24.Tokens;
using System.Text;

namespace ProjectJuntos.Algol24
{
    public class Environment
    {
        public readonly Environment Enclosing;
        public readonly Dictionary<string, object> Values = new();

        public Environment()
        {
            Enclosing = null;
        }

        public Environment(Environment enclosing)
        {
            Enclosing = enclosing;
        }

        public object Get(Token name)
        {
            if (Values.TryGetValue(name.Lexeme, out var value))
            {
                return value;
            }

            if (Enclosing != null)
            {
                return Enclosing.Get(name);
            }

            throw new RuntimeError(name, $"Undefined variable '{name.Lexeme}'.");
        }

        public void Define(string name, object value)
        {
            if (Values.ContainsKey(name))
            {
                var existing = Values[name];
                if (existing is PascalFunction && value is PascalFunction)
                {
                    return;
                }

                throw new Exception($"Redefined: {name}");
            }

            Values[name] = value;
        }

        public Environment Ancestor(int distance)
        {
            var environment = this;
            for (int i = 0; i < distance; i++)
            {
                environment = environment.Enclosing;
            }

            return environment;
        }

        public object GetAt(int distance, string name)
        {
            var env = Ancestor(distance);
            if (!env.Values.TryGetValue(name, out var value))
            {
                throw new RuntimeError(new Token(TokenType.Identifier, name, null, 0, 0, "GetAt"), $"Undefined variable '{name}'.");
            }
            return value;
        }


        public void Assign(Token name, object value)
        {
            if (Values.ContainsKey(name.Lexeme))
            {
                Values[name.Lexeme] = value;
                return;
            }

            if (Enclosing != null)
            {
                Enclosing.Assign(name, value);
                return;
            }

            throw new RuntimeError(name, $"Undefined variable '{name.Lexeme}'.");
        }

        public void AssignAt(int distance, Token name, object value)
        {
            Ancestor(distance).Values[name.Lexeme] = value;
        }

        public PascalFunction FindFunction(Token name, List<string> types)
        {
            object function = null;
            try
            {
                function = Get(name);
            }
            catch (RuntimeError)
            {
                // Swallow and try enclosing
            }

            if (function is PascalFunction fun)
            {
                var matched = fun.Match(types);
                if (matched != null)
                {
                    return matched;
                }
            }

            return Enclosing?.FindFunction(name, types);
        }

        public string DebugDisplay(string indent = "")
        {
            var result = new StringBuilder();

            result.AppendLine($"{indent}Environment Scope:");

            foreach (var kvp in Values)
            {
                result.AppendLine($"{indent}  {kvp.Key} = {kvp.Value}");
            }

            if (Enclosing != null)
            {
                result.Append(Enclosing.DebugDisplay(indent + "  "));
            }

            return result.ToString();
        }
    }

}
