using FluentAssertions;
using FluentNHibernate.Mapping;
using NUnit.Framework;

namespace NHibernateTesting.Tests.Relacoes.Unidirecionais.ComTabelaDeLigacao
{
    public class ManyToOneComTabelaDeLigacaoTestes : TestCase
    {
        public class PessoaMap : ClassMap<Pessoa>
        {
            public PessoaMap()
            {
                Id(x => x.Id);
                Map(x => x.Nome);
                Join("PessoaEndereco",
                     m =>
                     {
                         m.Optional();
                         m.KeyColumn("Pessoa_Id");
                         m.References(x => x.Endereco).Not.Nullable();
                     });
            }
        }

        public class EnderecoMap : ClassMap<Endereco>
        {
            public EnderecoMap()
            {
                Id(x => x.Id);
                Map(x => x.Logradouro);
                Map(x => x.Numero);
            }
        }

        [Test]
        public void DevePersistirCorretamente()
        {
            var persistido = WithNew(session =>
            {
                var endereco = new Endereco
                {
                    Logradouro = "Av Rio Branco",
                    Numero = "12345"
                };

                session.Save(endereco);

                var pessoa = new Pessoa { Nome = "Fulano", Endereco = endereco };
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
