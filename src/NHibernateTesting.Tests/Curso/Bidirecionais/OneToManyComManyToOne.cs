using FluentAssertions;
using FluentAssertions.Equivalency;
using FluentNHibernate.Mapping;
using NUnit.Framework;
using System.Collections.Generic;

namespace NHibernateTesting.Tests.Curso.Bidirecionais
{
    public class OneToManyComManyToOne : TestCase
    {
        public class PessoaMap : ClassMap<Pessoa>
        {
            public PessoaMap()
            {
                Id(x => x.Id);
                References(x => x.Endereco)
                    .Not.Nullable();
            }
        }

        public class EnderecoMap : ClassMap<Endereco>
        {
            public EnderecoMap()
            {
                Id(x => x.Id);
                HasMany(x => x.Pessoas)
                    .Inverse();
            }
        }

        [Test]
        public void DevePersistirCorretamente()
        {
            var persistido = WithNew(session =>
            {
                var endereco = new Endereco();
                session.Save(endereco);

                var pessoas =
                    new[]
                        {
                            new Pessoa {Endereco = endereco},
                            new Pessoa {Endereco = endereco}
                        };

                foreach (var pessoa in pessoas)
                {
                    endereco.Pessoas.Add(pessoa);
                    session.Save(pessoa);
                }

                return endereco;
            });

            WithNew(session =>
            {
                var recuperado = session.Get<Endereco>(persistido.Id);

                recuperado.ShouldHave()
                          .AllProperties()
                          .IncludingNestedObjects(CyclicReferenceHandling.Ignore)
                          .EqualTo(persistido);
            });
        }

        public class Pessoa
        {
            public virtual int Id { get; set; }
            public virtual Endereco Endereco { get; set; }
        }

        public class Endereco
        {
            public virtual int Id { get; set; }
            public virtual IList<Pessoa> Pessoas { get; set; }

            public Endereco()
            {
                Pessoas = new List<Pessoa>();
            }
        }
    }
}

