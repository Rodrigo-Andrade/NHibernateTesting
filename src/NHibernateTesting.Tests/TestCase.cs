using System.Linq;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using FluentNHibernate.Mapping;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;
using NUnit.Framework;
using System;

namespace NHibernateTesting.Tests
{
    public abstract class TestCase
    {
        private const bool SHOW_SCHEMA_ON_CONSOLE = true;
        private const bool EXPORT_SCHEMA = true;
        private const bool LOG_SQL_IN_CONSOLE = true;

        public ISessionFactory SessionFactory { get; private set; }

        private SchemaExport _schemaExport;

        [SetUp]
        public void SetUp()
        {
            var db = SQLiteConfiguration.Standard
                                        .UsingFile("NHibernate");

            SessionFactory =
                Fluently.Configure()
                        .Database(db)
                        .Mappings(Mappings)
                        .ExposeConfiguration(Config)
                        .BuildSessionFactory();
        }

        public virtual void Mappings(MappingConfiguration mappingConfig)
        {
            var mappings = from type in GetType().GetNestedTypes()
                           where type.BaseType != null
                           let baseType = type.BaseType
                           where baseType.IsGenericType
                           where baseType.GetGenericTypeDefinition().IsAssignableFrom(typeof(ClassMap<>))
                           select type;

            foreach (var mapping in mappings)
                mappingConfig.FluentMappings.Add(mapping);
        }

        private void Config(Configuration configuration)
        {
            configuration.DataBaseIntegration(c =>
                {
                    c.LogFormattedSql = true;
                    c.LogSqlInConsole = LOG_SQL_IN_CONSOLE;
                });

            configuration.SessionFactory().GenerateStatistics();

            SetCustomTestConfiguration(configuration);
            ExportSchema(configuration);
        }

        protected virtual void SetCustomTestConfiguration(Configuration configuration) { }

        private void ExportSchema(Configuration configuration)
        {
            _schemaExport = new SchemaExport(configuration);
            _schemaExport.Create(SHOW_SCHEMA_ON_CONSOLE, EXPORT_SCHEMA);
        }

        [TearDown]
        public void TearDown()
        {
            _schemaExport.Drop(SHOW_SCHEMA_ON_CONSOLE, EXPORT_SCHEMA);

            using (SessionFactory)
            {
            }
        }

        public void WithNew(Action<ISession> action)
        {
            SessionFactory.WithNew(action);
        }

        public T WithNew<T>(Func<ISession, T> func)
        {
            return SessionFactory.WithNew(func);
        }
    }
}
