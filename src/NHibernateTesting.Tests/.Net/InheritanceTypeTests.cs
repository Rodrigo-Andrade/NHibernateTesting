using System;
using FluentAssertions;
using NUnit.Framework;

namespace NHibernateTesting.Tests.Net
{
    public class InheritanceTypeTests
    {
        public class Foo
        {
            public virtual Type ReturnType()
            {
                return GetType();
            }
        }

        public class Bar : Foo{}

        [Test]
        public void DerivedTypeShouldReturnTypeOfDerivedType()
        {
            var bar = new Bar();

            bar.ReturnType().Should().Be(typeof(Bar));
        }
    }
}