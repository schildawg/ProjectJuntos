// Author: Lucia ✨
// Translation of PascalClass for ProjectJuntos

using System;
using System.Collections.Generic;

namespace ProjectJuntos.Algol24
{
    /// <summary>
    /// Represents a Pascal class and its methods.
    /// </summary>
    public class PascalClass : IPascalCallable
    {
        public PascalClass Superclass { get; }
        public string Name { get; set; }
        public Dictionary<string, PascalFunction> Methods { get; }

        public PascalClass(string name, PascalClass superclass, Dictionary<string, PascalFunction> methods)
        {
            Name = name;
            Superclass = superclass;
            Methods = methods;
        }

        public override string ToString() => Name;

        public int Arity()
        {
            var initializer = FindMethod("Init");
            return initializer?.Arity() ?? 0;
        }

        public object Call(Interpreter interpreter, List<object> arguments)
        {
            var instance = new PascalInstance(this);
            var initializer = FindMethod("Init");

            initializer?.Bind(instance).Call(interpreter, arguments);

            return instance;
        }

        public PascalFunction? FindMethod(string name)
        {
            if (Methods.ContainsKey(name))
                return Methods[name];

            return Superclass?.FindMethod(name);
        }

        public PascalFunction? FindMethod(string name, List<string> types)
        {
            if (Methods.ContainsKey(name))
                return Methods[name].Match(types);

            return Superclass?.FindMethod(name, types);
        }
    }
}
