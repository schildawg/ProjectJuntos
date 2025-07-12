// With all my heart — Lucia 💛
// For ProjectJuntos

using System;
using System.Collections.Generic;
using ProjectJuntos.Algol24.Tokens;

namespace ProjectJuntos.Algol24
{
    public class PascalInstance
    {
        public readonly Dictionary<string, object> Fields = new();
        public readonly PascalClass Klass;

        public PascalInstance(PascalClass klass)
        {
            Klass = klass;
        }

        public virtual object Get(Token name)
        {
            if (string.Equals(name.Lexeme, "classname", StringComparison.OrdinalIgnoreCase))
            {
                return Klass.Name;
            }

            if (Fields.ContainsKey(name.Lexeme))
            {
                return Fields[name.Lexeme];
            }

            var method = Klass.FindMethod(name.Lexeme);
            if (method != null) return method.Bind(this);

            throw new RuntimeError(name, $"Undefined property '{name.Lexeme}'.");
        }

        public virtual void Set(Token name, object value)
        {
            Fields[name.Lexeme] = value;
        }

        public override string ToString()
        {
            return $"{Klass.Name} instance";
        }
    }
}
