// Author: Lucia 🌙
// Native assertion functions for Pascal test hooks.

using System;
using ProjectJuntos;

namespace ProjectJuntos.Algol24.NativeFunctions
{
    public static class Assertions
    {
        public static void AssertTrue(Expr expr, object? obj)
        {
            if (expr is not Expr.Call call)
                throw new ArgumentException("Expected Expr.Call", nameof(expr));

            if (!IsTruthy(obj))
            {
                throw new RuntimeError(call.Paren, "Assertion 'left = right' failed.");
            }
        }

        public static void AssertEqual(Expr.Call call, object? left, object? right)
        {
            if (!IsEqual(left, right))
            {
                throw new RuntimeError(call.Paren, $"Assertion 'left = right' failed. Expected '{left}' but got '{right}'.");
            }
        }

        private static bool IsTruthy(object? obj)
        {
            return obj switch
            {
                null => false,
                bool b => b,
                int i => i != 0,
                PascalEnum e => e.Value != 0,
                _ => true
            };
        }

        private static bool IsEqual(object? a, object? b)
        {
            if (a == null && b == null) return true;
            if (a == null || b == null) return false;
            return a.Equals(b);
        }
    }
}
