using FluentAssertions;
using FluentNHibernate.Mapping;
using NUnit.Framework;

namespace NHibernateTesting.Tests.Relacoes.Bidirecionais.ComTabelaDeLigacao
{
    public class OneToOneComTabelaDeLigacaoTestes : TestCase
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
                         m.KeyColumn("Pessoa_id");
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
                Map(x => x.Logradouro);
                Map(x => x.Numero);

                Join("PessoaEndereco",
                     m =>
                     {
                         m.Optional();
                         m.Inverse();
                         m.KeyColumn("Endereco_id");
                         m.References(x => x.Pessoa)
                          .Not.Nullable()
                          .Unique();
                     });
            }
        }

        [Test]
        [Ignore("chicken egg")]
        public void DevePersistirCorretamente()
        {
            var persistido = WithNew(session =>
            {
                var pessoa = new Pessoa { Nome = "Fulano" };

                var endereco = new Endereco
                {
                    Logradouro = "Av Rio Branco",
                    Numero = "12345",
                    Pessoa = pessoa
                };
                pessoa.Endereco = endereco;

                session.Save(pessoa);
                session.Save(endereco);


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
            public virtual string Nome { get; set; }

            public virtual Endereco Endereco { get; set; }
        }

        public class Endereco
        {
            public virtual int Id { get; set; }
            public virtual string Logradouro { get; set; }
            public virtual string Numero { get; set; }

            public virtual Pessoa Pessoa { get; set; }
        }
    }
}
