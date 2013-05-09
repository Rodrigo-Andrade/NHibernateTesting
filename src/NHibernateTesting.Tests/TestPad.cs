using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using FluentAssertions;
using FluentNHibernate.Mapping;
using NHibernate;
using NHibernate.SqlTypes;
using NHibernate.UserTypes;
using NUnit.Framework;

namespace NHibernateTesting.Tests
{
    public class TestPad : TestCase
    {
        public class Foo
        {
            public virtual int Id { get; set; }
            public virtual Nome Nome { get; set; }
        }

        public class FooMap : ClassMap<Foo>
        {
            public FooMap()
            {
                Id(x => x.Id);
                Map(x => x.Nome).CustomType<StringUserType<Nome>>();
            }
        }

        [Test]
        public void Do()
        {
            var persistido = new Foo { Nome = new Nome("Rodrigo") };

            WithNew(session => session.Save(persistido));

            WithNew(session =>
                {
                    var foo = session.Get<Foo>(persistido.Id);
                    foo.Nome.Should().Be(new Nome("Rodrigo"));
                    foo.Nome.Should().NotBe(new Nome("Fulano"));
                });

        }
    }


    public class Nome : IEquatable<Nome>
    {
        private readonly string _nome;

        public Nome(string nome)
        {
            if (string.IsNullOrWhiteSpace(nome))
                throw new ArgumentException("O valor do nome não pode ser vazio ou nulo ");

            _nome = nome;
        }

        public override string ToString()
        {
            return _nome;
        }

        public bool Equals(Nome other)
        {
            if (ReferenceEquals(null, other)) return false;
            return ReferenceEquals(this, other) || string.Equals(_nome, other._nome);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((Nome)obj);
        }

        public override int GetHashCode()
        {
            return (_nome != null ? _nome.GetHashCode() : 0);
        }

        public static bool operator ==(Nome left, Nome right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Nome left, Nome right)
        {
            return !Equals(left, right);
        }
    }

    public class StringUserType<T> : IUserType
    {
        private const int STRING_LENGTH = 256;

        public object NullSafeGet(IDataReader rs, string[] names, object owner)
        {
            var value = (string)NHibernateUtil.String.NullSafeGet(rs, names[0]);
            return (T)Activator.CreateInstance(typeof(T), value);
        }

        public void NullSafeSet(IDbCommand cmd, object value, int index)
        {
            string valueToSet = null;
            var valueAsT = (T)value;

            if (value != null)
                valueToSet = valueAsT.ToString();

            NHibernateUtil.String.NullSafeSet(cmd, valueToSet, index);
        }

        public SqlType[] SqlTypes
        {
            get { return new SqlType[] { SqlTypeFactory.GetString(STRING_LENGTH) }; }
        }

        public new bool Equals(object x, object y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (x == null || y == null)
            {
                return false;
            }

            return x.Equals(y);
        }

        public int GetHashCode(object x)
        {
            return x.GetHashCode();
        }

        public object DeepCopy(object value)
        {
            return value;
        }

        public object Replace(object original, object target, object owner)
        {
            return original;
        }

        public object Assemble(object cached, object owner)
        {
            return DeepCopy(cached);
        }

        public object Disassemble(object value)
        {
            return DeepCopy(value);
        }

        public Type ReturnedType
        {
            get { return typeof(T); }
        }

        public bool IsMutable
        {
            get { return false; }
        }
    }
}
