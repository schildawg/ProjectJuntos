// Author: Lucia 🌟

using ProjectJuntos;
using ProjectJuntos.Algol24;


using System;
using Xunit;

namespace ProjectJuntos.Tests
{
    public class TypeLookupTests
    {
        // Tests adding a global.
        [Fact]
        public void TestGlobal()
        {
            var uut = new TypeLookup();
            uut.SetType("Abc", "String");

            var type = uut.GetType("Abc");

            Assert.Equal("String", type);
        }

        // Tests scoped lookups
        [Fact]
        public void TestScopedLookup()
        {
            var uut = new TypeLookup();
            uut.BeginScope();

            uut.SetType("Abc", "String");

            var type = uut.GetType("Abc");

            Assert.Equal("String", type);
        }

        // Tests scoped lookup chained
        [Fact]
        public void TestScopedLookupChained()
        {
            var uut = new TypeLookup();
            uut.SetType("Abc", "String");
            uut.BeginScope();
            uut.BeginScope();

            var type = uut.GetType("Abc");

            Assert.Equal("String", type);
        }

        // Tests out-of-scope.
        [Fact]
        public void TestOutOfScope()
        {
            var uut = new TypeLookup();
            uut.BeginScope();
            uut.SetType("Abc", "String");
            uut.EndScope();

            var type = uut.GetType("Abc");

            Assert.Null(type);
        }
    }
}
