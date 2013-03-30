using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using FluentAssertions.Equivalency;
using FluentNHibernate.Mapping;
using NUnit.Framework;

namespace NHibernateTesting.Tests.Relacoes.Bidirecionais.ComTabelaDeLigacao
{
    public class ManyToManyTestes : TestCase
    {

        public class AutorMap : ClassMap<Autor>
        {
            public AutorMap()
            {
                Id(x => x.Id);
                Map(x => x.Nome);
                HasManyToMany(x => x.LivrosPublicados);
            }
        }

        public class LivroMap : ClassMap<Livro>
        {
            public LivroMap()
            {
                Id(x => x.Id);
                Map(x => x.Titulo);
                HasManyToMany(x => x.Autores).Inverse();
            }
        }

        [Test]
        public void DevePersistirCorretamente()
        {
            var livros = WithNew(session =>
                        {
                            var autor01 = new Autor { Nome = "Autor1" };
                            var autor02 = new Autor { Nome = "Autor2" };
                            var autor03 = new Autor { Nome = "Autor3" };

                            session.Save(autor01);
                            session.Save(autor02);
                            session.Save(autor03);

                            var livro01 = new Livro { Titulo = "Livro1" };
                            var livro02 = new Livro { Titulo = "Livro2" };

                            session.Save(livro01);
                            session.Save(livro02);

                            livro01.Autores.Add(autor01);
                            livro01.Autores.Add(autor02);
                            autor01.LivrosPublicados.Add(livro01);
                            autor02.LivrosPublicados.Add(livro01);

                            livro02.Autores.Add(autor02);
                            livro02.Autores.Add(autor03);
                            autor02.LivrosPublicados.Add(livro02);
                            autor03.LivrosPublicados.Add(livro02);

                            return new[] { livro01, livro02 };
                        });

            WithNew(session =>
            {
                var livro01 = session.Get<Livro>(livros.First().Id);

                livro01
                    .ShouldHave()
                    .AllProperties()
                    .IncludingNestedObjects(CyclicReferenceHandling.Ignore)
                    .EqualTo(livros.First());

                var livro02 = session.Get<Livro>(livros.Last().Id);

                livro02
                    .ShouldHave()
                    .AllProperties()
                    .IncludingNestedObjects(CyclicReferenceHandling.Ignore)
                    .EqualTo(livros.Last());

            });
        }

        public class Autor
        {
            public virtual int Id { get; protected set; }
            public virtual string Nome { get; set; }

            public virtual IList<Livro> LivrosPublicados { get; protected set; }

            public Autor()
            {
                LivrosPublicados = new List<Livro>();
            }
        }

        public class Livro
        {
            public virtual int Id { get; protected set; }
            public virtual string Titulo { get; set; }

            public virtual IList<Autor> Autores { get; protected set; }

            public Livro()
            {
                Autores = new List<Autor>();
            }
        }
    }
}
