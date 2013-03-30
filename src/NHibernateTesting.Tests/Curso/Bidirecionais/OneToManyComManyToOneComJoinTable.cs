using FluentAssertions;
using FluentAssertions.Equivalency;
using FluentNHibernate.Mapping;
using NUnit.Framework;
using System.Collections.Generic;

namespace NHibernateTesting.Tests.Curso.Bidirecionais
{
    public class OneToManyComManyToOneComJoinTable : TestCase
    {
        public class PessoaMap : ClassMap<Pessoa>
        {
            public PessoaMap()
            {
                Id(x => x.Id);
                HasManyToMany(x => x.Enderecos)
                    .Table("PessoaEndereco")
                    .ChildKeyColumns.Add("Endereco_Id", x => x.Unique());
            }
        }

        public class EnderecoMap : ClassMap<Endereco>
        {
            public EnderecoMap()
            {
                Id(x => x.Id);
                Join("PessoaEndereco",
                        m =>
                        {
                            m.Inverse();
                            m.Optional();
                            m.KeyColumn("Endereco_id");
                            m.References(x => x.Pessoa).Not.Nullable();
                        });
            }
        }

        [Test]
        public void DevePersistirCorretamente()
        {
            var persistido = WithNew(session =>
            {
                var pessoa = new Pessoa();
                session.Save(pessoa);

                var livros = new[]
                {
                    new Endereco {Pessoa = pessoa},
                    new Endereco {Pessoa = pessoa}
                };

                foreach (var livro in livros)
                {
                    session.Save(livro);
                    pessoa.Enderecos.Add(livro);
                }

                return pessoa;
            });

            WithNew(session =>
            {
                var recuperado = session.Get<Pessoa>(persistido.Id);

                recuperado
                    .ShouldHave()
                    .AllProperties()
                    .IncludingNestedObjects(CyclicReferenceHandling.Ignore)
                    .EqualTo(persistido);
            });
        }


        public class Pessoa
        {
            public virtual int Id { get; protected set; }
            public virtual IList<Endereco> Enderecos { get; protected set; }

            public Pessoa()
            {
                Enderecos = new List<Endereco>();
            }
        }

        public class Endereco
        {
            public virtual int Id { get; protected set; }
            public virtual Pessoa Pessoa { get; set; }
        }
    }
}
