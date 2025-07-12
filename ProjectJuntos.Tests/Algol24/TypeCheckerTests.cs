using System;
using System.Collections.Generic;
using Xunit;
using ProjectJuntos;
using ProjectJuntos.Algol24;
using ProjectJuntos.Algol24.Tokens;

namespace ProjectJuntos.Tests
{
    public class TypeCheckerTests
    {
        private List<Stmt> ParseStmts(string source)
        {
            var scanner = new Scanner("test", source);
            var tokens = scanner.ScanTokens();
            var parser = new Parser(tokens, false);
            return parser.Parse();
        }

        [Fact]
        public void TestStringType()
        {
            var interpreter = new Interpreter(new TestErrorHandler());
            var stmts = ParseStmts("var Abc : String := 'Abc';");
            var checker = new TypeChecker();
            checker.Resolve(stmts);
            interpreter.Interpret(stmts);
            var result = interpreter.Globals.Get(new Token(TokenType.Identifier, "Abc", "", 0, 0, "test"));
            Assert.Equal("Abc", result);
        }

        [Fact]
        public void TestStringTypeMismatch()
        {
            var stmts = ParseStmts("var Abc : String := 123;");
            var ex = Assert.Throws<RuntimeError>(() => {
                var checker = new TypeChecker();
                checker.Resolve(stmts);
            });
            Assert.Equal("Type mismatch!", ex.Message);
        }

      [Fact]
        public void TestIntegerType()
        {
            var interpreter = new Interpreter(new TestErrorHandler());
            var stmts = ParseStmts("var Abc : Integer := 123;");

            var checker = new TypeChecker();
            checker.Resolve(stmts);
            interpreter.Interpret(stmts);

            var result = interpreter.Globals.Get(new Token(TokenType.Identifier, "Abc", "", 0, 0, "test"));
            Assert.Equal(123, result);
        }

        [Fact]
        public void TestIntegerTypeMismatch()
        {
            var stmts = ParseStmts("var Abc : Integer := True;");
            var ex = Assert.Throws<RuntimeError>(() =>
            {
                var checker = new TypeChecker();
                checker.Resolve(stmts);
            });
            Assert.Equal("Type mismatch!", ex.Message);
        }

                [Fact]
        public void TestBooleanType()
        {
            var interpreter = new Interpreter(new TestErrorHandler());
            var stmts = ParseStmts("var Abc : Boolean := True;");

            var checker = new TypeChecker();
            checker.Resolve(stmts);
            interpreter.Interpret(stmts);

            var result = interpreter.Globals.Get(new Token(TokenType.Identifier, "Abc", "", 0, 0, "test"));
            Assert.Equal(true, result);
        }

        [Fact]
        public void TestBooleanTypeMismatch()
        {
            var stmts = ParseStmts("var Abc : Boolean := 'ABC';");
            var ex = Assert.Throws<RuntimeError>(() =>
            {
                var checker = new TypeChecker();
                checker.Resolve(stmts);
            });
            Assert.Equal("Type mismatch!", ex.Message);
        }

        [Fact]
        public void TestCharType()
        {
            var interpreter = new Interpreter(new TestErrorHandler());
            var stmts = ParseStmts("var Abc : Char := 'J';");

            var checker = new TypeChecker();
            checker.Resolve(stmts);
            interpreter.Interpret(stmts);

            var result = interpreter.Globals.Get(new Token(TokenType.Identifier, "Abc", "", 0, 0, "test"));
            Assert.Equal('J', result);
        }

        [Fact]
        public void TestAltCharType()
        {
            var interpreter = new Interpreter(new TestErrorHandler());
            var stmts = ParseStmts("var Abc : Char := #74;");

            var checker = new TypeChecker();
            checker.Resolve(stmts);
            interpreter.Interpret(stmts);

            var result = interpreter.Globals.Get(new Token(TokenType.Identifier, "Abc", "", 0, 0, "test"));
            Assert.Equal('J', result);
        }

        [Fact]
        public void TestCharTypeMismatch()
        {
            var stmts = ParseStmts("var Abc : Char := False;");
            var ex = Assert.Throws<RuntimeError>(() =>
            {
                var checker = new TypeChecker();
                checker.Resolve(stmts);
            });
            Assert.Equal("Type mismatch!", ex.Message);
        }

        [Fact]
        public void TestDoubleType()
        {
            var interpreter = new Interpreter(new TestErrorHandler());
            var stmts = ParseStmts("var Abc : Double := 123.0;");

            var checker = new TypeChecker();
            checker.Resolve(stmts);
            interpreter.Interpret(stmts);

            var result = interpreter.Globals.Get(new Token(TokenType.Identifier, "Abc", "", 0, 0, "test"));
            Assert.Equal(123.0, result);
        }

        [Fact]
        public void TestDoubleTypeMismatch()
        {
            var stmts = ParseStmts("var Abc : Double := False;");
            var ex = Assert.Throws<RuntimeError>(() =>
            {
                var checker = new TypeChecker();
                checker.Resolve(stmts);
            });
            Assert.Equal("Type mismatch!", ex.Message);
        }

        [Fact]
        public void TestEnumType()
        {
            var interpreter = new Interpreter(new TestErrorHandler());
            var stmts = ParseStmts(@"
               type Dual = (No, Yes);
               var Abc : Dual := Yes;
               ");

            var checker = new TypeChecker();
            checker.Resolve(stmts);
            interpreter.Interpret(stmts);

            var result = interpreter.Globals.Get(new Token(TokenType.Identifier, "Abc", "", 0, 0, "test"));
            Assert.Equal("Yes", result.ToString());
        }

        [Fact]
        public void TestEnumTypeMismatch()
        {
            var stmts = ParseStmts(@"
               type Dual = (No, Yes);
               var Abc : Dual := 5;
               ");

            var ex = Assert.Throws<RuntimeError>(() =>
            {
                var checker = new TypeChecker();
                checker.Resolve(stmts);
            });
            Assert.Equal("Type mismatch!", ex.Message);
        }

        [Fact]
        public void TestDifferentEnumTypeMismatch()
        {
            var stmts = ParseStmts(@"
               type Dual = (No, Yes);
               type Dual2 = (On, Off);
               var Abc : Dual := Off;
               ");

            var ex = Assert.Throws<RuntimeError>(() =>
            {
                var checker = new TypeChecker();
                checker.Resolve(stmts);
            });
            Assert.Equal("Type mismatch!", ex.Message);
        }

        [Fact]
        public void TestClassType()
        {
            var interpreter = new Interpreter(new TestErrorHandler());
            var stmts = ParseStmts(@"
               class Item;
               begin
               end

               var Abc : Item := Item();
               ");

            var checker = new TypeChecker();
            checker.Resolve(stmts);
            interpreter.Interpret(stmts);

            var result = interpreter.Globals.Get(new Token(TokenType.Identifier, "Abc", "", 0, 0, "test"));
            Assert.Equal("Item instance", result.ToString());
        }

        [Fact]
        public void TestClassTypeMismatch()
        {
            var stmts = ParseStmts(@"
               class Item;
               begin
               end

               var Abc : Item := 8;
               ");

            var ex = Assert.Throws<RuntimeError>(() =>
            {
                var checker = new TypeChecker();
                checker.Resolve(stmts);
            });
            Assert.Equal("Type mismatch!", ex.Message);
        }

        [Fact]
        public void TestDifferentClassTypeMismatch()
        {
            var stmts = ParseStmts(@"
               class Item;
               begin
               end

               class Room;
               begin
               end

               var Abc : Item := Room();
               ");

            var ex = Assert.Throws<RuntimeError>(() =>
            {
                var checker = new TypeChecker();
                checker.Resolve(stmts);
            });
            Assert.Equal("Type mismatch!", ex.Message);
        }

        [Fact]
        public void TestAnyType()
        {
            var interpreter = new Interpreter(new TestErrorHandler());
            var stmts = ParseStmts("var Abc : Any := 123;");

            var checker = new TypeChecker();
            checker.Resolve(stmts);
            interpreter.Interpret(stmts);

            var result = interpreter.Globals.Get(new Token(TokenType.Identifier, "Abc", "", 0, 0, "test"));
            Assert.Equal(123, result);
        }

        [Fact]
        public void TestAssignFunction()
        {
            var interpreter = new Interpreter(new TestErrorHandler());
            var stmts = ParseStmts(@"
               function DoSomething : String;
               begin
                  Exit 'ABC';
               end

               var Abc : String := DoSomething();
               ");

            var checker = new TypeChecker();
            checker.Resolve(stmts);
            interpreter.Interpret(stmts);

            var result = interpreter.Globals.Get(new Token(TokenType.Identifier, "Abc", "", 0, 0, "test"));
            Assert.Equal("ABC", result.ToString());
        }

        [Fact]
        public void TestAssignFunctionTypeMismatch()
        {
            var stmts = ParseStmts(@"
               function DoSomething : String;
               begin
                  Exit 'ABC';
               end

               var Abc : Integer := DoSomething();
               ");

            var ex = Assert.Throws<RuntimeError>(() =>
            {
                var checker = new TypeChecker();
                checker.Resolve(stmts);
            });

            Assert.Equal("Type mismatch!", ex.Message);
        }

        [Fact]
        public void TestAssignMethod()
        {
            var interpreter = new Interpreter(new TestErrorHandler());
            var stmts = ParseStmts(@"
               class Item;
               begin
                  function DoSomething : String;
                  begin
                     Exit 'ABC';
                  end
               end

               var Thing := Item();
               var Abc : String := Thing.DoSomething();
               ");

            var checker = new TypeChecker();
            checker.Resolve(stmts);
            interpreter.Interpret(stmts);

            var result = interpreter.Globals.Get(new Token(TokenType.Identifier, "Abc", "", 0, 0, "test"));
            Assert.Equal("ABC", result.ToString());
        }

        [Fact]
        public void TestAssignLogicalExpression()
        {
            var interpreter = new Interpreter(new TestErrorHandler());
            var stmts = ParseStmts("var Abc : Boolean := True or False;");

            var checker = new TypeChecker();
            checker.Resolve(stmts);
            interpreter.Interpret(stmts);

            var result = interpreter.Globals.Get(new Token(TokenType.Identifier, "Abc", "", 0, 0, "test"));
            Assert.Equal(true, result);
        }


        [Fact]
        public void TestAssignLogicalTypeMismatch()
        {
            var stmts = ParseStmts("var Abc : Boolean := True or 1;");

            var ex = Assert.Throws<RuntimeError>(() =>
            {
                var checker = new TypeChecker();
                checker.Resolve(stmts);
            });

            Assert.Equal("Type mismatch.", ex.Message);
        }

        [Fact]
        public void TestIntegerArithmetic()
        {
            var interpreter = new Interpreter(new TestErrorHandler());
            var stmts = ParseStmts("var Abc : Integer := 1 + 1;");

            var checker = new TypeChecker();
            checker.Resolve(stmts);
            interpreter.Interpret(stmts);

            var result = interpreter.Globals.Get(new Token(TokenType.Identifier, "Abc", "", 0, 0, "test"));
            Assert.Equal(2, result);
        }

        [Fact]
        public void TestBinaryTypeMismatch()
        {
            var stmts = ParseStmts("var Abc : Boolean := 1 + false;");

            var ex = Assert.Throws<RuntimeError>(() =>
            {
                var checker = new TypeChecker();
                checker.Resolve(stmts);
            });

            Assert.Equal("Type mismatch.", ex.Message);
        }

        [Fact]
        public void TestIntegerUnary()
        {
            var interpreter = new Interpreter(new TestErrorHandler());
            var stmts = ParseStmts("var Abc : Integer := -1;");

            var checker = new TypeChecker();
            checker.Resolve(stmts);
            interpreter.Interpret(stmts);

            var result = interpreter.Globals.Get(new Token(TokenType.Identifier, "Abc", "", 0, 0, "test"));
            Assert.Equal(-1, result);
        }

        [Fact]
        public void TestBooleanUnary()
        {
            var interpreter = new Interpreter(new TestErrorHandler());
            var stmts = ParseStmts("var Abc : Boolean := Not True;");

            var checker = new TypeChecker();
            checker.Resolve(stmts);
            interpreter.Interpret(stmts);

            var result = interpreter.Globals.Get(new Token(TokenType.Identifier, "Abc", "", 0, 0, "test"));
            Assert.Equal(false, result);
        }

        [Fact]
        public void TestInvalidAssign()
        {
            var stmts = ParseStmts(@"
                var Abc : String := 'ABC';
                Abc := 42;
            ");

            var ex = Assert.Throws<RuntimeError>(() =>
            {
                var checker = new TypeChecker();
                checker.Resolve(stmts);
            });

            Assert.Equal("Type mismatch!", ex.Message);
        }

        [Fact]
        public void TestInvalidExit()
        {
            var stmts = ParseStmts(@"
                function DoSomething() : String;
                begin
                    Exit 123;
                end
            ");

            var ex = Assert.Throws<RuntimeError>(() =>
            {
                var checker = new TypeChecker();
                checker.Resolve(stmts);
            });

            Assert.Equal("Type mismatch!", ex.Message);
        }

        [Fact]
        public void TestProcedureReturningValue()
        {
            var stmts = ParseStmts(@"
                procedure DoSomething();
                begin
                    Exit 123;
                end
            ");

            var ex = Assert.Throws<RuntimeError>(() =>
            {
                var checker = new TypeChecker();
                checker.Resolve(stmts);
            });

            Assert.Equal("Can't return value from procedure.", ex.Message);
        }

        [Fact]
        public void TestVariableSection()
        {
            var interpreter = new Interpreter(new TestErrorHandler());

            var stmts = ParseStmts(@"
                procedure DoSomething();
                var
                    A, B, C : Integer := 5;
                begin
                    A := 1;
                end

                DoSomething();
            ");

            var checker = new TypeChecker();
            checker.Resolve(stmts);

            var resolver = new Resolver(interpreter);
            resolver.Resolve(stmts);

            interpreter.Interpret(stmts);
        }

        [Fact]
        public void TestSubscriptNoGeneric()
        {
            var stmts = ParseStmts(@"
                var Tokens : List := List();
                Tokens.Add('ABC');

                var Token : String := Tokens[0];
            ");

            var ex = Assert.Throws<RuntimeError>(() =>
            {
                var checker = new TypeChecker();
                checker.Resolve(stmts);
            });

            Assert.Equal("Type mismatch!", ex.Message);
        }

        [Fact]
        public void TestSubscriptWithGeneric()
        {
            var interpreter = new Interpreter(new TestErrorHandler());

            var stmts = ParseStmts(@"
                var Tokens : List of String := List();
                Tokens.Add('ABC');

                var Token : String := Tokens[0];
            ");

            var checker = new TypeChecker();
            checker.Resolve(stmts);

            interpreter.Interpret(stmts);
        }

        [Fact]
        public void TestAssignSubclass()
        {
            var interpreter = new Interpreter(new TestErrorHandler());

            var stmts = ParseStmts(@"
                class Animal;
                begin
                end

                class Dog (Animal);
                begin
                end

                var Pet : Animal := Dog();
            ");

            var checker = new TypeChecker();
            checker.Resolve(stmts);

            interpreter.Interpret(stmts);
        }
    }
}
