using FluentAssertions;
using FluentAssertions.Equivalency;
using FluentNHibernate.Mapping;
using NUnit.Framework;
using System.Collections.Generic;

namespace NHibernateTesting.Tests.Relacoes.Bidirecionais.ComTabelaDeLigacao
{
    public class OneToManyComManyToOneTestes : TestCase
    {
        public class AutorMap : ClassMap<Autor>
        {
            public AutorMap()
            {
                Id(x => x.Id);
                Map(x => x.Nome);
                HasManyToMany(x => x.LivrosPublicados)
                    .Table("AutorPessoa")
                    .ChildKeyColumns.Add("Livro_Id", x => x.Unique());
            }
        }

        public class LivroMap : ClassMap<Livro>
        {
            public LivroMap()
            {
                Id(x => x.Id);
                Map(x => x.Titulo);
                Join("AutorPessoa",
                     m =>
                     {
                         m.Inverse();
                         m.Optional();
                         m.KeyColumn("Livro_id");
                         m.References(x => x.Autor).Not.Nullable();
                     });
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
                    session.Save(livro);
                    autor.LivrosPublicados.Add(livro);
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
