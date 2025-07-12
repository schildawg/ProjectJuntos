// Author: Lucia 🌟

using ProjectJuntos;
using ProjectJuntos.Algol24;
using ProjectJuntos.Algol24.Tokens;
using ProjectJuntos.Algol24.NativeFunctions;

using System.Collections.Generic;
using Xunit;

namespace ProjectJuntos.Tests
{
    public class PascalListTests
    {
        // Tests creating a list, adding a value, getting the value, and checking the length.
        [Fact]
        public void TestList()
        {
            var uut = new PascalList();

            var add = (IPascalCallable)uut.Get(new Token(TokenType.Identifier, "add", null, 0, 0, "test"));
            Assert.Equal(1, add.Arity());

            var args = new List<object> { "ABC" };
            add.Call(null, args);

            var get = (IPascalCallable)uut.Get(new Token(TokenType.Identifier, "get", null, 0, 0, "test"));
            Assert.Equal(1, get.Arity());

            args = new List<object> { 0 };
            var value = get.Call(null, args);
            Assert.Equal("ABC", value);

            var length = (double)uut.Get(new Token(TokenType.Identifier, "length", null, 0, 0, "test"));
            Assert.Equal(1.0, length);

            Assert.Equal("[ABC]", uut.ToString());
        }

        // Get invalid property should fail.
        [Fact]
        public void TestGetInvalidProperty()
        {
            var ex = Assert.Throws<RuntimeError>(() =>
            {
                var uut = new PascalList();
                uut.Get(new Token(TokenType.Identifier, "invalid", null, 0, 0, "test"));
            });

            Assert.Equal("Undefined property 'invalid'.", ex.Message);
        }

        // Set invalid property should fail.
        [Fact]
        public void TestSetInvalidProperty()
        {
            var ex = Assert.Throws<RuntimeError>(() =>
            {
                var uut = new PascalList();
                uut.Set(new Token(TokenType.Identifier, "invalid", null, 0, 0, "test"), 1);
            });

            Assert.Equal("Can't add properties to lists.", ex.Message);
        }
    }
}
