using System.Collections.Generic;
using ProjectJuntos;
using ProjectJuntos.Algol24;

using Xunit;

namespace ProjectJuntos.Tests
{
    public class PascalClassTests
    {
        [Fact]
        public void TestCreate()
        {
            var uut = new PascalClass("Test", null, new Dictionary<string, PascalFunction>());

            Assert.Equal("Test", uut.ToString());
        }
    }
}
