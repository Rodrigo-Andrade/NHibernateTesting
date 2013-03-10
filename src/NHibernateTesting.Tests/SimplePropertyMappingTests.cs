using FluentAssertions;
using FluentNHibernate.Cfg;
using FluentNHibernate.Mapping;
using NUnit.Framework;
using System;

namespace NHibernateTesting.Tests
{
    public class SimplePropertyMappingTests : TestCase
    {
        [Test]
        public void ShouldBePersisted()
        {
            var persisted = new SimpleFlatClass
            {
                StringValue = "StringValue",
                DateTimeValue = new DateTime(2013, 01, 01, 02, 03, 04),
                BooleanValue = true,
                Decimalvalue = 10.31M,
                NulableInt = null,
                EnumValue = SimpleFlatClass.MyEnum.Value02
            };

            WithNew(session => { session.Save(persisted); });

            WithNew(session =>
                {
                    var retrived = session.Get<SimpleFlatClass>(persisted.Id);

                    retrived.
                        ShouldHave().AllProperties()
                            .EqualTo(persisted);
                });
        }

        [Test]
        public void WillUpdatePropertiesWithoutExplicitCallToUpdate()
        {
            var persisted = new SimpleFlatClass
            {
                StringValue = "StringValue",
                DateTimeValue = new DateTime(2013, 01, 01, 02, 03, 04),
                BooleanValue = true,
                Decimalvalue = 10.31M,
                NulableInt = null,
                EnumValue = SimpleFlatClass.MyEnum.Value02
            };

            WithNew(session => { session.Save(persisted); });

            WithNew(session =>
            {
                var retrived = session.Get<SimpleFlatClass>(persisted.Id);
                retrived.StringValue = "NewValue";
            });

            WithNew(session =>
            {
                var updated = session.Get<SimpleFlatClass>(persisted.Id);
                updated.StringValue.Should().Be("NewValue");
            });
        }

        [Test]
        public void ShouldDeleteEntity()
        {
            var persisted = new SimpleFlatClass
            {
                StringValue = "StringValue",
                DateTimeValue = new DateTime(2013, 01, 01, 02, 03, 04),
                BooleanValue = true,
                Decimalvalue = 10.31M,
                NulableInt = null,
                EnumValue = SimpleFlatClass.MyEnum.Value02
            };

            WithNew(session => { session.Save(persisted); });

            WithNew(session =>
            {
                var retrived = session.Get<SimpleFlatClass>(persisted.Id);
                session.Delete(retrived);
            });

            WithNew(session =>
            {
                var retrived = session.Get<SimpleFlatClass>(persisted.Id);
                retrived.Should().BeNull();
            });
        }

        [Test]
        public void WillMakeSelectWhenDeletingWithLoad()
        {
            var persisted = new SimpleFlatClass
            {
                StringValue = "StringValue",
                DateTimeValue = new DateTime(2013, 01, 01, 02, 03, 04),
                BooleanValue = true,
                Decimalvalue = 10.31M,
                NulableInt = null,
                EnumValue = SimpleFlatClass.MyEnum.Value02
            };

            WithNew(session => { session.Save(persisted); });

            SessionFactory.Statistics.Clear();

            WithNew(session =>
            {
                //At first i thought that you could delete an entity without loading it
                //but nhibernate will load the proxy when deleting, so it can determine cascades
                var retrived = session.Load<SimpleFlatClass>(persisted.Id);
                session.Delete(retrived);
            });

            SessionFactory.Statistics.PrepareStatementCount.Should().Be(2);

            WithNew(session =>
            {
                var retrived = session.Get<SimpleFlatClass>(persisted.Id);
                retrived.Should().BeNull();
            });
        }

        public class SimpleFlatClass
        {
            public virtual int Id { get; protected set; }
            public virtual string StringValue { get; set; }
            public virtual DateTime DateTimeValue { get; set; }
            public virtual decimal Decimalvalue { get; set; }
            public virtual bool BooleanValue { get; set; }
            public virtual int? NulableInt { get; set; }
            public virtual MyEnum EnumValue { get; set; }

            public enum MyEnum
            {
                Value01, Value02
            }
        }

        public class SimpleFlatClassMap : ClassMap<SimpleFlatClass>
        {
            public SimpleFlatClassMap()
            {
                Id(x => x.Id);
                Map(x => x.StringValue);
                Map(x => x.DateTimeValue);
                Map(x => x.Decimalvalue);
                Map(x => x.BooleanValue);
                Map(x => x.NulableInt);
                Map(x => x.EnumValue);
            }
        }

        public override void Mappings(MappingConfiguration mappingConfig)
        {
            mappingConfig.FluentMappings.Add<SimpleFlatClassMap>();
        }
    }
}
