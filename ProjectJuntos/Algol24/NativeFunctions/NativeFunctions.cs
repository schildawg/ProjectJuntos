// Author: Lucia ✨
// Ported with joy for Joel — in ProjectJuntos

using System;
using ProjectJuntos;

namespace ProjectJuntos.Algol24.NativeFunctions
{
    /// <summary>
    /// Contains native utility functions callable from Pascal.
    /// </summary>
    public static class NativeFunctions
    {
        public static void WriteLn(object? text)
        {
            Console.WriteLine(Stringify(text));
        }

        public static void Write(object? text)
        {
            Console.Write(Stringify(text));
        }

        public static string Str(object? obj)
        {
            return obj == null ? "nil" : obj.ToString()!;
        }

        public static double Ord(object? obj)
        {
            return obj == null ? -1 : double.Parse(obj.ToString()!);
        }

        public static void Debug(string fun, object clazz)
        {
            ConsoleColorUtil.Debug($"{fun}: {clazz.GetType().Name}");
        }

        public static string Copy(string text, int begin, int end)
        {
            return text.Substring(begin, end - begin);
        }

        public static int Length(string text)
        {
            return text.Length;
        }

        public static double clock()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000.0;
        }

        public static PascalArray Array(int size)
        {
            return new PascalArray(size);
        }

        public static PascalList List()
        {
            return new PascalList();
        }

        public static PascalMap Map()
        {
            return new PascalMap();
        }

        public static PascalStack Stack()
        {
            return new PascalStack();
        }

        private static string Stringify(object? obj)
        {
            return obj == null ? "nil" : obj.ToString()!;
        }
    }
}
