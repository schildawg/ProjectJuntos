using System;
using Xunit;
using ProjectJuntos;
using ProjectJuntos.Algol24;

namespace ProjectJuntos.Tests
{
    /// <summary>
    /// Tests PascalEnum.
    /// </summary>
    public class PascalEnumTest
    {
        // Tests creating an Enum.
        [Fact]
        public void TestCreate()
        {
            var uut = new PascalEnum("Color", "Red", 1);
            Assert.Equal("Red", uut.ToString());
        }
    }
}
