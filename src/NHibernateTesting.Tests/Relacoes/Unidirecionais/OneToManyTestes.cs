﻿using FluentAssertions;
using FluentNHibernate.Mapping;
using NUnit.Framework;

namespace NHibernateTesting.Tests.Relacoes.Unidirecionais
{
    public class OneToManyTestes : TestCase
    {
        public class AutorMap : ClassMap<Autor>
        {
            public AutorMap()
            {
                Id(x => x.Id);
                Map(x => x.Nome);
                HasMany(x => x.Livros);
            }
        }

        public class LivroMap : ClassMap<Livro>
        {
            public LivroMap()
            {
                Id(x => x.Id);
                Map(x => x.Titulo);
            }
        }

        [Test]
        public void DevePersistirCorretamente()
        {
            var pesistido = new Autor
            {
                Nome = "Autor",
                Livros =
                {
                    new Livro {Titulo = "Livro 01"},
                    new Livro {Titulo = "Livro 02"}
                }
            };

            WithNew(session =>
                        {
                            foreach (var livro in pesistido.Livros)
                                session.Save(livro);
                            session.Save(pesistido);
                        });

            WithNew(session =>
                        {
                            var recuperado = session.Get<Autor>(1);

                            recuperado
                                .ShouldHave()
                                .AllProperties()
                                .IncludingNestedObjects()
                                .EqualTo(pesistido);
                        });
        }
    }
}
