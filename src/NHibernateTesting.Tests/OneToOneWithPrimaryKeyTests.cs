using FluentAssertions;
using FluentAssertions.Equivalency;
using FluentNHibernate.Mapping;
using NHibernate;
using NUnit.Framework;
using System;

namespace NHibernateTesting.Tests
{
    public class OneToOneWithPrimaryKeyTests : TestCase
    {
        [Test]
        public void CanNavigateAssociation()
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

            persisted.Address.User = persisted;

            WithNew(session =>
            {
                session.Save(persisted.Address);
                session.Save(persisted);
            });

            WithNew(session =>
            {
                var retrieved = session.Get<User>(persisted.Id);

                retrieved.ShouldHave().AllProperties()
                         .IncludingNestedObjects(CyclicReferenceHandling.Ignore)
                         .EqualTo(persisted);
            });
        }

        [Test]
        public void WillNotAllowMoreThenOneAssocioation()
        {
            var persisted = WithNew(session =>
            {
                var user = new User
                {
                    Name = "User Name",
                    Address = new Address
                    {
                        City = "City",
                        Number = "123",
                        Street = "Street"
                    }
                };
                user.Address.User = user;
                session.Save(user.Address);
                session.Save(user);
                return user;
            });

            var newPersisted = WithNew(session =>
            {
                var retrieved = session.Get<User>(persisted.Id);
                var address = retrieved.Address;
                var newUser = new User { Address = address };
                //NHibernate will ignore this the new assigned association
                address.User = newUser;
                session.Save(newUser);
                return newUser;
            });

            WithNew(session =>
            {
                var retrivedNewUser = session.Get<User>(newPersisted.Id);
                retrivedNewUser.Address.Should().BeNull();
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
            public virtual int Id { get; protected set; }
            public virtual string Street { get; set; }
            public virtual string Number { get; set; }
            public virtual string City { get; set; }

            public virtual User User { get; set; }
        }

        public class UserMap : ClassMap<User>
        {
            public UserMap()
            {
                Id(x => x.Id);
                Map(x => x.Name);

                HasOne(x => x.Address);
            }
        }

        public class AddressMap : ClassMap<Address>
        {
            public AddressMap()
            {
                Id(x => x.Id).GeneratedBy.Foreign("User");
                Map(x => x.Street);
                Map(x => x.Number);
                Map(x => x.City);

                HasOne(x => x.User);
            }
        }
    }
}
