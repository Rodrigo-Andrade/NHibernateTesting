using FluentAssertions;
using FluentNHibernate.Mapping;
using System;

namespace NHibernateTesting.Tests
{
    public class ManyToOneMappingTests : TestCase
    {
        public void ShouldRetrieveAssociation()
        {
            var persisted = new Order
            {
                Date = new DateTime(2013, 01, 01),
                User = new User
                {
                    Name = "User Name"
                }
            };

            WithNew(session =>
            {
                session.Save(persisted.User);
                session.Save(persisted);
            });

            WithNew(session =>
            {
                var retrieved = session.Get<Order>(persisted.Id);

                retrieved.ShouldHave()
                         .AllProperties().IncludingNestedObjects()
                         .EqualTo(persisted);
            });
        }

        public class User
        {
            public virtual int Id { get; protected set; }
            public virtual string Name { get; set; }
        }

        public class Order
        {
            public virtual int Id { get; protected set; }
            public virtual DateTime Date { get; set; }

            public virtual User User { get; set; }
        }

        public class UserMap : ClassMap<User>
        {
            public UserMap()
            {
                Id(x => x.Id);
                Map(x => x.Name);
            }
        }

        public class OrderMap : ClassMap<Order>
        {
            public OrderMap()
            {
                Id(x => x.Id);
                Map(x => x.Date);

                References(x => x.User);
            }
        }
    }
}
