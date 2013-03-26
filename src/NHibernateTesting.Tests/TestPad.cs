using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using FluentNHibernate.Mapping;
using NHibernate;
using NUnit.Framework;

namespace NHibernateTesting.Tests
{
    public class TestPad : TestCase
    {
        public class CartContext
        {
            public Cart Cart { get; private set; }

            public CartContext(Cart cart)
            {
                Cart = cart;
            }

            public void SetCart(Cart cart)
            {
                Cart = cart;
            }
        }

        [Test]
        public void MergeWithNew()
        {
            var station = new Station { Name = "Estacao" };
            var movie = new Movie { Name = "Filme" };
            var sessao = new Session { Movie = movie };

            WithNew(session =>
            {
                session.Save(station);
                session.Save(movie);
                session.Save(sessao);
            });

            var cart = new Cart { Station = station };
            cart.Tickets.Add(new Ticket { Session = sessao });

            WithNew(session =>
                        {
                            cart = session.Merge(cart);
                        });

            cart.Tickets.Add(new Ticket { Session = sessao });

            WithNew(session =>
            {
                cart = session.Merge(cart);

                Console.WriteLine(cart.Station.Name);
            });
        }

        [Test]
        public void DisconnectedScenario()
        {
            var c = WithNew(session =>
                         {
                             var station = new Station { Name = "Estacao" };
                             session.Save(station);

                             var cart = new Cart { Station = station };
                             session.Save(cart);

                             var movie = new Movie { Name = "Filme" };
                             session.Save(movie);

                             var sessao = new Session { Movie = movie };
                             session.Save(sessao);

                             var ticket = new Ticket { Session = sessao };
                             cart.Tickets.Add(ticket);

                             return cart;
                         });
            //var s = c.Tickets.First().Session;
            //c.Tickets.Add(new Ticket { Session = s });


            WithNew(session =>
                        {
                            //session.Merge(c);
                            //session.SaveOrUpdate(c);

                            session.Lock(c, LockMode.None);

                            var s = c.Tickets.First().Session;
                            c.Tickets.Add(new Ticket { Session = s });
                        });

            WithNew(session =>
                        {
                            var cart = session.Get<Cart>(1);

                            cart.Tickets.Count.Should().Be(2);
                            cart.Tickets.GroupBy(x => x.Session).Select(x => x.Key).Single().Id.Should().Be(1);
                        });
        }


        public class StationMap : ClassMap<Station>
        {
            public StationMap()
            {
                Id(x => x.Id);
                Map(x => x.Name);
            }
        }

        public class CartMap : ClassMap<Cart>
        {
            public CartMap()
            {
                Id(x => x.Id);
                References(x => x.Station)
                    .Cascade.Merge();

                HasMany(x => x.Tickets)
                    .Cascade
                    .AllDeleteOrphan();
            }
        }

        public class TicketMap : ClassMap<Ticket>
        {
            public TicketMap()
            {
                Id(x => x.Id);
                References(x => x.Session);
            }
        }

        public class SessionMap : ClassMap<Session>
        {
            public SessionMap()
            {
                Id(x => x.Id);
                References(x => x.Movie);
            }
        }

        public class MovieMap : ClassMap<Movie>
        {
            public MovieMap()
            {
                Id(x => x.Id);
                Map(x => x.Name);
            }
        }

        public class Station
        {
            public virtual int Id { get; set; }
            public virtual string Name { get; set; }
        }

        public class Cart
        {
            public virtual int Id { get; set; }
            public virtual Station Station { get; set; }

            public virtual IList<Ticket> Tickets { get; set; }

            public Cart()
            {
                Tickets = new List<Ticket>();
            }
        }

        public class Ticket
        {
            public virtual int Id { get; set; }
            public virtual Session Session { get; set; }
        }

        public class Session
        {
            public virtual int Id { get; set; }
            public virtual Movie Movie { get; set; }
        }

        public class Movie
        {
            public virtual int Id { get; set; }
            public virtual string Name { get; set; }
        }
    }
}
