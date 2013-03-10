using FluentAssertions;
using FluentNHibernate.Mapping;
using NUnit.Framework;
using System.Linq;

namespace NHibernateTesting.Tests.Net
{
    public class ReflectionTests
    {
        public class Foo
        {
            public class Bar { }
            public class Fee { }
        }

        [Test]
        public void CanRetrieveNextedClasses()
        {
            var nextedTypes = typeof(Foo).GetNestedTypes();
            nextedTypes.ShouldBeEquivalentTo(new object[] { typeof(Foo.Bar), typeof(Foo.Fee) });
        }

        public class FooMap : ClassMap<Foo>
        {
        }

        public class BarMap : ClassMap<Foo.Bar>
        {
        }

        [Test]
        public void CanRetrieveAllNextedClassesThatDerivesFromClassMap()
        {
            var classMaps = from type in typeof (ReflectionTests).GetNestedTypes()
                            where type.BaseType != null
                            let baseType = type.BaseType
                            where baseType.IsGenericType
                            where baseType.GetGenericTypeDefinition().IsAssignableFrom(typeof (ClassMap<>))
                            select type;

            classMaps.ShouldBeEquivalentTo(new object[] { typeof(FooMap), typeof(BarMap) });
        }
    }
}
