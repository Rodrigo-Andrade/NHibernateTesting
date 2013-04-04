using System;
using System.Collections.Generic;
using FluentAssertions;
using FluentNHibernate.Mapping;
using NUnit.Framework;

namespace NHibernateTesting.Tests.Colecoes
{
    public class Map : TestCase
    {
        [Test]
        public void DevePersistirCorretamente()
        {
            var persistido = WithNew(session =>
                         {
                             var calendario = new Calendario();

                             calendario.Feriados.Add("Ano novo", new DateTime(2013, 01, 01));
                             calendario.Feriados.Add("Natal", new DateTime(2013, 12, 25));

                             session.Save(calendario);

                             return calendario;
                         });

            WithNew(session =>
                        {
                            var recuperado = session.Get<Calendario>(persistido.Id);

                            recuperado.Feriados["Ano novo"].Should().Be(new DateTime(2013, 01, 01));
                            recuperado.Feriados["Natal"].Should().Be(new DateTime(2013, 12, 25));

                            recuperado
                                .ShouldHave()
                                .AllProperties()
                                .IncludingNestedObjects()
                                .EqualTo(persistido);
                        });
        }

        public class CalendarioMap : ClassMap<Calendario>
        {
            public CalendarioMap()
            {
                Id(x => x.Id);

                HasMany(x => x.Feriados)
                    .Cascade.AllDeleteOrphan()
                    .AsMap<string>("NomeFeriado")
                    .Element("DataFeriado");
            }
        }


        public class Calendario
        {
            public virtual int Id { get; set; }
            public virtual IDictionary<string, DateTime> Feriados { get; set; }

            public Calendario()
            {
                Feriados = new Dictionary<string, DateTime>();
            }
        }
    }
}
