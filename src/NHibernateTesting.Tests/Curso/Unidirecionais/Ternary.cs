using FluentAssertions;
using FluentNHibernate.Mapping;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace NHibernateTesting.Tests.Curso.Unidirecionais
{
    public class Ternary : TestCase
    {
        [Test]
        public void DevePersistirCorretamente()
        {
            var persistido = WithNew(session =>
                                         {
                                             var chave = new Chave();
                                             session.Save(chave);
                                             var main = new Main
                                                            {
                                                                Valores = { { chave, new Valor() } }
                                                            };

                                             session.Save(main);
                                             return main;
                                         });

            WithNew(session =>
                        {
                            var recuperado = session.Get<Main>(persistido.Id);
                            var key = recuperado.Valores.Keys.First();
                            recuperado.Valores[key].ShouldHave().AllProperties().EqualTo(persistido.Valores.First().Value);
                        });
        }

        public class MainMap : ClassMap<Main>
        {
            public MainMap()
            {
                Id(x => x.Id);

                HasManyToMany(x => x.Valores)
                    .Table("ChaveValor")
                    .AsMap("Chave").AsTernaryAssociation().Cascade.AllDeleteOrphan();
            }
        }

        public class ChaveMap : ClassMap<Chave>
        {
            public ChaveMap()
            {
                Id(x => x.Id);
            }
        }

        public class ValorMap : ClassMap<Valor>
        {
            public ValorMap()
            {
                Id(x => x.Id);
            }
        }

        public class Main
        {
            public virtual int Id { get; set; }

            public virtual IDictionary<Chave, Valor> Valores { get; set; }

            public Main()
            {
                Valores = new Dictionary<Chave, Valor>();
            }
        }

        public class Chave
        {
            public virtual int Id { get; set; }
        }

        public class Valor
        {
            public virtual int Id { get; set; }
        }
    }
}
