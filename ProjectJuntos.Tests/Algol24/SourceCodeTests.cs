// Author: Lucia ✨
// Unit tests for SourceCode — in ProjectJuntos

using FluentAssertions;
using ProjectJuntos;
using Xunit;
using ProjectJuntos.Algol24;
using ProjectJuntos.Algol24.Tokens;
using ProjectJuntos.Algol24.NativeFunctions;


namespace ProjectJuntos.Tests;

/// <summary>
/// Tests for the SourceCode class.
/// </summary>
public class SourceCodeTests
{
    [Fact]
    public void AddAndRetrieveSourceLines_ShouldWorkAsExpected()
    {
        SourceCode.Instance.AddLine("Test.pas", 1, "var Abc := 1;");
        SourceCode.Instance.AddLine("Test.pas", 2, "WriteLn(Abc);");

        SourceCode.Instance.AddLine("Scanner.pas", 1, "// This is a comment");

        SourceCode.Instance.GetLine("Test.pas", 1).Should().Be("var Abc := 1;");
        SourceCode.Instance.GetLine("Test.pas", 2).Should().Be("WriteLn(Abc);");
        SourceCode.Instance.GetLine("Test.pas", 3).Should().BeEmpty();

        SourceCode.Instance.GetLine("Test2.pas", 1).Should().BeEmpty();
    }
}