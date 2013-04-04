using System;
using System.Collections.Generic;
using FluentAssertions;
using FluentNHibernate.Mapping;
using Iesi.Collections.Generic;
using NUnit.Framework;

namespace NHibernateTesting.Tests.Colecoes
{
    public class SortedSet : TestCase
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
            public virtual Iesi.Collections.Generic.ISet<Endereco> Enderecos { get; set; }

            public Pessoa()
            {
                Enderecos = new OrderedSet<Endereco>();
            }
        }

        public class Endereco
        {
            public virtual int Id { get; set; }
            public virtual string Logradouro { get; set; }
        }

        public class EnderecoComparer : IComparer<Endereco>
        {
            public int Compare(Endereco x, Endereco y)
            {
                return string.Compare(x.Logradouro, y.Logradouro, StringComparison.InvariantCulture);
            }
        }
    }
}
