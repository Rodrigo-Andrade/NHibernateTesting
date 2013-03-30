using FluentAssertions;
using FluentAssertions.Equivalency;
using FluentNHibernate.Mapping;
using NUnit.Framework;
using System.Collections.Generic;

namespace NHibernateTesting.Tests.Relacoes.Bidirecionais
{
    public class OneToManyComManyToOneTestes : TestCase
    {
        public class AutorMap : ClassMap<Autor>
        {
            public AutorMap()
            {
                Id(x => x.Id);
                Map(x => x.Nome);
                HasMany(x => x.LivrosPublicados)
                    .Inverse();
            }
        }

        public class LivroMap : ClassMap<Livro>
        {
            public LivroMap()
            {
                Id(x => x.Id);
                Map(x => x.Titulo);
                References(x => x.Autor);
            }
        }

        [Test]
        public void DevePersistirCorretamente()
        {
            var persistido = WithNew(session =>
            {
                var autor = new Autor { Nome = "Autor" };
                session.Save(autor);

                var livros = new[]
                {
                    new Livro {Titulo = "Livro 01", Autor = autor},
                    new Livro {Titulo = "Livro 02", Autor = autor}
                };

                foreach (var livro in livros)
                {
                    autor.LivrosPublicados.Add(livro);
                    session.Save(livro);
                }

                return autor;
            });

            WithNew(session =>
            {
                var recuperado = session.Get<Autor>(persistido.Id);

                recuperado
                    .ShouldHave()
                    .AllProperties()
                    .IncludingNestedObjects(CyclicReferenceHandling.Ignore)
                    .EqualTo(persistido);
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

            public virtual Autor Autor { get; set; }
        }
    }
}
