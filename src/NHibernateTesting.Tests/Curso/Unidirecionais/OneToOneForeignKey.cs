using FluentAssertions;
using FluentNHibernate.Mapping;
using NUnit.Framework;

namespace NHibernateTesting.Tests.Curso.Unidirecionais
{
    public class OneToOneForeignKey : TestCase
    {
        public class PessoaMap : ClassMap<Pessoa>
        {
            public PessoaMap()
            {
                Id(x => x.Id);
                References(x => x.Endereco)
                    .ForeignKey("FK_PESSOA_ENDERECO")
                    .Unique();
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

                            var pessoa = new Pessoa {Endereco = endereco};
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

    public class Pessoa
    {
        public virtual int Id { get; set; }
        public virtual Endereco Endereco { get; set; }
    }

    public class Endereco
    {
        public virtual int Id { get; set; }
    }
}
