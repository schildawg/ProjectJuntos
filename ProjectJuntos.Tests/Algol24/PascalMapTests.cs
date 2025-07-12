using System.Collections.Generic;
using ProjectJuntos;
using ProjectJuntos.Algol24;
using ProjectJuntos.Algol24.Tokens;
using ProjectJuntos.Algol24.NativeFunctions;

using Xunit;

namespace ProjectJuntos.Tests
{
    public class PascalMapTests
    {
        [Fact]
        public void TestMap()
        {
            var uut = new PascalMap();

            var add = (IPascalCallable) uut.Get(new Token(TokenType.Identifier, "put", null, 0, 0, "test"));
            Assert.Equal(2, add.Arity());

            var args = new List<object> { 1, "ABC" };
            add.Call(null, args);

            var get = (IPascalCallable) uut.Get(new Token(TokenType.Identifier, "get", null, 0, 0, "test"));
            Assert.Equal(1, get.Arity());

            args = new List<object> { 1 };
            var value = get.Call(null, args);
            Assert.Equal("ABC", value);

            var contains = (IPascalCallable) uut.Get(new Token(TokenType.Identifier, "contains", null, 0, 0, "test"));
            Assert.Equal(1, contains.Arity());

            args = new List<object> { 1 };
            var result = contains.Call(null, args);
            Assert.True((bool)result);

            Assert.Equal("{[1, ABC]}", uut.ToString());
        }

        [Fact]
        public void TestGetInvalidProperty()
        {
            var ex = Assert.Throws<RuntimeError>(() =>
            {
                var uut = new PascalMap();
                uut.Get(new Token(TokenType.Identifier, "invalid", null, 0, 0, "test"));
            });

            Assert.Equal("Undefined property 'invalid'.", ex.Message);
        }

        [Fact]
        public void TestSetInvalidProperty()
        {
            var ex = Assert.Throws<RuntimeError>(() =>
            {
                var uut = new PascalMap();
                uut.Set(new Token(TokenType.Identifier, "invalid", null, 0, 0, "test"), 1);
            });

            Assert.Equal("Can't add properties to maps.", ex.Message);
        }
    }
}
