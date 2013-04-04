using System.Collections.Generic;

namespace NHibernateTesting.Tests.Relacoes.Unidirecionais
{
    public class Autor
    {
        public virtual int Id { get; protected set; }
        public virtual string Nome { get; set; }

        public virtual IList<Livro> Livros { get; protected set; }

        public Autor()
        {
            Livros = new List<Livro>();
        }
    }

    public class Livro
    {
        public virtual int Id { get; protected set; }
        public virtual string Titulo { get; set; }
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
    }
}
