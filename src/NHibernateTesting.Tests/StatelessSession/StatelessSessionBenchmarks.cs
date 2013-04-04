using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FluentNHibernate.Mapping;
using NUnit.Framework;

namespace NHibernateTesting.Tests.StatelessSession
{
    public class StatelessSessionBenchmarks : TestCase
    {
        public override bool ShowSchemaOnConsole
        {
            get { return false; }
        }

        public override bool LogSqlInConsole
        {
            get { return false; }
        }

        private List<SimpleClass> _valores;

        public class SimpleClass
        {
            public virtual Guid Id { get; set; }
            public virtual string ValorString { get; set; }
            public virtual int ValorInteiro { get; set; }
            public virtual DateTime ValorDatetime { get; set; }
        }

        public class SimpleClassMap : ClassMap<SimpleClass>
        {
            public SimpleClassMap()
            {
                Id(x => x.Id);
                Map(x => x.ValorString);
                Map(x => x.ValorInteiro);
                Map(x => x.ValorDatetime);
            }
        }

        [SetUp]
        public void SetUpNew()
        {
            var random = new Random();
            _valores = (from i in Enumerable.Range(1, 10000)
                        select new SimpleClass
                                   {
                                       ValorInteiro = random.Next(1, 100),
                                       ValorString = Guid.NewGuid().ToString(),
                                       ValorDatetime = DateTime.Now
                                   }).ToList();
        }

        [Test]
        [Ignore]
        public void StatelessSessionBenchmark()
        {
            var sw = Stopwatch.StartNew();
            using (var session = SessionFactory.OpenStatelessSession())
            using (var tx = session.BeginTransaction())
            {
                foreach (var valor in _valores)
                    session.Insert(valor);

                tx.Commit();
            }

            sw.Stop();
            Console.WriteLine("TEMPO DECORRIDO {0}:{1}", sw.Elapsed.Seconds, sw.Elapsed.Milliseconds);
        }

        [Test]
        [Ignore]
        public void SessionBenchmark()
        {
            var sw = Stopwatch.StartNew();
            WithNew(session =>
                        {
                            foreach (var valor in _valores)
                                session.Save(valor);
                        });
            sw.Stop();
            Console.WriteLine("TEMPO DECORRIDO {0}:{1}", sw.Elapsed.Seconds, sw.Elapsed.Milliseconds);
        }
    }
}
