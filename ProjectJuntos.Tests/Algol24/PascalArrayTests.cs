// Author: Lucia 🌟

using ProjectJuntos;
using ProjectJuntos.Algol24;
using ProjectJuntos.Algol24.Tokens;
using ProjectJuntos.Algol24.NativeFunctions;

using System;
using System.Collections.Generic;
using Xunit;

namespace ProjectJuntos.Tests
{
    public class PascalArrayTests
    {
        // Tests creating an array, adding a value, getting the value, and checking the length.
        [Fact]
        public void TestArray()
        {
            var uut = new PascalArray(1);

            var set = (IPascalCallable)uut.Get(new Token(TokenType.Identifier, "set", null, 0, 0, "test"));
            Assert.Equal(2, set.Arity());

            var args = new List<object> { 0, "ABC" };
            set.Call(null, args);

            var get = (IPascalCallable)uut.Get(new Token(TokenType.Identifier, "get", null, 0, 0, "test"));
            Assert.Equal(1, get.Arity());

            args = new List<object> { 0 };
            var value = get.Call(null, args);
            Assert.Equal("ABC", value);

            var length = (double)uut.Get(new Token(TokenType.Identifier, "length", null, 0, 0, "test"));
            Assert.Equal(1, length);

            Assert.Equal("[ABC]", uut.ToString());
        }

        // Get invalid property should fail.
        [Fact]
        public void TestGetInvalidProperty()
        {
            var ex = Assert.Throws<RuntimeError>(() =>
            {
                var uut = new PascalArray(1);
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
                var uut = new PascalArray(1);
                uut.Set(new Token(TokenType.Identifier, "invalid", null, 0, 0, "test"), 1);
            });

            Assert.Equal("Can't add properties to arrays.", ex.Message);
        }
    }
}
