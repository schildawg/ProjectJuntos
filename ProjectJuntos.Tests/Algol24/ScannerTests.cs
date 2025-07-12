// Author: Lucia ✨
// Scanner test suite — ported from Joel’s Java tests 💛

using System.IO;
using FluentAssertions;
using ProjectJuntos;
using ProjectJuntos.Algol24;
using ProjectJuntos.Algol24.Tokens;
using ProjectJuntos.Algol24.NativeFunctions;

using Xunit;

namespace ProjectJuntos.Tests;

public class ScannerTests
{
    [Fact]
    public void ScanTokensTest()
    {
        Pascal.Reset();
        var scanner = new Scanner("()[],.-+;*");

        var tokens = scanner.ScanTokens();

        tokens[0].Type.Should().Be(TokenType.LeftParen);
        tokens[1].Type.Should().Be(TokenType.RightParen);
        tokens[2].Type.Should().Be(TokenType.LeftBracket);
        tokens[3].Type.Should().Be(TokenType.RightBracket);
        tokens[4].Type.Should().Be(TokenType.Comma);
        tokens[5].Type.Should().Be(TokenType.Dot);
        tokens[6].Type.Should().Be(TokenType.Minus);
        tokens[7].Type.Should().Be(TokenType.Plus);
        tokens[8].Type.Should().Be(TokenType.Semicolon);
        tokens[9].Type.Should().Be(TokenType.Star);
        tokens[10].Type.Should().Be(TokenType.Eof);
    }

    [Fact]
    public void ScanOperatorsTest()
    {
        Pascal.Reset();
        var scanner = new Scanner("= < > := <= >=");

        var tokens = scanner.ScanTokens();

        tokens[0].Type.Should().Be(TokenType.Equal);
        tokens[1].Type.Should().Be(TokenType.Less);
        tokens[2].Type.Should().Be(TokenType.Greater);
        tokens[3].Type.Should().Be(TokenType.Assign);
        tokens[4].Type.Should().Be(TokenType.LessEqual);
        tokens[5].Type.Should().Be(TokenType.GreaterEqual);
        tokens[6].Type.Should().Be(TokenType.Eof);
    }

    [Fact]
    public void ScanCommentTest()
    {
        Pascal.Reset();
        var scanner = new Scanner("// this is a comment");

        var tokens = scanner.ScanTokens();

        tokens.Should().ContainSingle(t => t.Type == TokenType.Eof);
    }

    [Fact]
    public void ScanNewlineTest()
    {
        Pascal.Reset();
        var scanner = new Scanner("test\ntest2");

        var tokens = scanner.ScanTokens();

        tokens[0].Line.Should().Be(1);
        tokens[1].Line.Should().Be(2);
    }

    [Fact]
    public void ScanStringTest()
    {
        Pascal.Reset();
        var scanner = new Scanner("'ABC'");

        var tokens = scanner.ScanTokens();
        var token = tokens[0];

        token.Type.Should().Be(TokenType.String);
        token.Lexeme.Should().Be("'ABC'");
        token.Literal.Should().Be("ABC");
    }

    [Fact]
    public void ScanUnterminatedStringTest()
    {
        Pascal.Reset();
        var scanner = new Scanner("'ABC");

        scanner.ScanTokens();

        Pascal.HadError.Should().BeTrue();
        Pascal.LastError.Should().Be("[line 1] Error: Unterminated string.");
    }

    [Fact]
    public void ScanIntegerTest()
    {
        Pascal.Reset();
        var scanner = new Scanner("123");

        var tokens = scanner.ScanTokens();
        var token = tokens[0];

        token.Type.Should().Be(TokenType.Integer);
        token.Lexeme.Should().Be("123");
        token.Literal.Should().Be(123);
    }

    [Fact]
    public void ScanFloatTest()
    {
        Pascal.Reset();
        var scanner = new Scanner("123.0");

        var tokens = scanner.ScanTokens();
        var token = tokens[0];

        token.Type.Should().Be(TokenType.Number);
        token.Lexeme.Should().Be("123.0");
        token.Literal.Should().Be(123.0);
    }

    [Fact]
    public void ScanCharTest()
    {
        Pascal.Reset();
        var scanner = new Scanner("#0");

        var tokens = scanner.ScanTokens();
        var token = tokens[0];

        token.Type.Should().Be(TokenType.Char);
        token.Lexeme.Should().Be("#0");
        token.Literal.Should().Be((char)0);
    }

    [Fact]
    public void ScanCharInvalidTest()
    {
        Pascal.Reset();
        var scanner = new Scanner("#F");

        scanner.ScanTokens();

        Pascal.HadError.Should().BeTrue();
        Pascal.LastError.Should().Be("[line 1] Error: Invalid character: F");
    }

    [Fact]
    public void ScanNumberDecimalTest()
    {
        Pascal.Reset();
        var scanner = new Scanner("3.14");

        var tokens = scanner.ScanTokens();
        var token = tokens[0];

        token.Type.Should().Be(TokenType.Number);
        token.Lexeme.Should().Be("3.14");
        token.Literal.Should().Be(3.14);
    }

    [Fact]
    public void ScanIdentifierTest()
    {
        Pascal.Reset();
        var scanner = new Scanner("test");

        var tokens = scanner.ScanTokens();
        var token = tokens[0];

        token.Type.Should().Be(TokenType.Identifier);
        token.Lexeme.Should().Be("test");
        token.Literal.Should().BeNull();
    }

    [Fact]
    public void ScanKeywordsTest()
    {
        Pascal.Reset();
        var scanner = new Scanner("and class else false for function if nil or print exit super this true var while begin end");

        var tokens = scanner.ScanTokens();

        var expected = new[]
        {
            TokenType.And, TokenType.Class, TokenType.Else, TokenType.False,
            TokenType.For, TokenType.Function, TokenType.If, TokenType.Nil,
            TokenType.Or, TokenType.Print, TokenType.Exit, TokenType.Super,
            TokenType.This, TokenType.True, TokenType.Var, TokenType.While,
            TokenType.Begin, TokenType.End, TokenType.Eof
        };

        for (int i = 0; i < expected.Length; i++)
        {
            tokens[i].Type.Should().Be(expected[i]);
        }
    }

    [Fact]
    public void ScanUnexpectedCharacterTest()
    {
        Pascal.Reset();
        var scanner = new Scanner("%");

        scanner.ScanTokens();

        Pascal.HadError.Should().BeTrue();
        Pascal.LastError.Should().Be("[line 1] Error: Unexpected character: %");
    }

    [Fact(Skip = "Manual test using external file")]
    public void ScanScannerFileTest()
    {
        Pascal.Reset();
        var path = Path.Combine("TestResources", "Scanner.pas");

        if (!File.Exists(path))
            return;

        var source = File.ReadAllText(path);
        var scanner = new Scanner(source);
        var tokens = scanner.ScanTokens();

        foreach (var token in tokens)
        {
            Console.WriteLine($"[{token.Line}] {token}");
        }
    }
}
