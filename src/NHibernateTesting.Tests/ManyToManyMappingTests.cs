using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Mapping;

namespace NHibernateTesting.Tests
{
    public class ManyToManyMappingTests : TestCase
    {
        public void DeveRecuperarAssociacao()
        {
            WithNew(session =>
            {
                var usuario = new Usuario
                {
                    Nome = "Nome usuario",
                    Email = "email@email.com.br"
                };

                session.Save(usuario);

                var primeiroEndereco = new Endereco { Logradouro = "Logradouro1", Numero = "12345" };
                var segundoEndereco = new Endereco { Logradouro = "Logradouro2", Numero = "54321" };

                session.Save(primeiroEndereco);
                session.Save(segundoEndereco);

                usuario.Enderecos.Add(primeiroEndereco);
                usuario.Enderecos.Add(segundoEndereco);
            });
        }

        public class Usuario
        {
            public virtual int Id { get; set; }
            public virtual string Nome { get; set; }
            public virtual string Email { get; set; }

            public virtual IList<Endereco> Enderecos { get; set; }

            public Usuario()
            {
                Enderecos = new List<Endereco>();
            }
        }

        public class Endereco
        {
            public virtual int Id { get; set; }
            public virtual string Logradouro { get; set; }
            public virtual string Numero { get; set; }
        }

        public class UsuarioMap : ClassMap<Usuario>
        {
            public UsuarioMap()
            {
                Id(x => x.Id);
                Map(x => x.Nome);
                Map(x => x.Email);
            }
        }

        public class EnderecoMap : ClassMap<Endereco>
        {
            public EnderecoMap()
            {
                Id(x => x.Id);
                Map(x => x.Logradouro);
                Map(x => x.Numero);
            }
        }
    }
}
