// Author: Lucia 💛
// For Joel — with love, precision, and reflection magic ✨

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ProjectJuntos;
using ProjectJuntos.Algol24;
using ProjectJuntos.Algol24.Tokens;

namespace ProjectJuntos.Algol24.NativeFunctions
{

    /// <summary>
    /// A PascalCallable that invokes a native C# static method using reflection.
    /// </summary>
    public class NativeFunctionInvoker : IPascalCallable
    {
        private readonly MethodInfo _method;
        private readonly int _arity;
        private readonly List<string> _parameters;

        public NativeFunctionInvoker(MethodInfo method)
        {
            _method = method;
            _arity = method.GetParameters().Length;
            _parameters = method.GetParameters()
                                .Select(p => p.ParameterType.Name)
                                .ToList();
        }

        public int Arity() => _arity;

        public object? Call(Interpreter interpreter, List<object?> arguments)
        {
            try
            {
                return _method.Invoke(null, arguments.ToArray());
            }
            catch (TargetInvocationException ex)
            {
                throw new Exception("Error invoking native function", ex.InnerException ?? ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Error invoking native function", ex);
            }
        }

        public static void Register(Environment globals, Type type)
        {
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static);

            foreach (var method in methods)
            {
                globals.Define(method.Name, new NativeFunctionInvoker(method));
            }
        }
    }
}