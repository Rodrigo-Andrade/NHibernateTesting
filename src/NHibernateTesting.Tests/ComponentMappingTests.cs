using FluentAssertions;
using FluentNHibernate.Mapping;
using NUnit.Framework;

namespace NHibernateTesting.Tests
{
    public class ComponentMappingTests : TestCase
    {
        [Test]
        public void ShouldPersistComponent()
        {
            var persisted = new User
            {
                Name = "User Name",
                Address = new Address
                {
                    City = "City",
                    Number = "Number",
                    Street = "Street"
                }
            };

            WithNew(session => { session.Save(persisted); });

            WithNew(session =>
                {
                    var retrieved = session.Get<User>(persisted.Id);

                    retrieved
                        .ShouldHave().AllProperties()
                        .IncludingNestedObjects()
                        .EqualTo(persisted);
                });
        }

        [Test]
        public void EmptyComponentWillBeRetrievedAsNull()
        {
            var persisted = new User
            {
                Name = "User Name",
                Address = new Address()
            };

            WithNew(session => { session.Save(persisted); });

            WithNew(session =>
            {
                var retrieved = session.Get<User>(persisted.Id);
                retrieved.Address.Should().BeNull();
            });
        }

        public class User
        {
            public virtual int Id { get; protected set; }
            public virtual string Name { get; set; }
            public virtual Address Address { get; set; }
        }

        public class Address
        {
            public string Street { get; set; }
            public string Number { get; set; }
            public string City { get; set; }
        }

        public class UserMap : ClassMap<User>
        {
            public UserMap()
            {
                Id(x => x.Id);
                Map(x => x.Name);

                Component(x => x.Address, c =>
                    {
                        c.Map(x => x.Street);
                        c.Map(x => x.Number);
                        c.Map(x => x.City);
                    });
            }
        }
    }
}
