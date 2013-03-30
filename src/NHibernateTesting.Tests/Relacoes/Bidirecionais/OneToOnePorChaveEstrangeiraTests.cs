using FluentAssertions;
using FluentAssertions.Equivalency;
using FluentNHibernate.Mapping;
using NUnit.Framework;

namespace NHibernateTesting.Tests.Relacoes.Bidirecionais
{
    public class OneToOnePorChaveEstrangeiraTestes : TestCase
    {
        public class PessoaMap : ClassMap<Pessoa>
        {
            public PessoaMap()
            {
                Id(x => x.Id);
                Map(x => x.Nome);
                References(x => x.Endereco).Unique();
            }
        }

        public class EnderecoMap : ClassMap<Endereco>
        {
            public EnderecoMap()
            {
                Id(x => x.Id);
                Map(x => x.Logradouro);
                Map(x => x.Numero);
                HasOne(x => x.Pessoa).PropertyRef(x => x.Endereco);
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
                    Numero = "12345",
                };
                session.Save(endereco);

                var pessoa = new Pessoa { Nome = "Fulano", Endereco = endereco};
                session.Save(pessoa);

                //não faz diferenca no banco, está aqui apenas para manter a consistencia entre os objetos
                endereco.Pessoa = pessoa;

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
