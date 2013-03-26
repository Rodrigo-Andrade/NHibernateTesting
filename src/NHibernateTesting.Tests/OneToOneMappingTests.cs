using System;
using FluentAssertions;
using FluentNHibernate.Mapping;
using NHibernate;
using NUnit.Framework;

namespace NHibernateTesting.Tests
{
    public class OneToOneWithForeignKeyMappingTests : TestCase
    {
        [Test]
        public void WillNotAllowMoreThenOneAssociation()
        {
            var persisted = new User
            {
                Name = "User Name",
                Address = new Address
                {
                    City = "City",
                    Number = "123",
                    Street = "Street"
                }
            };

            WithNew(session =>
                {
                    session.Save(persisted.Address);
                    session.Save(persisted);
                });

            var userId = persisted.Id;
            Action action =
                () =>
                WithNew(session =>
                    {
                        var retrieved = session.Get<User>(userId);
                        var newUser = new User {Address = retrieved.Address};
                        session.Save(newUser);
                    });

            action.ShouldThrow<HibernateException>();
        }

        public class User
        {
            public virtual int Id { get; protected set; }
            public virtual string Name { get; set; }

            public virtual Address Address { get; set; }
        }

        public class Address
        {
            public virtual int Id { get; protected set; }
            public virtual string Street { get; set; }
            public virtual string Number { get; set; }
            public virtual string City { get; set; }
        }

        public class UserMap : ClassMap<User>
        {
            public UserMap()
            {
                Id(x => x.Id);
                Map(x => x.Name);

                References(x => x.Address)
                    .Unique();
            }
        }

        public class AddressMap : ClassMap<Address>
        {
            public AddressMap()
            {
                Id(x => x.Id);
                Map(x => x.Street);
                Map(x => x.Number);
                Map(x => x.City);
            }
        }
    }
}
