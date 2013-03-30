using FluentAssertions;
using FluentAssertions.Equivalency;
using FluentNHibernate.Mapping;
using NUnit.Framework;

namespace NHibernateTesting.Tests.Curso.Bidirecionais
{
    public class OneToOnePorChaveEstrangeira : TestCase
    {
        public class PessoaMap : ClassMap<Pessoa>
        {
            public PessoaMap()
            {
                Id(x => x.Id);
                References(x => x.Endereco).Unique();
            }
        }

        public class EnderecoMap : ClassMap<Endereco>
        {
            public EnderecoMap()
            {
                Id(x => x.Id);
                HasOne(x => x.Pessoa).PropertyRef(x => x.Endereco);
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
            public virtual Endereco Endereco { get; set; }
        }

        public class Endereco
        {
            public virtual int Id { get; set; }
            public virtual Pessoa Pessoa { get; set; }
        }
    }
}
