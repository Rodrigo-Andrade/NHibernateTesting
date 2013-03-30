using System.Linq;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using FluentNHibernate.Mapping;
using HibernatingRhinos.Profiler.Appender.NHibernate;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;
using NUnit.Framework;
using System;

namespace NHibernateTesting.Tests
{
    public abstract class TestCase
    {
        public ISessionFactory SessionFactory { get; private set; }

        public virtual bool ShowSchemaOnConsole
        {
            get { return true; }
        }

        public virtual bool ShouldExportSchema
        {
            get { return true; }
        }

        public static bool LogSqlInConsole
        {
            get { return true; }
        }

        private SchemaExport _schemaExport;

        [SetUp]
        public void SetUp()
        {
            var sqlLite = SQLiteConfiguration
                .Standard
                .UsingFile("NHibernate");

            //var msSql = MsSqlConfiguration
            //    .MsSql2008
            //    .ConnectionString(x => x.FromConnectionStringWithKey("NHibernateMsSql"));

            SessionFactory =
                Fluently.Configure()
                        .Database(sqlLite)
                        .Mappings(Mappings)
                        .ExposeConfiguration(Config)
                        .BuildSessionFactory();
        }

        [TestFixtureSetUp]
        public void TextFixtureSetUp()
        {
            NHibernateProfiler.Initialize();
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
                    c.LogSqlInConsole = LogSqlInConsole;
                });

            configuration.SessionFactory().GenerateStatistics();

            SetCustomTestConfiguration(configuration);
            ExportSchema(configuration);
        }

        protected virtual void SetCustomTestConfiguration(Configuration configuration) { }

        private void ExportSchema(Configuration configuration)
        {
            _schemaExport = new SchemaExport(configuration);
            _schemaExport.Create(ShowSchemaOnConsole, ShouldExportSchema);
        }

        [TearDown]
        public void TearDown()
        {
            _schemaExport.Drop(ShowSchemaOnConsole, ShouldExportSchema);

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
