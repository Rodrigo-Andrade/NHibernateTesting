using FluentAssertions;
using FluentAssertions.Equivalency;
using FluentNHibernate.Mapping;
using NUnit.Framework;

namespace NHibernateTesting.Tests.Relacoes.Bidirecionais
{
    public class OneToOnePorChavePrimariaTestes : TestCase
    {
        public class PessoaMap : ClassMap<Pessoa>
        {
            public PessoaMap()
            {
                Id(x => x.Id);
                Map(x => x.Nome);
                HasOne(x => x.Endereco);
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

        [Test]
        public void DevePersistirCorretamente()
        {
            var persistido = WithNew(session =>
            {
                var pessoa = new Pessoa{Nome = "Fulano"};
                session.Save(pessoa);

                var endereco = new Endereco
                                   {
                                       Logradouro = "Av Rio Branco",
                                       Numero = "12345",
                                       Pessoa = pessoa
                                   };

                session.Save(endereco);

                //não faz diferenca no banco, está aqui apenas para manter a consistencia entre os objetos
                pessoa.Endereco = endereco;

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
