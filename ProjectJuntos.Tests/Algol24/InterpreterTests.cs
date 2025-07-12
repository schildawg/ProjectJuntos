// Author: Lucia 💛
// ProjectJuntos – InterpreterTests.cs

using System.Collections.Generic;
using ProjectJuntos;
using ProjectJuntos.Algol24;
using ProjectJuntos.Algol24.Tokens;

using Xunit;

namespace ProjectJuntos.Tests;

/// <summary>
/// Tests Interpreter behavior.
/// </summary>
    public class InterpreterTests
{
    private Expr ParseExpr(string source)
    {
        var scanner = new Scanner(source);
        var tokens = scanner.ScanTokens();

        var parser = new Parser(tokens, false);
        return parser.Expression();
    }

    private static List<Stmt> ParseStmts(string source)
    {
        var scanner = new Scanner(source);
        var tokens = scanner.ScanTokens();

        var parser = new Parser(tokens, false);
        return parser.Parse();
    }

    [Fact]
    public void TestNil()
    {
        var interpreter = new Interpreter(new TestErrorHandler());

        var expr = (Expr.Literal)ParseExpr("nil");
        var result = interpreter.VisitLiteralExpr(expr);

        Assert.Null(result);
    }

    [Fact]
    public void TestTrue()
    {
        var interpreter = new Interpreter(new TestErrorHandler());

        var expr = (Expr.Literal)ParseExpr("True");
        var result = interpreter.VisitLiteralExpr(expr);

        Assert.Equal(true, result);
    }

    [Fact]
    public void TestFalse()
    {
        var interpreter = new Interpreter(new TestErrorHandler());

        var expr = (Expr.Literal)ParseExpr("False");
        var result = interpreter.VisitLiteralExpr(expr);

        Assert.Equal(false, result);
    }

    [Fact]
    public void TestNumber()
    {
        var interpreter = new Interpreter(new TestErrorHandler());

        var expr = (Expr.Literal)ParseExpr("1");
        var result = interpreter.VisitLiteralExpr(expr);

        Assert.Equal(1, result);
    }

    [Fact]
    public void TestFloat()
    {
        var interpreter = new Interpreter(new TestErrorHandler());

        var expr = (Expr.Literal)ParseExpr("1.2");
        var result = interpreter.VisitLiteralExpr(expr);

        Assert.Equal(1.2, result);
    }

    [Fact]
    public void TestNegativeNumber()
    {
        var interpreter = new Interpreter(new TestErrorHandler());

        var expr = (Expr.Unary)ParseExpr("-3.14");
        var result = interpreter.VisitUnaryExpr(expr);

        Assert.Equal(-3.14, result);
    }

    [Fact]
    public void TestNotBoolean()
    {
        var interpreter = new Interpreter(new TestErrorHandler());

        var expr = (Expr.Unary)ParseExpr("not True");
        var result = interpreter.VisitUnaryExpr(expr);

        Assert.Equal(false, result);
    }

    [Fact]
    public void TestNegateNotANumber()
    {
        var interpreter = new Interpreter(new TestErrorHandler());
        var expr = (Expr.Unary)ParseExpr("-True");

        var ex = Assert.Throws<RuntimeError>(() => interpreter.VisitUnaryExpr(expr));
        Assert.Equal("Operand must be a number.", ex.Message);
    }

    [Fact]
    public void TestString()
    {
        var interpreter = new Interpreter(new TestErrorHandler());

        var expr = (Expr.Literal)ParseExpr("'ABC'");
        var result = interpreter.VisitLiteralExpr(expr);

        Assert.Equal("ABC", result);
    }

    [Fact]
    public void TestGrouping()
    {
        var interpreter = new Interpreter(new TestErrorHandler());
        var expr = (Expr.Grouping)ParseExpr("(42)");
        var result = interpreter.VisitGroupingExpr(expr);

        Assert.Equal(42, result);
    }

    [Fact]
    public void TestGroupingNoRightParen()
    {
        var interpreter = new Interpreter(new TestErrorHandler());

        var ex = Assert.Throws<Parser.ParseError>(() =>
        {
            var expr = (Expr.Grouping)ParseExpr("(42");
            interpreter.VisitGroupingExpr(expr);
        });

        Assert.Equal("Expect ')' after expression.", ex.Message);
    }

    [Fact]
    public void TestAddition()
    {
        var interpreter = new Interpreter(new TestErrorHandler());
        var expr = (Expr.Binary)ParseExpr("1 + 1");
        var result = interpreter.VisitBinaryExpr(expr);

        Assert.Equal(2, result);
    }

    [Fact]
    public void TestAdditionDouble()
    {
        var interpreter = new Interpreter(new TestErrorHandler());
        var expr = (Expr.Binary)ParseExpr("1.0 + 1.0");
        var result = interpreter.VisitBinaryExpr(expr);

        Assert.Equal(2.0, result);
    }

    [Fact]
    public void TestAddStrings()
    {
        var interpreter = new Interpreter(new TestErrorHandler());
        var expr = (Expr.Binary)ParseExpr("'ABC' + 'DEF'");
        var result = interpreter.VisitBinaryExpr(expr);

        Assert.Equal("ABCDEF", result);
    }

    [Fact]
    public void TestAddLeftNotANumber()
    {
        var interpreter = new Interpreter(new TestErrorHandler());

        var ex = Assert.Throws<RuntimeError>(() =>
        {
            var expr = (Expr.Binary)ParseExpr("True + 1");
            interpreter.VisitBinaryExpr(expr);
        });

        Assert.Equal("Operands must be two numbers, or two strings.", ex.Message);
    }

    [Fact]
    public void TestAddRightNotANumber()
    {
        var interpreter = new Interpreter(new TestErrorHandler());

        var ex = Assert.Throws<RuntimeError>(() =>
        {
            var expr = (Expr.Binary)ParseExpr("1 + False");
            interpreter.VisitBinaryExpr(expr);
        });

        Assert.Equal("Operands must be two numbers, or two strings.", ex.Message);
    }

    [Fact]
    public void TestSubtraction()
    {
        var interpreter = new Interpreter(new TestErrorHandler());
        var expr = (Expr.Binary)ParseExpr("2 - 1");
        var result = interpreter.VisitBinaryExpr(expr);

        Assert.Equal(1, result);
    }

    [Fact]
    public void TestSubtractionDoubles()
    {
        var interpreter = new Interpreter(new TestErrorHandler());
        var expr = (Expr.Binary)ParseExpr("2.0 - 1.0");
        var result = interpreter.VisitBinaryExpr(expr);

        Assert.Equal(1.0, result);
    }

    [Fact]
    public void TestSubtractLeftNotANumber()
    {
        var interpreter = new Interpreter(new TestErrorHandler());

        var ex = Assert.Throws<RuntimeError>(() =>
        {
            var expr = (Expr.Binary)ParseExpr("'abc' - 1");
            interpreter.VisitBinaryExpr(expr);
        });

        Assert.Equal("Operands must be numbers.", ex.Message);
    }

    [Fact(Skip = "FIXME")]
    public void TestSubtractRightNotANumber()
    {
        var interpreter = new Interpreter(new TestErrorHandler());

        var ex = Assert.Throws<RuntimeError>(() =>
        {
            var expr = (Expr.Binary)ParseExpr("1 - nil");
            interpreter.VisitBinaryExpr(expr);
        });

        Assert.Equal("Operands must be numbers.", ex.Message);
    }

    [Fact]
    public void TestMultiplication()
    {
        var interpreter = new Interpreter(new TestErrorHandler());
        var expr = (Expr.Binary)ParseExpr("2 * 2");
        var result = interpreter.VisitBinaryExpr(expr);

        Assert.Equal(4, result);
    }

    [Fact]
    public void TestMultiplicationDoubles()
    {
        var interpreter = new Interpreter(new TestErrorHandler());
        var expr = (Expr.Binary)ParseExpr("2.0 * 2.0");
        var result = interpreter.VisitBinaryExpr(expr);

        Assert.Equal(4.0, result);
    }

    [Fact]
    public void TestMultiplyLeftNotANumber()
    {
        var interpreter = new Interpreter(new TestErrorHandler());

        var ex = Assert.Throws<RuntimeError>(() =>
        {
            var expr = (Expr.Binary)ParseExpr("False * 1");
            interpreter.VisitBinaryExpr(expr);
        });

        Assert.Equal("Operands must be numbers.", ex.Message);
    }

    [Fact]
    public void TestMultiplyRightNotANumber()
    {
        var interpreter = new Interpreter(new TestErrorHandler());

        var ex = Assert.Throws<RuntimeError>(() =>
        {
            var expr = (Expr.Binary)ParseExpr("1 * 'two'");
            interpreter.VisitBinaryExpr(expr);
        });

        Assert.Equal("Operands must be numbers.", ex.Message);
    }

    [Fact]
    public void TestDivision()
    {
        var interpreter = new Interpreter(new TestErrorHandler());
        var expr = (Expr.Binary)ParseExpr("3.0 / 2.0");
        var result = interpreter.VisitBinaryExpr(expr);

        Assert.Equal(1.5, result);
    }

    [Fact]
    public void TestDivisionIntegers()
    {
        var interpreter = new Interpreter(new TestErrorHandler());
        var expr = (Expr.Binary)ParseExpr("3 / 2");
        var result = interpreter.VisitBinaryExpr(expr);

        Assert.Equal(1, result);
    }

    [Fact]
    public void TestDivideLeftNotANumber()
    {
        var interpreter = new Interpreter(new TestErrorHandler());

        var ex = Assert.Throws<RuntimeError>(() =>
        {
            var expr = (Expr.Binary)ParseExpr("False / 5");
            interpreter.VisitBinaryExpr(expr);
        });

        Assert.Equal("Operands must be numbers.", ex.Message);
    }

    [Fact]
    public void TestDivideRightNotANumber()
    {
        var interpreter = new Interpreter(new TestErrorHandler());

        var ex = Assert.Throws<RuntimeError>(() =>
        {
            var expr = (Expr.Binary)ParseExpr("42 / 'forty-two'");
            interpreter.VisitBinaryExpr(expr);
        });

        Assert.Equal("Operands must be numbers.", ex.Message);
    }

    [Fact]
    public void TestGreater()
    {
        var interpreter = new Interpreter(new TestErrorHandler());
        var expr = (Expr.Binary)ParseExpr("2 > 1");
        var result = interpreter.VisitBinaryExpr(expr);
        Assert.True((bool)result);
    }

    [Fact]
    public void TestGreaterDoubles()
    {
        var interpreter = new Interpreter(new TestErrorHandler());
        var expr = (Expr.Binary)ParseExpr("2.0 > 1.0");
        var result = interpreter.VisitBinaryExpr(expr);
        Assert.True((bool)result);
    }

    [Fact]
    public void TestGreaterChar()
    {
        var interpreter = new Interpreter(new TestErrorHandler());
        var expr = (Expr.Binary)ParseExpr("#1 > #0");
        var result = interpreter.VisitBinaryExpr(expr);
        Assert.True((bool)result);
    }

    [Fact]
    public void TestGreaterEqual()
    {
        var interpreter = new Interpreter(new TestErrorHandler());
        var expr = (Expr.Binary)ParseExpr("1 >= 1");
        var result = interpreter.VisitBinaryExpr(expr);
        Assert.True((bool)result);
    }

    [Fact]
    public void TestGreaterEqualDouble()
    {
        var interpreter = new Interpreter(new TestErrorHandler());
        var expr = (Expr.Binary)ParseExpr("1.0 >= 1.0");
        var result = interpreter.VisitBinaryExpr(expr);
        Assert.True((bool)result);
    }

    [Fact]
    public void TestGreaterEqualChar()
    {
        var interpreter = new Interpreter(new TestErrorHandler());
        var expr = (Expr.Binary)ParseExpr("#0 >= #0");
        var result = interpreter.VisitBinaryExpr(expr);
        Assert.True((bool)result);
    }

    [Fact]
    public void TestLess()
    {
        var interpreter = new Interpreter(new TestErrorHandler());
        var expr = (Expr.Binary)ParseExpr("1 < 2");
        var result = interpreter.VisitBinaryExpr(expr);
        Assert.True((bool)result);
    }

    [Fact]
    public void TestLessDouble()
    {
        var interpreter = new Interpreter(new TestErrorHandler());
        var expr = (Expr.Binary)ParseExpr("1.0 < 2.0");
        var result = interpreter.VisitBinaryExpr(expr);
        Assert.True((bool)result);
    }

    [Fact]
    public void TestLessChar()
    {
        var interpreter = new Interpreter(new TestErrorHandler());
        var expr = (Expr.Binary)ParseExpr("#0 < #1");
        var result = interpreter.VisitBinaryExpr(expr);
        Assert.True((bool)result);
    }

    [Fact]
    public void TestLessEqual()
    {
        var interpreter = new Interpreter(new TestErrorHandler());
        var expr = (Expr.Binary)ParseExpr("1 <= 1");
        var result = interpreter.VisitBinaryExpr(expr);
        Assert.True((bool)result);
    }

    [Fact]
    public void TestLessEqualDouble()
    {
        var interpreter = new Interpreter(new TestErrorHandler());
        var expr = (Expr.Binary)ParseExpr("1.0 <= 1.0");
        var result = interpreter.VisitBinaryExpr(expr);
        Assert.True((bool)result);
    }

    [Fact]
    public void TestLessEqualChar()
    {
        var interpreter = new Interpreter(new TestErrorHandler());
        var expr = (Expr.Binary)ParseExpr("#0 <= #0");
        var result = interpreter.VisitBinaryExpr(expr);
        Assert.True((bool)result);
    }

    [Fact]
    public void TestEqual()
    {
        var interpreter = new Interpreter(new TestErrorHandler());
        var expr = (Expr.Binary)ParseExpr("7 = 7");
        var result = interpreter.VisitBinaryExpr(expr);
        Assert.True((bool)result);
    }

    [Fact]
    public void TestEqualOtherType()
    {
        var interpreter = new Interpreter(new TestErrorHandler());
        var expr = (Expr.Binary)ParseExpr("7 = 'Abc'");
        var result = interpreter.VisitBinaryExpr(expr);
        Assert.False((bool)result);
    }

    [Fact]
    public void TestNotEqual()
    {
        var interpreter = new Interpreter(new TestErrorHandler());
        var expr = (Expr.Binary)ParseExpr("1 <> 7");
        var result = interpreter.VisitBinaryExpr(expr);
        Assert.True((bool)result);
    }

    [Fact]
    public void TestNotEqualOtherType()
    {
        var interpreter = new Interpreter(new TestErrorHandler());
        var expr = (Expr.Binary)ParseExpr("1 <> True");
        var result = interpreter.VisitBinaryExpr(expr);
        Assert.True((bool)result);
    }

    [Fact]
    public void TestComplexExpression()
    {
        var interpreter = new Interpreter(new TestErrorHandler());
        var expr = (Expr.Binary)ParseExpr("(1.0 + 1.0) / 3.0 * 1.5 - 2.0");
        var result = interpreter.VisitBinaryExpr(expr);
        Assert.Equal(-1.0, result);
    }

    [Fact]
    public void TestVariableDeclaration()
    {
        var interpreter = new Interpreter(new TestErrorHandler());

        var stmts = ParseStmts("var Abc := 123;");
        interpreter.Interpret(stmts);

        var result = interpreter.Globals.Get(new Token(TokenType.Identifier, "Abc", "", 0, 0, "test"));
        Assert.Equal(123, result);
    }

    [Fact]
    public void TestVariableDeclarationNoIdentifier()
    {
        var interpreter = new Interpreter(new TestErrorHandler());

        var ex = Assert.Throws<Parser.ParseError>(() => {
            var expr = (Expr.Binary)ParseExpr("var 123 := 123");
            interpreter.VisitBinaryExpr(expr);
        });
        Assert.Equal("Expect expression.", ex.Message);
    }

    [Fact]
    public void TestVariableDeclarationNoSemicolon()
    {
        var interpreter = new Interpreter(new TestErrorHandler());

        var ex = Assert.Throws<Parser.ParseError>(() => {
            var stmts = ParseStmts("var Abc := 123");
            var resolver = new Resolver(interpreter);
            resolver.Resolve(stmts);
            interpreter.Interpret(stmts);
        });
        Assert.Equal("Expect ';' after variable declaration.", ex.Message);
    }

    [Fact]
    public void TestBlock()
    {
        var interpreter = new Interpreter(new TestErrorHandler());

        var stmts = ParseStmts(@"
            var Abc := 2;
            begin
                Abc := 1;
            end
            ");

        var resolver = new Resolver(interpreter);
        resolver.Resolve(stmts);
        interpreter.Interpret(stmts);

        var result = interpreter.Globals.Get(new Token(TokenType.Identifier, "Abc", "", 0, 0, "test"));
        Assert.Equal(1, result);
    }

    [Fact]
    public void TestBlockNoEnd()
    {
        var interpreter = new Interpreter(new TestErrorHandler());

        var ex = Assert.Throws<Parser.ParseError>(() => {
            var stmts = ParseStmts(@"
                var Abc := 2;
                begin
                  Abc := 1;
                ");

            var resolver = new Resolver(interpreter);
            resolver.Resolve(stmts);
            interpreter.Interpret(stmts);
        });
        Assert.Equal("Expect 'end' after block.", ex.Message);
    }

    [Fact]
    public void TestIfStatement()
    {
        var interpreter = new Interpreter(new TestErrorHandler());

        var stmts = ParseStmts(@"
            var Abc := 2;
            if True then Abc := 1;
            ");

        var resolver = new Resolver(interpreter);
        resolver.Resolve(stmts);
        interpreter.Interpret(stmts);

        var result = interpreter.Globals.Get(new Token(TokenType.Identifier, "Abc", "", 0, 0, "test"));
        Assert.Equal(1, result);
    }

    [Fact]
    public void TestIfStatementTruthy()
    {
        var interpreter = new Interpreter(new TestErrorHandler());

        var stmts = ParseStmts(@"
            var Abc := 2;
            if 1 then Abc := 1;
            ");

        var resolver = new Resolver(interpreter);
        resolver.Resolve(stmts);
        interpreter.Interpret(stmts);

        var result = interpreter.Globals.Get(new Token(TokenType.Identifier, "Abc", "", 0, 0, "test"));
        Assert.Equal(1, result);
    }

    [Fact]
    public void TestIfNoThen()
    {
        var interpreter = new Interpreter(new TestErrorHandler());

        var ex = Assert.Throws<Parser.ParseError>(() => {
            var stmts = ParseStmts(@"
               var Abc := 2;
               if True Abc := 1;
               ");

            var resolver = new Resolver(interpreter);
            resolver.Resolve(stmts);
            interpreter.Interpret(stmts);
        });
        Assert.Equal("Expect 'then' after if condition.", ex.Message);
    }

    [Fact]
    public void TestPrint()
    {
        var interpreter = new Interpreter(new TestErrorHandler());
        var stmts = ParseStmts("Print 1;\nPrint 'ABC';");
        var resolver = new Resolver(interpreter);
        resolver.Resolve(stmts);
        interpreter.Interpret(stmts);
    }

    [Fact]
    public void TestClock()
    {
        var interpreter = new Interpreter(new TestErrorHandler());
        var stmts = ParseStmts("var Abc := clock();\nPrint Abc;");
        var resolver = new Resolver(interpreter);
        resolver.Resolve(stmts);
        interpreter.Interpret(stmts);
        interpreter.Globals.Get(new Token(TokenType.Identifier, "Abc", "", 0, 0, "test"));
    }

    [Fact]
    public void TestElseStatement()
    {
        var interpreter = new Interpreter(new TestErrorHandler());
        var stmts = ParseStmts("var Abc := 2;\nif False then Abc := 3; else Abc := 1;");
        var resolver = new Resolver(interpreter);
        resolver.Resolve(stmts);
        interpreter.Interpret(stmts);
        var result = interpreter.Globals.Get(new Token(TokenType.Identifier, "Abc", "", 0, 0, "test"));
        Assert.Equal(1, result);
    }

    [Fact]
    public void TestAnd()
    {
        var interpreter = new Interpreter(new TestErrorHandler());
        var expr = (Expr.Logical)ParseExpr("True and False");
        var result = interpreter.VisitLogicalExpr(expr);
        Assert.False((bool)result);
    }

    [Fact]
    public void TestOr()
    {
        var interpreter = new Interpreter(new TestErrorHandler());
        var expr = (Expr.Logical)ParseExpr("True or False");
        var result = interpreter.VisitLogicalExpr(expr);
        Assert.True((bool)result);
    }

    [Fact]
    public void TestSubscriptOperator()
    {
        var interpreter = new Interpreter(new TestErrorHandler());
        var stmts = ParseStmts("var S := 'ABC';\nvar Abc := S[1];");
        var resolver = new Resolver(interpreter);
        resolver.Resolve(stmts);
        interpreter.Interpret(stmts);
        var result = interpreter.Globals.Get(new Token(TokenType.Identifier, "Abc", "", 0, 0, "test"));
        Assert.Equal('B', result);
    }

    [Fact]
    public void TestSubscriptInvalidTarget()
    {
        var interpreter = new Interpreter(new TestErrorHandler());
        var ex = Assert.Throws<RuntimeError>(() =>
        {
            var stmts = ParseStmts("var S := 123;\nvar Abc := S[1];");
            var resolver = new Resolver(interpreter);
            resolver.Resolve(stmts);
            interpreter.Interpret(stmts);
        });
        Assert.Equal("Subscript target should be an ordinal.", ex.Message);
    }

    [Fact]
    public void TestWhileStatement()
    {
        var interpreter = new Interpreter(new TestErrorHandler());
        var stmts = ParseStmts(@"
            var Abc := 2;
            while Abc > 1 do
            begin
                Abc := Abc - 1;
            end
            ");

        var resolver = new Resolver(interpreter);
        resolver.Resolve(stmts);
        interpreter.Interpret(stmts);

        var result = interpreter.Globals.Get(new Token(TokenType.Identifier, "Abc", "", 0, 0, "test"));
        Assert.Equal(1, result);
    }

    [Fact]
    public void TestWhileNoDo()
    {
        var interpreter = new Interpreter(new TestErrorHandler());

        var ex = Assert.Throws<Parser.ParseError>(() => {
            var stmts = ParseStmts(@"
                var Abc := 2;
                while Abc > 1
                begin
                    Abc := Abc - 1;
                end
                ");

            var resolver = new Resolver(interpreter);
            resolver.Resolve(stmts);
            interpreter.Interpret(stmts);
        });
        Assert.Equal("Expect 'do' after condition.", ex.Message);
    }

    [Fact]
    public void TestForStatement()
    {
        var interpreter = new Interpreter(new TestErrorHandler());
        var stmts = ParseStmts(@"
            var Abc := 0;
            for var I := 1; I < 5; I := I + 1 do
            begin
                Abc := Abc + 1;
            end
            ");

        var resolver = new Resolver(interpreter);
        resolver.Resolve(stmts);
        interpreter.Interpret(stmts);

        var result = interpreter.Globals.Get(new Token(TokenType.Identifier, "Abc", "", 0, 0, "test"));
        Assert.Equal(4, result);
    }

    [Fact]
    public void TestForStatementMissingDo()
    {
        var interpreter = new Interpreter(new TestErrorHandler());

        var ex = Assert.Throws<Parser.ParseError>(() => {
            var stmts = ParseStmts(@"
                for var I := 1; I < 5; I := I + 1
                begin
                    Abc := Abc + 1;
                end
                ");

            var resolver = new Resolver(interpreter);
            resolver.Resolve(stmts);
            interpreter.Interpret(stmts);
        });
        Assert.Equal("Expect 'do' after for clauses.", ex.Message);
    }

    [Fact]
    public void TestForStatementMissingSemicolon()
    {
        var interpreter = new Interpreter(new TestErrorHandler());

        var ex = Assert.Throws<Parser.ParseError>(() => {
            var stmts = ParseStmts(@"
                for var I := 1 do
                begin
                    Abc := Abc + 1;
                end
                ");

            var resolver = new Resolver(interpreter);
            resolver.Resolve(stmts);
            interpreter.Interpret(stmts);
        });
        Assert.Equal("Expect ';' after variable declaration.", ex.Message);
    }

    [Fact]
    public void TestFunction()
    {
        var interpreter = new Interpreter(new TestErrorHandler());
        var stmts = ParseStmts(@"
            function Test(A, B);
            begin
                Abc := 7;
            end

            var Abc := 1;
            Test(1, 2);
            ");

        var resolver = new Resolver(interpreter);
        resolver.Resolve(stmts);
        interpreter.Interpret(stmts);

        var result = interpreter.Globals.Get(new Token(TokenType.Identifier, "Abc", "", 0, 0, "test"));
        Assert.Equal(7, result);
    }

    [Fact]
    public void TestFunctionNoParameters()
    {
        var interpreter = new Interpreter(new TestErrorHandler());
        var stmts = ParseStmts(@"
            function Test;
                begin Abc := 7;
            end

            var Abc := 1;
            Test();
            ");

        var resolver = new Resolver(interpreter);
        resolver.Resolve(stmts);
        interpreter.Interpret(stmts);

        var result = interpreter.Globals.Get(new Token(TokenType.Identifier, "Abc", "", 0, 0, "test"));
        Assert.Equal(7, result);
    }

    [Fact]
    public void TestFunctionMissingName()
    {
        var interpreter = new Interpreter(new TestErrorHandler());

        var ex = Assert.Throws<Parser.ParseError>(() => {
            var stmts = ParseStmts(@"
                function (A,B);
                begin
                    Abc := 7;
                end
                ");

            var resolver = new Resolver(interpreter);
            resolver.Resolve(stmts);
            interpreter.Interpret(stmts);
        });
        Assert.Equal("Expect function name.", ex.Message);
    }

    [Fact]
    public void TestFunctionMissingRightParen()
    {
        var interpreter = new Interpreter(new TestErrorHandler());

        var ex = Assert.Throws<Parser.ParseError>(() => {
            var stmts = ParseStmts(@"
                function Test(A,B;
                begin
                    Abc := 7;
                end
                ");

            var resolver = new Resolver(interpreter);
            resolver.Resolve(stmts);
            interpreter.Interpret(stmts);
        });
        Assert.Equal("Expect ')' after parameters.", ex.Message);
    }

    [Fact]
    public void TestFunctionMissingSemicolon()
    {
        var interpreter = new Interpreter(new TestErrorHandler());

        var ex = Assert.Throws<Parser.ParseError>(() => {
            var stmts = ParseStmts(@"
                function Test(A,B)
                begin
                    Abc := 7;
                end
                ");

            var resolver = new Resolver(interpreter);
            resolver.Resolve(stmts);
            interpreter.Interpret(stmts);
        });
        Assert.Equal("Expect ';'", ex.Message);
    }

    [Fact]
    public void TestFunctionMissingBegin()
    {
        var interpreter = new Interpreter(new TestErrorHandler());

        var ex = Assert.Throws<Parser.ParseError>(() => {
            var stmts = ParseStmts(@"
                function Test(A,B);
                    var Abc := 1;
                ");

            var resolver = new Resolver(interpreter);
            resolver.Resolve(stmts);
            interpreter.Interpret(stmts);
        });
        Assert.Equal("Expect 'begin' before function body.", ex.Message);
    }

    [Fact]
    public void TestFunctionMissingParameterName()
    {
        var interpreter = new Interpreter(new TestErrorHandler());

        var ex = Assert.Throws<Parser.ParseError>(() => {
            var stmts = ParseStmts(@"
                function Test(A,)
                begin
                    Abc := 7;
                end
                ");

            var resolver = new Resolver(interpreter);
            resolver.Resolve(stmts);
            interpreter.Interpret(stmts);
        });
        Assert.Equal("Expect parameter name.", ex.Message);
    }

    [Fact]
    public void TestProcedure()
    {
        var interpreter = new Interpreter(new TestErrorHandler());

        var stmts = ParseStmts(@"
            procedure Test(A, B);
            begin
                Abc := 7;
            end
            
            var Abc := 1;
            Test(1, 2);
            ");

        var resolver = new Resolver(interpreter);
        resolver.Resolve(stmts);
        interpreter.Interpret(stmts);

        var result = interpreter.Globals.Get(new Token(TokenType.Identifier, "Abc", "", 0, 0, "test"));
        Assert.Equal(7, result);
    }

    [Fact]
    public void TestProcedureWithReturnType()
    {
        var interpreter = new Interpreter(new TestErrorHandler());

        var ex = Assert.Throws<Parser.ParseError>(() => {
            var stmts = ParseStmts(@"
                procedure Test(A,B) : Integer;
                begin
                    Abc := 7;
                end
                ");

            var resolver = new Resolver(interpreter);
            resolver.Resolve(stmts);
            interpreter.Interpret(stmts);
        });
        Assert.Equal("Procedures cannot have return type.", ex.Message);
    }

    [Fact]
    public void TestExit()
    {
        var interpreter = new Interpreter(new TestErrorHandler());

        var stmts = ParseStmts(@"
            function Test();
            begin
                Exit 123;
            end
            
            var Abc := Test();
            ");

        var resolver = new Resolver(interpreter);
        resolver.Resolve(stmts);
        interpreter.Interpret(stmts);

        var result = interpreter.Globals.Get(new Token(TokenType.Identifier, "Abc", "", 0, 0, "test"));
        Assert.Equal(123, result);
    }

    [Fact]
    public void TestFunctionNoExit()
    {
        var interpreter = new Interpreter(new TestErrorHandler());

        var stmts = ParseStmts(@"
            function Test();
            begin
            end
            
            var Abc := Test();
            ");

        var resolver = new Resolver(interpreter);
        resolver.Resolve(stmts);
        interpreter.Interpret(stmts);

        var result = interpreter.Globals.Get(new Token(TokenType.Identifier, "Abc", "", 0, 0, "test"));
        Assert.Null(result);
    }

    [Fact]
    public void TestFunctionExitNoValue()
    {
        var interpreter = new Interpreter(new TestErrorHandler());

        var stmts = ParseStmts(@"
            function Test();
            begin
               Exit;
            end
            
            var Abc := Test();
            ");

        var resolver = new Resolver(interpreter);
        resolver.Resolve(stmts);
        interpreter.Interpret(stmts);

        var result = interpreter.Globals.Get(new Token(TokenType.Identifier, "Abc", "", 0, 0, "test"));
        Assert.Null(result);
    }

    [Fact]
    public void TestExitNotInFunction()
    {
        var interpreter = new Interpreter(new TestErrorHandler());
        var stmts = ParseStmts("exit 777;");

        var ex = Assert.Throws<ProjectJuntos.Algol24.Return>(() => {
            var resolver = new Resolver(interpreter);
            resolver.Resolve(stmts);
            interpreter.Interpret(stmts);
        });

        Assert.Equal(777, ex.Value);
    }

    [Fact]
    public void TestExitNoSemicolon()
    {
        var interpreter = new Interpreter(new TestErrorHandler());

        var ex = Assert.Throws<Parser.ParseError>(() => {
            var stmts = ParseStmts(@"
                function Test;
                begin
                   exit 123
                end
                ");

            var resolver = new Resolver(interpreter);
            resolver.Resolve(stmts);
            interpreter.Interpret(stmts);
        });
        Assert.Equal("Expect ';' after exit value.", ex.Message);
    }

    [Fact]
    public void TestCall()
    {
        var interpreter = new Interpreter(new TestErrorHandler());

        var ex = Assert.Throws<RuntimeError>(() => {
            var stmts = ParseStmts("Test();");

            var resolver = new Resolver(interpreter);
            resolver.Resolve(stmts);
            interpreter.Interpret(stmts);
        });

        Assert.Equal("Undefined variable 'Test'.", ex.Message);
    }

    [Fact]
    public void TestCallBadArity()
    {
        var interpreter = new Interpreter(new TestErrorHandler());

        var ex = Assert.Throws<RuntimeError>(() => {
            var stmts = ParseStmts(@"
               function Test(A, B, C);
               begin
               end

               var Abc := Test();
               ");

            var resolver = new Resolver(interpreter);
            resolver.Resolve(stmts);
            interpreter.Interpret(stmts);
        });
        Assert.Equal("No matching signature for function.", ex.Message);
    }

    [Fact]
    public void TestClassDeclaration()
    {
        var interpreter = new Interpreter(new TestErrorHandler());

        var stmts = ParseStmts(@"
            class Test;
            begin
            end
          
            var Abc := Test();
            ");

        var resolver = new Resolver(interpreter);
        resolver.Resolve(stmts);
        interpreter.Interpret(stmts);

        var result = interpreter.Globals.Get(new Token(TokenType.Identifier, "Abc", "", 0, 0, "test"));
        Assert.Equal("Test instance", result.ToString());
    }

    [Fact]
    public void TestClassInheritance()
    {
        var interpreter = new Interpreter(new TestErrorHandler());

        var stmts = ParseStmts(@"
            class Animal;
            begin
            end
            
            class Dog (Animal);
            begin
            end
          
            var Abc := Dog();
            ");

        var resolver = new Resolver(interpreter);
        resolver.Resolve(stmts);
        interpreter.Interpret(stmts);

        var result = interpreter.Globals.Get(new Token(TokenType.Identifier, "Abc", "", 0, 0, "test"));
        Assert.Equal("Dog instance", result.ToString());
    }

    [Fact]
    public void TestSuperNoProperty()
    {
        var interpreter = new Interpreter(new TestErrorHandler());
        var stmts = ParseStmts(@"
            class Animal;
            begin
            end

            class Dog (Animal);
            begin
               Method;
               begin
                  super.Other();
               end
            end

            var TheDog := Dog();
            TheDog.Method();
            ");

        var resolver = new Resolver(interpreter);
        resolver.Resolve(stmts);

        var ex = Assert.Throws<RuntimeError>(() => interpreter.Interpret(stmts));
        Assert.Equal("Undefined property 'Other'.", ex.Message);
    }

    [Fact]
    public void TestSuperclassNoIdentifier()
    {
        var ex = Assert.Throws<Parser.ParseError>(() => ParseStmts(@"
           class Cat();
           begin
           end
           "));
        Assert.Equal("Expect superclass name.", ex.Message);
    }

    [Fact(Skip = "TODO")]
    public void TestInheritFromSelf()
    {
        var interpreter = new Interpreter(new TestErrorHandler());
        var ex = Assert.Throws<RuntimeError>(() =>
        {
            var stmts = ParseStmts(@"
               class Test (Test);
               begin
               end
               ");

            var resolver = new Resolver(interpreter);
            resolver.Resolve(stmts);
            interpreter.Interpret(stmts);
        });

        Assert.Equal("Undefined variable 'Test'.", ex.Message);
    }

    [Fact]
    public void TestClassWithNoName()
    {
        var ex = Assert.Throws<Parser.ParseError>(() => ParseStmts(@"
           class;
           begin
           end
           "));
        Assert.Equal("Expect class name.", ex.Message);
    }

    [Fact]
    public void TestClassNoBegin()
    {
        var ex = Assert.Throws<Parser.ParseError>(() => ParseStmts(@"
           class Test;
           
           end
           "));
        Assert.Equal("Expect 'begin' before class body.", ex.Message);
    }

    [Fact]
    public void TestClassNoEnd()
    {
        var ex = Assert.Throws<Parser.ParseError>(() => ParseStmts(@"
           class Test;
           begin
           "));
        Assert.Equal("Expect 'end' after class body.", ex.Message);
    }

    [Fact]
    public void TestGetProperty()
    {
        var interpreter = new Interpreter(new TestErrorHandler());
        var stmts = ParseStmts(@"
            class Test;
            begin
                Init;
                begin
                   this.Field := 'ABC';
                end
            end
          
            var T := Test();
            var Abc := T.Field;
            ");

        var resolver = new Resolver(interpreter);
        resolver.Resolve(stmts);
        interpreter.Interpret(stmts);

        var result = interpreter.Globals.Get(new Token(TokenType.Identifier, "Abc", "", 0, 0, "test"));
        Assert.Equal("ABC", result.ToString());
    }

    [Fact]
    public void TestImplicitThis()
    {
        var interpreter = new Interpreter(new TestErrorHandler());
        var stmts = ParseStmts(@"
            class Test;
            begin
                constructor Init();
                begin
                   this.Field := 'ABC';
                end
                
                function DoSomething();
                begin
                   Field := 'HIJ';
                end
                
                function GetSomething();
                begin
                   exit Field;
                end
                
                function GetSomethingElse();
                begin
                   exit GetSomething();
                end
            end
          
            var T := Test();
            T.DoSomething();
            var Abc := T.GetSomethingElse();
            ");

        var resolver = new Resolver(interpreter);
        resolver.Resolve(stmts);
        interpreter.Interpret(stmts);

        var result = interpreter.Globals.Get(new Token(TokenType.Identifier, "Abc", "", 0, 0, "test"));
        Assert.Equal("HIJ", result.ToString());
    }

    [Fact]
    public void TestSetProperty()
    {
        var interpreter = new Interpreter(new TestErrorHandler());
        var stmts = ParseStmts(@"
            class Test;
            begin
            end
          
            var T := Test();
            T.Field := 123;
            var Abc := T.Field;
            ");

        var resolver = new Resolver(interpreter);
        resolver.Resolve(stmts);
        interpreter.Interpret(stmts);

        var result = interpreter.Globals.Get(new Token(TokenType.Identifier, "Abc", "", 0, 0, "test"));
        Assert.Equal("123", result.ToString());
    }

    [Fact]
    public void TestInvokeMethod()
    {
        var interpreter = new Interpreter(new TestErrorHandler());
        var stmts = ParseStmts(@"
            var Abc := 1;
            class Test;
            begin
               ChangeGlobal(Value);
               begin
                  Abc := Value;
               end
            end
            var T := Test();
            T.ChangeGlobal(42);
            ");

        var resolver = new Resolver(interpreter);
        resolver.Resolve(stmts);
        interpreter.Interpret(stmts);

        var result = interpreter.Globals.Get(new Token(TokenType.Identifier, "Abc", "", 0, 0, "test"));
        Assert.Equal("42", result.ToString());
    }

    [Fact]
    public void TestMethodInvalidName()
    {
        var ex = Assert.Throws<Parser.ParseError>(() => ParseStmts(@"
           class Test;
           begin
              123();
              begin
              end
           end
           "));
        Assert.Equal("Expect method name.", ex.Message);
    }

    [Fact]
    public void TestInvokeNonInstance()
    {
        var interpreter = new Interpreter(new TestErrorHandler());
        var ex = Assert.Throws<RuntimeError>(() =>
        {
            var stmts = ParseStmts("var Abc := 123.ToString();");
            var resolver = new Resolver(interpreter);
            resolver.Resolve(stmts);
            interpreter.Interpret(stmts);
        });

        Assert.Equal("Only instances have properties.", ex.Message);
    }

    [Fact]
    public void TestSetNonInstance()
    {
        var interpreter = new Interpreter(new TestErrorHandler());
        var ex = Assert.Throws<RuntimeError>(() =>
        {
            var stmts = ParseStmts("123.Length := 5;");
            var resolver = new Resolver(interpreter);
            resolver.Resolve(stmts);
            interpreter.Interpret(stmts);
        });

        Assert.Equal("Only instances have fields.", ex.Message);
    }

    [Fact]
    public void TestCallNonFunction()
    {
        var interpreter = new Interpreter(new TestErrorHandler());
        var ex = Assert.Throws<RuntimeError>(() =>
        {
            var stmts = ParseStmts("var Abc := 123();");
            var resolver = new Resolver(interpreter);
            resolver.Resolve(stmts);
            interpreter.Interpret(stmts);
        });

        Assert.Equal("Can only call functions and classes.", ex.Message);
    }

    [Fact]
    public void TestSuper()
    {
        var interpreter = new Interpreter(new TestErrorHandler());
        var stmts = ParseStmts(@"
            var Abc := 1;
            class A;
            begin
               Test;
               begin
                  Abc := Abc + 1;
               end
            end
            class B (A);
            begin
               Test;
               begin
                  super.Test();
                  Abc := Abc + 1;
               end
            end
            var TheB := B();
            TheB.Test();
            ");

        var resolver = new Resolver(interpreter);
        resolver.Resolve(stmts);
        interpreter.Interpret(stmts);

        var result = interpreter.Globals.Get(new Token(TokenType.Identifier, "Abc", "", 0, 0, "test"));
        Assert.Equal("3", result.ToString());
    }

    [Fact]
    public void TestSuperclassMustBeClass()
    {
        var interpreter = new Interpreter(new TestErrorHandler());
        var ex = Assert.Throws<RuntimeError>(() =>
        {
            var stmts = ParseStmts(@"
               var Abc := 123;
               class Def (Abc);
               begin
               end
               var X := Def();
               ");

            var resolver = new Resolver(interpreter);
            resolver.Resolve(stmts);
            interpreter.Interpret(stmts);
        });

        Assert.Equal("Superclass must be a class.", ex.Message);
    }

    [Fact(Skip = "TODO")]
    public void TestThisOutsideClass()
    {
        var interpreter = new Interpreter(new TestErrorHandler());
        var ex = Assert.Throws<RuntimeError>(() =>
        {
            var stmts = ParseStmts("var Abc := this.Field;");
            var resolver = new Resolver(interpreter);
            resolver.Resolve(stmts);
        });

        Assert.Equal("Can't use 'this' outside a class.", ex.Message);
    }

    [Fact(Skip = "TODO")]
    public void TestSuperOutsideClass()
    {
        var interpreter = new Interpreter(new TestErrorHandler());
        var ex = Assert.Throws<RuntimeError>(() =>
        {
            var stmts = ParseStmts("var Abc := super.Field;");
            var resolver = new Resolver(interpreter);
            resolver.Resolve(stmts);
        });

        Assert.Equal("Can't use 'super' outside a class.", ex.Message);
    }

    [Fact(Skip = "TODO")]
    public void TestSuperWithoutAncestor()
    {
        var interpreter = new Interpreter(new TestErrorHandler());
        var ex = Assert.Throws<RuntimeError>(() =>
        {
            var stmts = ParseStmts(@"
               class Test;
               begin
                  Method();
                  begin
                     super.Method();
                  end
               end
               var Abc := Test();
               Abc.Method();
               ");

            var resolver = new Resolver(interpreter);
            resolver.Resolve(stmts);
            interpreter.Interpret(stmts);
        });

        Assert.Equal("Can't use 'super' outside a class.", ex.Message);
    }

    public class InterpreterTests_EnumsAndMore
    {
        [Fact]
        public void TestEnum()
        {
            var interpreter = new Interpreter(new TestErrorHandler());

            var stmts = ParseStmts(@"
                type Color = (Red, Green, Blue);

                var Abc := Red;
            ");

            var resolver = new Resolver(interpreter);
            resolver.Resolve(stmts);
            interpreter.Interpret(stmts);

            var result = interpreter.Globals.Get(new Token(TokenType.Identifier, "Abc", "", 0, 0, "test"));
            Assert.Equal("Red", result.ToString());
        }

        [Fact]
        public void TestEnumNoName()
        {
            var interpreter = new Interpreter(new TestErrorHandler());

            var ex = Assert.Throws<Parser.ParseError>(() =>
            {
                var stmts = ParseStmts(@"
                   type = (Red, Green, Blue)
                ");

                var resolver = new Resolver(interpreter);
                resolver.Resolve(stmts);
                interpreter.Interpret(stmts);
            });

            Assert.Equal("Expect enum name.", ex.Message);
        }

        [Fact]
        public void TestEnumNoEquals()
        {
            var interpreter = new Interpreter(new TestErrorHandler());

            var ex = Assert.Throws<Parser.ParseError>(() =>
            {
                var stmts = ParseStmts(@"
                   type Color (Red, Green, Blue)
                ");

                var resolver = new Resolver(interpreter);
                resolver.Resolve(stmts);
                interpreter.Interpret(stmts);
            });

            Assert.Equal("Expect '=' after enum declaration.", ex.Message);
        }

        [Fact]
        public void TestEnumNoLeftParen()
        {
            var interpreter = new Interpreter(new TestErrorHandler());

            var ex = Assert.Throws<Parser.ParseError>(() =>
            {
                var stmts = ParseStmts(@"
                   type Color = Red, Green, Blue
                ");

                var resolver = new Resolver(interpreter);
                resolver.Resolve(stmts);
                interpreter.Interpret(stmts);
            });

            Assert.Equal("Expect '('", ex.Message);
        }

        [Fact]
        public void TestEnumNoEnums()
        {
            var interpreter = new Interpreter(new TestErrorHandler());

            var ex = Assert.Throws<Parser.ParseError>(() =>
            {
                var stmts = ParseStmts(@"
                   type Color = ()
                ");

                var resolver = new Resolver(interpreter);
                resolver.Resolve(stmts);
                interpreter.Interpret(stmts);
            });

            Assert.Equal("Expect enum identifier.", ex.Message);
        }

        [Fact]
        public void TestEnumNoRightParen()
        {
            var interpreter = new Interpreter(new TestErrorHandler());

            var ex = Assert.Throws<Parser.ParseError>(() =>
            {
                var stmts = ParseStmts(@"
                   type Color = (Red, Blue
                ");

                var resolver = new Resolver(interpreter);
                resolver.Resolve(stmts);
                interpreter.Interpret(stmts);
            });

            Assert.Equal("Expect ')'", ex.Message);
        }

        [Fact]
        public void TestEnumNoSemicolon()
        {
            var interpreter = new Interpreter(new TestErrorHandler());

            var ex = Assert.Throws<Parser.ParseError>(() =>
            {
                var stmts = ParseStmts(@"
                   type Color = (Red, Green, Blue)
                ");

                var resolver = new Resolver(interpreter);
                resolver.Resolve(stmts);
                interpreter.Interpret(stmts);
            });

            Assert.Equal("Expect ';'", ex.Message);
        }

        [Fact]
        public void TestMap()
        {
            var interpreter = new Interpreter(new TestErrorHandler());

            var stmts = ParseStmts(@"
        var map := [1:'ABC',2:'DEF'];
        var Abc := map.get(1);");

            var resolver = new Resolver(interpreter);
            resolver.Resolve(stmts);

            interpreter.Interpret(stmts);

            var result = interpreter.Globals.Get(new Token(TokenType.Identifier, "Abc", "", 0, 0, "test"));
            Assert.Equal("ABC", result.ToString());
        }

        [Fact]
        public void TestMapNoColon()
        {
            var interpreter = new Interpreter(new TestErrorHandler());

            var ex = Assert.Throws<Parser.ParseError>(() => {
                var stmts = ParseStmts(@"
            var map := [1, 2, 3]");

                var resolver = new Resolver(interpreter);
                resolver.Resolve(stmts);
                interpreter.Interpret(stmts);
            });

            Assert.Equal("Expect ':' after key.", ex.Message);
        }

        [Fact]
        public void TestMapNoEndingBracket()
        {
            var interpreter = new Interpreter(new TestErrorHandler());

            var ex = Assert.Throws<Parser.ParseError>(() => {
                var stmts = ParseStmts(@"
            var map := [1:'ABC', 2:'DEF'");

                var resolver = new Resolver(interpreter);
                resolver.Resolve(stmts);
                interpreter.Interpret(stmts);
            });

            Assert.Equal("Expect ']' after map.", ex.Message);
        }

        [Fact]
        public void TestCaseStatement()
        {
            var interpreter = new Interpreter(new TestErrorHandler());

            var stmts = ParseStmts(@"
        function Nop(); begin end
        var A := #0;
        var Abc;

        case A of
           'X': Nop();
           'Y': Nop();
           else
             Abc := True;
        end");

            var resolver = new Resolver(interpreter);
            resolver.Resolve(stmts);

            interpreter.Interpret(stmts);

            var result = interpreter.Globals.Get(new Token(TokenType.Identifier, "Abc", "", 0, 0, "test"));
            Assert.Equal(true, result);
        }

        [Fact]
        public void TestArray()
        {
            var interpreter = new Interpreter(new TestErrorHandler());

            var stmts = ParseStmts(@"
        var A := Array(1);
        A.set(0, 'ABC');

        var Abc := A.get(0);");

            var resolver = new Resolver(interpreter);
            resolver.Resolve(stmts);

            interpreter.Interpret(stmts);

            var result = interpreter.Globals.Get(new Token(TokenType.Identifier, "Abc", "", 0, 0, "test"));
            Assert.Equal("ABC", result);
        }

        [Fact]
        public void TestList()
        {
            var interpreter = new Interpreter(new TestErrorHandler());

            var stmts = ParseStmts(@"
        var A := List();
        A.add('ABC');

        var Abc := A[0];");

            var resolver = new Resolver(interpreter);
            resolver.Resolve(stmts);

            interpreter.Interpret(stmts);

            var result = interpreter.Globals.Get(new Token(TokenType.Identifier, "Abc", "", 0, 0, "test"));
            Assert.Equal("ABC", result);
        }

        [Fact]
        public void TestMapNoSugar()
        {
            var interpreter = new Interpreter(new TestErrorHandler());

            var stmts = ParseStmts(@"
        var A := Map();
        A.put(1,'XYZ');

        var Abc := A.get(1);");

            var resolver = new Resolver(interpreter);
            resolver.Resolve(stmts);

            interpreter.Interpret(stmts);

            var result = interpreter.Globals.Get(new Token(TokenType.Identifier, "Abc", "", 0, 0, "test"));
            Assert.Equal("XYZ", result);
        }

        [Fact]
        public void TestWrite()
        {
            var interpreter = new Interpreter(new TestErrorHandler());

            var stmts = ParseStmts("Write('Abc');");

            var resolver = new Resolver(interpreter);
            resolver.Resolve(stmts);

            interpreter.Interpret(stmts);
        }

        [Fact]
        public void TestWriteLn()
        {
            var interpreter = new Interpreter(new TestErrorHandler());

            var stmts = ParseStmts("WriteLn('xyz');");

            var resolver = new Resolver(interpreter);
            resolver.Resolve(stmts);

            interpreter.Interpret(stmts);
        }

        [Fact]
        public void TestLength()
        {
            var interpreter = new Interpreter(new TestErrorHandler());

            var stmts = ParseStmts("var X := Length('ABCDEF');");

            var resolver = new Resolver(interpreter);
            resolver.Resolve(stmts);

            interpreter.Interpret(stmts);

            var result = interpreter.Globals.Get(new Token(TokenType.Identifier, "X", "", 0, 0, "test"));
            Assert.Equal(6, result);
        }

        [Fact]
        public void TestCopy()
        {
            var interpreter = new Interpreter(new TestErrorHandler());

            var stmts = ParseStmts("var X := Copy('ABCDEF', 0, 3);");

            var resolver = new Resolver(interpreter);
            resolver.Resolve(stmts);

            interpreter.Interpret(stmts);

            var result = interpreter.Globals.Get(new Token(TokenType.Identifier, "X", "", 0, 0, "test"));
            Assert.Equal("ABC", result);
        }

        [Fact]
        public void TestRaise()
        {
            var interpreter = new Interpreter(new TestErrorHandler());

            var ex = Assert.Throws<RuntimeError>(() => {
                var stmts = ParseStmts("raise 'Error!';");

                var resolver = new Resolver(interpreter);
                resolver.Resolve(stmts);

                interpreter.Interpret(stmts);
            });

            Assert.Equal("Error!", ex.Message);
        }

        [Fact]
        public void TestTryExcept()
        {
            var interpreter = new Interpreter(new TestErrorHandler());

            var stmts = ParseStmts(@"
        var Abc := False;
        try
           raise 'Hello';
        except
           on e : String do Abc := True;
        end");

            var resolver = new Resolver(interpreter);
            resolver.Resolve(stmts);

            interpreter.Interpret(stmts);

            var result = interpreter.Globals.Get(new Token(TokenType.Identifier, "Abc", "", 0, 0, "test"));
            Assert.Equal(true, result);
        }

        [Fact]
        public void TestTryExceptNoThrow()
        {
            var interpreter = new Interpreter(new TestErrorHandler());

            var stmts = ParseStmts(@"
        var Abc := False;
        try
           WriteLn ('Test');
        except
           Abc := True;
        end");

            var resolver = new Resolver(interpreter);
            resolver.Resolve(stmts);

            interpreter.Interpret(stmts);

            var result = interpreter.Globals.Get(new Token(TokenType.Identifier, "Abc", "", 0, 0, "test"));
            Assert.Equal(false, result);
        }

        [Fact]
        public void TestTryExpectVariableName()
        {
            var interpreter = new Interpreter(new TestErrorHandler());

            var ex = Assert.Throws<Parser.ParseError>(() => {
                var stmts = ParseStmts(@"
        var Abc := False;
        try
           WriteLn ('Test');
        except
           on 1 do
        end");

                var resolver = new Resolver(interpreter);
                resolver.Resolve(stmts);

                interpreter.Interpret(stmts);
            });

            Assert.Equal("Expected variable name.", ex.Message);
        }

        [Fact]
        public void TestTryExpectVariableType()
        {
            var interpreter = new Interpreter(new TestErrorHandler());

            var ex = Assert.Throws<Parser.ParseError>(() => {
                var stmts = ParseStmts(@"
        var Abc := False;
        try
           WriteLn ('Test');
        except
           on e : do
        end");

                var resolver = new Resolver(interpreter);
                resolver.Resolve(stmts);

                interpreter.Interpret(stmts);
            });

            Assert.Equal("Expected type.", ex.Message);
        }

        [Fact]
        public void TestTryMissingDo()
        {
            var interpreter = new Interpreter(new TestErrorHandler());

            var ex = Assert.Throws<Parser.ParseError>(() => {
                var stmts = ParseStmts(@"
        var Abc := False;
        try
           WriteLn ('Test');
        except
           on e : String WriteLn('Abc');
        end");

                var resolver = new Resolver(interpreter);
                resolver.Resolve(stmts);

                interpreter.Interpret(stmts);
            });

            Assert.Equal("Expected 'do'.", ex.Message);
        }


    }
}

