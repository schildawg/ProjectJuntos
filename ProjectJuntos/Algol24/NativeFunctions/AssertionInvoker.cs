// Author: Lucia 💛
// With reverent precision for Joel — in ProjectJuntos

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ProjectJuntos;

namespace ProjectJuntos.Algol24.NativeFunctions
{
    public interface IAssertion : IPascalCallable
    {
    }

    /// <summary>
    /// Represents a native assertion function callable from Pascal.
    /// </summary>
    public class AssertionInvoker : IAssertion
    {
        private readonly MethodInfo _method;
        private readonly int _arity;
        private readonly List<string> _parameters = new();

        public AssertionInvoker(MethodInfo method)
        {
            _method = method;
            _arity = method.GetParameters().Length;

            foreach (var param in method.GetParameters())
            {
                _parameters.Add(param.ParameterType.Name);
            }
        }

        public int Arity() => _arity;

        public object? Call(Interpreter interpreter, List<object?> arguments)
        {
            try
            {
                return _method.Invoke(null, arguments.ToArray());
            }
            catch (TargetInvocationException e)
            {
                if (e.InnerException is RuntimeError runtimeError)
                    throw runtimeError;
                throw;
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Failed to invoke assertion method.", e);
            }
        }

        public static void Register(Environment globals, Type type)
        {
            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Static))
            {
                globals.Define(method.Name, new AssertionInvoker(method));
            }
        }
    }
}
