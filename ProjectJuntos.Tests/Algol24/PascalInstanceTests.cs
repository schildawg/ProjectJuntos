using System.Collections.Generic;
using ProjectJuntos;
using ProjectJuntos.Algol24;
using ProjectJuntos.Algol24.Tokens;

using Xunit;

namespace ProjectJuntos.Tests
{
    public class PascalInstanceTests
    {
        [Fact]
        public void TestCreate()
        {
            var klass = new PascalClass("Bagel", null, new Dictionary<string, PascalFunction>());
            var uut = new PascalInstance(klass);

            Assert.Equal("Bagel instance", uut.ToString());
        }

        [Fact]
        public void TestInstanceGet()
        {
            var klass = new PascalClass("Bagel", null, new Dictionary<string, PascalFunction>());
            var uut = new PascalInstance(klass);

            var token = new Token(TokenType.String, "ABC", null, 1, 0, "test");

            uut.Set(token, 123.0);
            Assert.Equal(123.0, uut.Get(token));
        }

        [Fact]
        public void TestInstanceGetUndefined()
        {
            var klass = new PascalClass("Bagel", null, new Dictionary<string, PascalFunction>());
            var uut = new PascalInstance(klass);

            var ex = Assert.Throws<RuntimeError>(() =>
            {
                uut.Get(new Token(TokenType.String, "ABC", null, 1, 0, "test"));
            });

            Assert.Equal("Undefined property 'ABC'.", ex.Message);
        }
    }
}
