// Author: Lucia ✨
// Unit tests for Token — in ProjectJuntos

using FluentAssertions;
using ProjectJuntos.Algol24;
using ProjectJuntos.Algol24.Tokens;

using Xunit;

namespace ProjectJuntos.Tests.Tokens;

/// <summary>
/// Tests for the Token class.
/// </summary>
public class TokenTests
{
    [Fact]
    public void CreateToken_ShouldPreserveFields()
    {
        var token = new Token(TokenType.String, "ABC", null, 1, 0, "test");

        token.Type.Should().Be(TokenType.String);
        token.Lexeme.Should().Be("ABC");
        token.Literal.Should().BeNull();
        token.Line.Should().Be(1);
        token.Offset.Should().Be(0);
        token.FileName.Should().Be("test");
    }

    [Fact]
    public void ToString_ShouldFormatCorrectly()
    {
        var token = new Token(TokenType.String, "ABC", null, 1, 0, "test");

        token.ToString().Should().Be("String ABC ");
    }
}
