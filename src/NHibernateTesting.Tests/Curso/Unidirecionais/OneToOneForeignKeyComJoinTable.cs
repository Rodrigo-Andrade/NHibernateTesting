using FluentAssertions;
using FluentNHibernate.Mapping;
using NUnit.Framework;

namespace NHibernateTesting.Tests.Curso.Unidirecionais
{
    public class OneToOneForeignKeyComJoinTable : TestCase
    {
        public class PessoaMap : ClassMap<Pessoa>
        {
            public PessoaMap()
            {
                Id(x => x.Id);
                Join("PessoaEndereco",
                    m =>
                    {
                        m.Optional();
                        m.KeyColumn("Pessoa_Id");
                        m.References(x => x.Endereco)
                        .Not.Nullable()
                        .Unique();
                    });
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
                var endereco = new Endereco();
                session.Save(endereco);

                var pessoa = new Pessoa { Endereco = endereco };
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
    }
}
