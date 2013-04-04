using System.Threading.Tasks;
using FluentAssertions;
using FluentNHibernate.Mapping;
using NUnit.Framework;
using System.Collections.Generic;

namespace NHibernateTesting.Tests.Colecoes
{
    public class IndexedLists : TestCase
    {
        private Pessoa _pessoa;
        private Endereco _endereco01;
        private Endereco _endereco02;

        public class PessoaMap : ClassMap<Pessoa>
        {
            public PessoaMap()
            {
                Id(x => x.Id);
                HasMany(x => x.Enderecos)
                    .AsList(x => x.Column("Ordem"));
            }
        }

        protected override void Seed()
        {
            WithNew(session =>
            {
                _pessoa = new Pessoa();
                _endereco01 = new Endereco();
                _endereco02 = new Endereco();

                session.Save(_endereco01);
                session.Save(_endereco02);
                session.Save(_pessoa);

                _pessoa.Enderecos.Insert(0, _endereco01);
                _pessoa.Enderecos.Insert(1, _endereco02);
            });
        }

        public class EnderecoMap : ClassMap<Endereco>
        {
            public EnderecoMap()
            {
                Id(x => x.Id);
            }
        }

        [Test]
        public void DeveRecuperarListaOrdenadaCorretamente()
        {
            WithNew(session =>
                        {
                            var recuperado = session.Get<Pessoa>(_pessoa.Id);

                            recuperado.Enderecos[0].Id.Should().Be(_endereco01.Id);
                            recuperado.Enderecos[1].Id.Should().Be(_endereco02.Id);
                        });
        }

        [Test]
        public void DeveRecuperarListaReOrdenadaCorretamente()
        {
            WithNew(session =>
            {
                var recuperado = session.Get<Pessoa>(_pessoa.Id);

                var aux = recuperado.Enderecos[0];
                recuperado.Enderecos[0] = recuperado.Enderecos[1];
                recuperado.Enderecos[1] = aux;
            });

            WithNew(session =>
            {
                var recuperado = session.Get<Pessoa>(_pessoa.Id);

                recuperado.Enderecos[0].Id.Should().Be(_endereco02.Id);
                recuperado.Enderecos[1].Id.Should().Be(_endereco01.Id);
            });
        }

        [Test]
        public void AdicionarItemNaListaMantemOrdemCorreta()
        {
            var endereco03 = WithNew(session =>
            {
                var recuperado = session.Get<Pessoa>(_pessoa.Id);

                var novoEndereco = new Endereco();
                session.Save(novoEndereco);
                recuperado.Enderecos.Add(novoEndereco);

                return novoEndereco;
            });

            WithNew(session =>
            {
                var recuperado = session.Get<Pessoa>(_pessoa.Id);

                recuperado.Enderecos[0].Id.Should().Be(_endereco01.Id);
                recuperado.Enderecos[1].Id.Should().Be(_endereco02.Id);
                recuperado.Enderecos[2].Id.Should().Be(endereco03.Id);
            });
        }

        public class Pessoa
        {
            public virtual int Id { get; set; }
            public virtual IList<Endereco> Enderecos { get; set; }

            public Pessoa()
            {
                Enderecos = new List<Endereco>();
            }
        }

        public class Endereco
        {
            public virtual int Id { get; set; }
        }
    }
}
