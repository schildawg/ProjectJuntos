// Author: Lucia 💛
// For Joel — faithfully ported to ProjectJuntos

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ProjectJuntos.Algol24.Tokens;

namespace ProjectJuntos.Algol24
{

    public static class Pascal
    {
        public static bool HadError = false;
        public static bool HadRuntimeError = false;
        public static string? LastError;

        private static readonly Interpreter interpreter = new();

        public static void Main(string[] args)
        {
            if (args.Length > 1)
            {
                Console.WriteLine("Usage: jpascal [script]");
            }
            else if (args.Length == 1)
            {
                RunFile(args[0]);
            }
            else
            {
                RunPrompt();
            }
        }

        private static void RunFile(string path)
        {
            string source = File.ReadAllText(path, Encoding.Default);
            Run(source);

            if (HadError) System.Environment.Exit(65);
            if (HadRuntimeError) System.Environment.Exit(70);
        }

        private static void RunPrompt()
        {
            while (true)
            {
                Console.Write("> ");
                var line = Console.ReadLine();
                if (line == null) break;
                Run(line);
            }
        }

        private static void Run(string source)
        {
            var scanner = new Scanner("REPL", source);
            var tokens = scanner.ScanTokens();

            var parser = new Parser(tokens);
            List<Stmt> statements = parser.ParseWithError();

            if (HadError)
            {
                ConsoleColorUtil.Info(ConsoleColorUtil.Bar);
                //ConsoleColorUtil.Info(ConsoleColorUtil.Red("BUILD FAILED"));
                ConsoleColorUtil.Info(ConsoleColorUtil.Bar);
                return;
            }

            var resolver = new Resolver(interpreter);
            resolver.Resolve(statements);

            try
            {
                var typeChecker = new TypeChecker();
                typeChecker.Resolve(statements);
            }
            catch (RuntimeError e)
            {
                ConsoleColorUtil.Error(e);
                HadError = true;
            }

            if (HadError) return;

            //interpreter.RunTests(statements);
            interpreter.Interpret(statements);
        }

        public static void Error(int line, string message)
        {
            Report(line, "", message);
        }

        private static void Report(int line, string where, string message)
        {
            LastError = $"[line {line}] Error{where}: {message}";
            HadError = true;
        }

        public static void Error(Token token, string message)
        {
            if (token.Type == TokenType.Eof)
            {
                Report(token.Line, " at end", message);
            }
            else
            {
                Report(token.Line, $" at '{token.Lexeme}'", message);
            }
        }

        public static void RuntimeError(RuntimeError error)
        {
            HadRuntimeError = true;
        }

        public static void Reset()
        {
            HadError = false;
            HadRuntimeError = false;
            LastError = null;
        }
    }
}
