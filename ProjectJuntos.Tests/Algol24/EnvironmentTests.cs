using System;
using Xunit;
using ProjectJuntos;
using ProjectJuntos.Algol24;
using ProjectJuntos.Algol24.Tokens;

namespace ProjectJuntos.Tests;

/// <summary>
/// Tests the Environment class.
/// </summary>
public class EnvironmentTests
{
    [Fact]
    public void DefinesAndRetrievesValue()
    {
        var env = new ProjectJuntos.Algol24.Environment();
        env.Define("test", 1);

        var result = env.Get(new Token(TokenType.Identifier, "test", null, 0, 0, "test"));

        Assert.Equal(1, result);
    }

    [Fact]
    public void OverridesLocalScopeWithoutAffectingGlobal()
    {
        var globals = new ProjectJuntos.Algol24.Environment();
        var env = new ProjectJuntos.Algol24.Environment(globals);

        globals.Define("test", 1);
        env.Define("test", 2);

        var token = new Token(TokenType.Identifier, "test", null, 0, 0, "test");

        Assert.Equal(1, globals.Get(token));
        Assert.Equal(2, env.Get(token));
    }

    [Fact]
    public void CanAccessThreeScopesDeep()
    {
        var globals = new ProjectJuntos.Algol24.Environment();
        var env = new ProjectJuntos.Algol24.Environment(globals);
        var functionEnv = new ProjectJuntos.Algol24.Environment(env);

        globals.Define("test", 1);
        var token = new Token(TokenType.Identifier, "test", null, 0, 0, "test");

        Assert.Equal(1, functionEnv.Get(token));
    }

    [Fact]
    public void CanHopThreeScopesDeep()
    {
        var globals = new ProjectJuntos.Algol24.Environment();
        var env = new ProjectJuntos.Algol24.Environment(globals);
        var functionEnv = new ProjectJuntos.Algol24.Environment(env);

        globals.Define("test", 1);

        Assert.Equal(1, functionEnv.GetAt(2, "test"));
    }

    [Fact]
    public void AssignsAndOverridesFromInnerScope()
    {
        var globals = new ProjectJuntos.Algol24.Environment();
        var env = new ProjectJuntos.Algol24.Environment(globals);

        globals.Define("test", 1);
        var token = new Token(TokenType.Identifier, "test", null, 0, 0, "test");

        env.Assign(token, 2);

        Assert.Equal(2, env.Get(token));
    }

    [Fact]
    public void ThrowsOnAssignToUndefined()
    {
        var globals = new ProjectJuntos.Algol24.Environment();
        var env = new ProjectJuntos.Algol24.Environment(globals);
        var token = new Token(TokenType.Identifier, "test", null, 0, 0, "test");

        var ex = Assert.Throws<RuntimeError>(() => env.Assign(token, 2));
        Assert.Equal("Undefined variable 'test'.", ex.Message);
    }

    [Fact]
    public void ThrowsOnGetFromUndefined()
    {
        var globals = new ProjectJuntos.Algol24.Environment();
        var token = new Token(TokenType.Identifier, "test", null, 0, 0, "test");

        var ex = Assert.Throws<RuntimeError>(() => globals.Get(token));
        Assert.Equal("Undefined variable 'test'.", ex.Message);
    }

    [Fact]
    public void CascadeAssignAffectsGlobalScope()
    {
        var globals = new ProjectJuntos.Algol24.Environment();
        var env = new ProjectJuntos.Algol24.Environment(globals);
        var functionEnv = new ProjectJuntos.Algol24.Environment(env);

        globals.Define("test", 1);
        var token = new Token(TokenType.Identifier, "test", null, 0, 0, "test");

        functionEnv.Assign(token, 2);

        Assert.Equal(2, functionEnv.Get(token));
    }
}
