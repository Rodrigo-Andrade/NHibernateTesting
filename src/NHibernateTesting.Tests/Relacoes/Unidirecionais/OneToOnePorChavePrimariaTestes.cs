using FluentAssertions;
using FluentNHibernate.Mapping;
using NUnit.Framework;

namespace NHibernateTesting.Tests.Relacoes.Unidirecionais
{
    public class OneToOnePorChavePrimariaTestes : TestCase
    {
        [Test]
        public void DevePersistirCorretamente()
        {
            var persistido = WithNew(session =>
            {
                var pessoa = new Pessoa {Nome = "Fulano"};
                session.Save(pessoa);

                var endereco = new Endereco
                {
                    Logradouro = "Av Rio Branco",
                    Numero = "12345",
                    Pessoa = pessoa
                };

                session.Save(endereco);
                return endereco;
            });

            WithNew(session =>
            {
                var recuperado = session.Get<Endereco>(persistido.Id);

                recuperado.ShouldHave()
                          .AllProperties()
                          .IncludingNestedObjects()
                          .EqualTo(persistido);
            });
        }

        public class Pessoa
        {
            public virtual int Id { get; set; }
            public virtual string Nome { get; set; }
        }

        public class Endereco
        {
            public virtual int Id { get; set; }
            public virtual string Logradouro { get; set; }
            public virtual string Numero { get; set; }

            public virtual Pessoa Pessoa { get; set; }
        }

        public class PessoaMap : ClassMap<Pessoa>
        {
            public PessoaMap()
            {
                Id(x => x.Id);
                Map(x => x.Nome);
            }
        }

        public class EnderecoMap : ClassMap<Endereco>
        {
            public EnderecoMap()
            {
                Id(x => x.Id).GeneratedBy.Foreign("Pessoa");
                Map(x => x.Logradouro);
                Map(x => x.Numero);
                HasOne(x => x.Pessoa).Constrained();
            }
        }
    }
}
