using System;
using System.Collections.Generic;
using Xunit;
using ProjectJuntos;
using ProjectJuntos.Algol24;

namespace ProjectJuntos.Tests
{
    /// <summary>
    /// Tests PascalFunction.
    /// </summary>
    public class PascalFunctionTest
    {
        private PascalFunction MakeFunction()
        {
            var code = @"
                function fib(n);
                begin
                    if n < 2 then exit n;
                    exit fib(n - 1) + fib(n - 2);
                end";

            var scanner = new Scanner("test", code);
            var tokens = scanner.ScanTokens();

            var parser = new Parser(tokens, false);
            var stmts = parser.Parse();

            return new PascalFunction((Stmt.Function)stmts[0], new ProjectJuntos.Algol24.Environment(), false);
        }

        // Tests creating a new Function.
        [Fact]
        public void TestCreate()
        {
            var function = MakeFunction();
            Assert.Equal(1, function.Arity());
        }

        // Tests PascalFunction.ToString()
        [Fact]
        public void TestToString()
        {
            var function = MakeFunction();
            Assert.Equal("<fn fib>", function.ToString());
        }

        // Tests calling a function
        [Fact]
        public void TestCall()
        {
            var code = @"
                function fib(n);
                begin
                    if n < 2.0 then exit n;
                    exit fib(n - 1.0) + fib(n - 2.0);
                end";

            var scanner = new Scanner("test", code);
            var tokens = scanner.ScanTokens();

            var parser = new Parser(tokens, false);
            var stmts = parser.Parse();

            var interpreter = new Interpreter();
            var resolver = new Resolver(interpreter);
            resolver.Resolve(stmts);

            var function = new PascalFunction((Stmt.Function)stmts[0], interpreter.Globals, false);

            interpreter.Globals.Define("fib", function);

            var args = new List<object> { 7.0 };

            function.Call(interpreter, args);
        }
    }
}
