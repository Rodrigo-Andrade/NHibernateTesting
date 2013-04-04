using FluentAssertions;
using FluentNHibernate.Mapping;
using NHibernate;
using NHibernate.Linq;
using NHibernate.SqlTypes;
using NHibernate.UserTypes;
using NUnit.Framework;
using System;
using System.Data;
using System.Linq;

namespace NHibernateTesting.Tests.UserTypes
{
    public class EstrategiaDePrecoTestes : TestCase
    {
        [Test]
        public void RetornaQueryPorEstrategia()
        {
            WithNew(session =>
            {
                session.Save(new Pagamento
                {
                    UsuarioId = 1,
                    ValorDevidoBruto = 10,
                    EstrategiaDePreco = new Normal()
                });
                session.Save(new Pagamento
                {
                    UsuarioId = 2,
                    ValorDevidoBruto = 10,
                    EstrategiaDePreco = new ComMulta()
                });
                session.Save(new Pagamento
                {
                    UsuarioId = 3,
                    ValorDevidoBruto = 10,
                    EstrategiaDePreco = new ComDesconto()
                });
            });

            WithNew(session =>
            {
                var order = session.Query<Pagamento>()
                    .Where(x => x.EstrategiaDePreco == new ComMulta())
                    .ToList()
                    .First();

                order.UsuarioId.Should().Be(2);
                order.EstrategiaDePreco.Should().BeOfType<ComMulta>();
            });
        }

        [Test]
        public void CanPersistStrategy()
        {
            WithNew(session =>
            {
                session.Save(new Pagamento
                {
                    UsuarioId = 1,
                    ValorDevidoBruto = 10,
                    EstrategiaDePreco = new Normal()
                });
            });

            WithNew(session =>
            {
                var order = session.Get<Pagamento>(1);

                order.EstrategiaDePreco.Should().BeOfType<Normal>();
            });
        }

        public class FabricaDeEstrategiaDePreco
        {
            public static IEstrategiaDePreco GetPriceStrategy(string type)
            {
                switch (type)
                {
                    case "Desconto": return new ComDesconto();
                    case "Normal": return new Normal();
                    case "Multa": return new ComMulta();
                    default: throw new InvalidOperationException("type not valid");
                }
            }

            public static string GetValue(IEstrategiaDePreco estrategiaDePreco)
            {
                if (estrategiaDePreco is ComDesconto)
                    return "Desconto";
                if (estrategiaDePreco is Normal)
                    return "Normal";
                if (estrategiaDePreco is ComMulta)
                    return "Multa";

                throw new NotImplementedException(estrategiaDePreco + " is not implemented");
            }
        }

        public interface IEstrategiaDePreco
        {
            decimal CalcularTotal(decimal valorBruto);
        }

        public class ComDesconto : IEstrategiaDePreco
        {
            public decimal CalcularTotal(decimal valorBruto)
            {
                return valorBruto * 0.9M;
            }
        }

        public class Normal : IEstrategiaDePreco
        {
            public decimal CalcularTotal(decimal valorBruto)
            {
                return valorBruto;
            }
        }

        public class ComMulta : IEstrategiaDePreco
        {
            public decimal CalcularTotal(decimal valorBruto)
            {
                return valorBruto * 1.1M;
            }
        }

        public class Pagamento
        {
            public virtual int Id { get; set; }
            public virtual int UsuarioId { get; set; }
            public virtual IEstrategiaDePreco EstrategiaDePreco { get; set; }

            public virtual decimal ValorDevidoBruto { get; set; }

            public virtual decimal TotalAPagar
            {
                get { return EstrategiaDePreco.CalcularTotal(ValorDevidoBruto); }
            }
        }

        public class OrderMap : ClassMap<Pagamento>
        {
            public OrderMap()
            {
                Id(x => x.Id);

                Map(x => x.UsuarioId);
                Map(x => x.EstrategiaDePreco).CustomType<PriceStrategyType>();
                Map(x => x.ValorDevidoBruto);
            }

            public class PriceStrategyType : IUserType
            {
                bool IUserType.Equals(object x, object y)
                {
                    if (ReferenceEquals(x, y))
                        return true;

                    if (x == null || y == null)
                        return false;

                    return x.Equals(y);
                }

                public int GetHashCode(object x)
                {
                    return x.GetHashCode();
                }

                public object NullSafeGet(IDataReader rs, string[] names, object owner)
                {
                    var dbValue = NHibernateUtil.String.NullSafeGet(rs, names[0]);

                    if (dbValue == null) return null;
                    var unwraped = (string)dbValue;
                    return FabricaDeEstrategiaDePreco.GetPriceStrategy(unwraped);
                }

                public void NullSafeSet(IDbCommand cmd, object value, int index)
                {
                    object valueToSet = DBNull.Value;

                    if (value != null)
                        valueToSet = FabricaDeEstrategiaDePreco.GetValue((IEstrategiaDePreco)value);

                    ((IDbDataParameter)cmd.Parameters[index]).Value = valueToSet;

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
                    return cached;
                }

                public object Disassemble(object value)
                {
                    return value;
                }

                public SqlType[] SqlTypes { get { return new[] { NHibernateUtil.String.SqlType }; } }
                public Type ReturnedType { get { return typeof(IEstrategiaDePreco); } }
                public bool IsMutable { get { return false; } }
            }
        }
    }
}
