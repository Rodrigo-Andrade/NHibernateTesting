using FluentAssertions;
using FluentNHibernate.Cfg;
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
                        .EqualTo(persisted);
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

            protected bool Equals(Address other)
            {
                return string.Equals(City, other.City) && string.Equals(Number, other.Number) && string.Equals(Street, other.Street);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != GetType()) return false;
                return Equals((Address)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    var hashCode = (City != null ? City.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (Number != null ? Number.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (Street != null ? Street.GetHashCode() : 0);
                    return hashCode;
                }
            }
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

        public override void Mappings(MappingConfiguration mappingConfig)
        {
            mappingConfig.FluentMappings.Add<UserMap>();
        }
    }
}
