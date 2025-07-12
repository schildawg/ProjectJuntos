// Author: Lucia ✨
// ConsoleColorUtil for ProjectJuntos — faithful to Joel's original design

using System;
using ProjectJuntos.Algol24;

namespace ProjectJuntos
{

    public static class ConsoleColorUtil
    {
        public const string Bar = "------------------------------------------------------------------------";

        public static void Header(string name)
        {
            int length = Bar.Length - 4 - name.Length;
            int side = length / 2;
            Info(new string('-', side) + "[ " + Color(name, ConsoleColor.Cyan) + " ]" + new string('-', side));
        }

        public static void Subheader(string name)
        {
            Info("< " + Color(name, ConsoleColor.Cyan) + " >");
        }

        public static void Info(string text)
        {
            WriteTag("INFO", ConsoleColor.Blue, text);
        }

        public static void Success(string name)
        {
            int length = Bar.Length - 10 - name.Length;
            Info(name + " " + new string('.', length) + Color(" SUCCESS", ConsoleColor.Green));
        }

        public static void Fail(string name)
        {
            int length = Bar.Length - 10 - name.Length;
            Info(name + " " + new string('.', length) + Color(" FAILED", ConsoleColor.Red));
        }

        public static void Error(RuntimeError err)
        {
            int line = err.Token.Line;
            string file = err.Token.FileName ?? "test";

            string text = SourceCode.Instance.GetLine(file, line); // assumed implemented
            int offset = err.Token.Offset;
            int lexemeLength = err.Token.Lexeme?.Length ?? 1;
            int lineLength = line.ToString().Length;

            WriteTag("ERROR", ConsoleColor.Red, $"{file}: {err.Message}");
            WriteTag("ERROR", ConsoleColor.Red, $"{line} ║ {text}");
            WriteRaw(new string(' ', lineLength) + " ║" + Color(new string(' ', offset + 1) + new string('^', lexemeLength), ConsoleColor.Red));
        }

        public static void Debug(string text)
        {
            WriteTag("DEBUG", ConsoleColor.Yellow, text);
        }

        private static void WriteTag(string tag, ConsoleColor tagColor, string message)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("[");
            Console.ForegroundColor = tagColor;
            Console.Write(tag);
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("] ");
            Console.ResetColor();
            Console.WriteLine(message);
        }

        private static void WriteRaw(string message)
        {
            Console.WriteLine(message);
        }

        private static string Color(string text, ConsoleColor color)
        {
            var prev = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.Write(text);
            Console.ForegroundColor = prev;
            return "";
        }

        public static string Green(string message) =>
              $"{Color(ConsoleColor.Green)}{message}{Reset()}";

        public static string Red(string message) =>
            $"{Color(ConsoleColor.Red)}{message}{Reset()}";

        public static string Blue(string message) =>
            $"{Color(ConsoleColor.Blue)}{message}{Reset()}";

        public static string Yellow(string message) =>
            $"{Color(ConsoleColor.Yellow)}{message}{Reset()}";

        public static string Cyan(string message) =>
            $"{Color(ConsoleColor.Cyan)}{message}{Reset()}";

        public static string White(string message) =>
            $"{Color(ConsoleColor.White)}{message}{Reset()}";

        private static string Color(ConsoleColor color)
        {
            return color switch
            {
                ConsoleColor.Black => "\u001b[30m",
                ConsoleColor.Red => "\u001b[31m",
                ConsoleColor.Green => "\u001b[32m",
                ConsoleColor.Yellow => "\u001b[33m",
                ConsoleColor.Blue => "\u001b[34m",
                ConsoleColor.Magenta => "\u001b[35m",
                ConsoleColor.Cyan => "\u001b[36m",
                ConsoleColor.White => "\u001b[37m",
                _ => ""
            };
        }

        private static string Reset() => "\u001b[0m";

    }
}
