using FluentAssertions;
using FluentNHibernate.Mapping;
using Iesi.Collections.Generic;
using NUnit.Framework;

namespace NHibernateTesting.Tests.Colecoes
{
    public class Sets : TestCase
    {
        public class PessoaMap : ClassMap<Pessoa>
        {
            public PessoaMap()
            {
                Id(x => x.Id);
                HasMany(x => x.Enderecos)
                    .AsSet();
            }
        }

        public class EnderecoMap : ClassMap<Endereco>
        {
            public EnderecoMap()
            {
                Id(x => x.Id);
            }
        }

        [Test]
        public void DevePersistirCorretamente()
        {
            var persistido = WithNew(session =>
            {
                var pessoa = new Pessoa
                                {
                                    Enderecos =
                                        {
                                            new Endereco(),
                                            new Endereco()
                                        }
                                };

                foreach (var endereco in pessoa.Enderecos)
                    session.Save(endereco);

                session.Save(pessoa);

                return pessoa;
            });

            WithNew(session =>
            {
                var recuperado = session.Get<Pessoa>(persistido.Id);

                recuperado.ShouldHave()
                          .AllProperties()
                          .IncludingNestedObjects()
                          .EqualTo(persistido);
            });
        }

        public class Pessoa
        {
            public virtual int Id { get; set; }
            public virtual ISet<Endereco> Enderecos { get; set; }

            public Pessoa()
            {
                Enderecos = new HashedSet<Endereco>();
            }
        }

        public class Endereco
        {
            public virtual int Id { get; set; }
        }
    }
}
