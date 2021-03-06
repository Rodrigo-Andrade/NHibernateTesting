﻿using System;
using FluentAssertions;
using FluentNHibernate.Mapping;
using NUnit.Framework;
using System.Collections.Generic;

namespace NHibernateTesting.Tests
{
    public class OneToManyMappingTests : TestCase
    {
        [Test]
        public void ShouldRetrieveCollection()
        {
            var persisted = new User
            {
                Name = "User Name",
                Addresses =
                        {
                            new Address
                                {
                                    City = "City 01",
                                    Number = "123",
                                    Street = "Street 01"
                                },
                            new Address
                                {
                                    City = "City 02",
                                    Number = "456",
                                    Street = "Street 02"
                                }
                        }
            };

            WithNew(session =>
            {
                foreach (var address in persisted.Addresses)
                    session.Save(address);
                session.Save(persisted);
            });

            WithNew(session =>
            {
                var retrieved = session.Get<User>(persisted.Id);

                retrieved.ShouldHave().AllProperties()
                         .IncludingNestedObjects()
                         .EqualTo(persisted);
            });
        }

        public class User
        {
            public virtual int Id { get; protected set; }
            public virtual string Name { get; set; }

            public virtual IList<Address> Addresses { get; set; }

            public User()
            {
                Addresses = new List<Address>();
            }

            protected bool Equals(User other)
            {
                return Id == other.Id;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((User) obj);
            }

            public override int GetHashCode()
            {
                return Id;
            }

            public static bool operator ==(User left, User right)
            {
                return Equals(left, right);
            }

            public static bool operator !=(User left, User right)
            {
                return !Equals(left, right);
            }
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

                HasMany(x => x.Addresses);
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
