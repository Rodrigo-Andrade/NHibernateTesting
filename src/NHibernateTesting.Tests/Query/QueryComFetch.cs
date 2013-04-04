using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using FluentNHibernate.Mapping;
using NHibernate.Linq;
using NUnit.Framework;

namespace NHibernateTesting.Tests.Query
{
    public class QueryComFetch : TestCase
    {
        [Test]
        public void FiltrarPorColecaoDeveTrazerColecaoInteira()
        {
            WithNew(session =>
                        {
                            var autores = new[]
                                              {
                                                  new Autor {Nome = "Joao"},
                                                  new Autor {Nome = "Maria"},
                                                  new Autor {Nome = "Pedro"}
                                              };

                            foreach (var autor in autores)
                                session.Save(autor);

                            var livros = new[]
                                             {
                                                 new Livro
                                                     {
                                                         Autores =
                                                             {
                                                                 autores[0],
                                                                 autores[1]
                                                             }
                                                     },
                                                 new Livro
                                                     {
                                                         Autores =
                                                             {
                                                                 autores[1],
                                                                 autores[2]
                                                             }
                                                     }
                                             };

                            foreach (var livro in livros)
                                session.Save(livro);
                        });

            WithNew(session =>
                        {
                            var livros = session.Query<Livro>()
                                                .Where(x => x.Autores.Any(a => a.Nome == "Maria"))
                                                .FetchMany(x => x.Autores)
                                                .ToList();

                            livros.Count.Should().Be(2);
                            livros.Count(x => x.Autores.Count == 2).Should().Be(2);
                        });
        }

        public class LivroMap : ClassMap<Livro>
        {
            public LivroMap()
            {
                Id(x => x.Id);
                HasManyToMany(x => x.Autores);
            }
        }

        public class AutorMap : ClassMap<Autor>
        {
            public AutorMap()
            {
                Id(x => x.Id);
                Map(x => x.Nome);
            }
        }

        public class Livro
        {
            public virtual int Id { get; set; }
            public virtual ICollection<Autor> Autores { get; set; }

            public Livro()
            {
                Autores = new List<Autor>();
            }
        }

        public class Autor
        {
            public virtual int Id { get; set; }
            public virtual string Nome { get; set; }
        }
    }

   
}
