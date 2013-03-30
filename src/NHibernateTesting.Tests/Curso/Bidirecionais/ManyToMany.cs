using FluentAssertions;
using FluentAssertions.Equivalency;
using FluentNHibernate.Mapping;
using NUnit.Framework;
using System.Collections.Generic;

namespace NHibernateTesting.Tests.Curso.Bidirecionais
{
    public class ManyToMany : TestCase
    {
        public class PessoaMap : ClassMap<Pessoa>
        {
            public PessoaMap()
            {
                Id(x => x.Id);
                HasManyToMany(x => x.Enderecos);
            }
        }

        public class EnderecoMap : ClassMap<Endereco>
        {
            public EnderecoMap()
            {
                Id(x => x.Id);
                HasManyToMany(x => x.Pessoas).Inverse();
            }
        }

        [Test]
        public void DevePersistirCorretamente()
        {
            var persistidos = WithNew(session =>
                        {
                            var pessoa01 = new Pessoa();
                            var pessoa02 = new Pessoa();

                            session.Save(pessoa01);
                            session.Save(pessoa02);

                            var endereco01 = new Endereco();
                            var endereco02 = new Endereco();
                            var endereco03 = new Endereco();

                            session.Save(endereco01);
                            session.Save(endereco02);
                            session.Save(endereco03);

                            pessoa01.Enderecos.Add(endereco01);
                            pessoa01.Enderecos.Add(endereco02);

                            pessoa02.Enderecos.Add(endereco02);
                            pessoa02.Enderecos.Add(endereco03);

                            endereco01.Pessoas.Add(pessoa01);

                            endereco02.Pessoas.Add(pessoa01);
                            endereco02.Pessoas.Add(pessoa02);

                            endereco03.Pessoas.Add(pessoa02);

                            return new[] { pessoa01, pessoa02 };
                        });

            WithNew(session =>
                        {
                            foreach (var persistido in persistidos)
                            {
                                var recuperado = session.Get<Pessoa>(persistido.Id);

                                recuperado
                                    .ShouldHave()
                                    .AllProperties()
                                    .IncludingNestedObjects(CyclicReferenceHandling.Ignore)
                                    .EqualTo(persistido);
                            }
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
            public virtual IList<Pessoa> Pessoas { get; protected set; }

            public Endereco()
            {
                Pessoas = new List<Pessoa>();
            }
        }
    }
}
