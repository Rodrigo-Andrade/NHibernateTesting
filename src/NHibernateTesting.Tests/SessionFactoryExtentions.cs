using System;
using NHibernate;

namespace NHibernateTesting.Tests
{
    public static class SessionFactoryExtentions
    {
        public static void WithNew(this ISessionFactory sessionFactory, Action<ISession> action)
        {
            if (sessionFactory == null) throw new ArgumentNullException("sessionFactory");
            if (action == null) throw new ArgumentNullException("action");

            using (var session = sessionFactory.OpenSession())
            using (var tx = session.BeginTransaction())
            {
                action(session);
                tx.Commit();
            }
        }

        public static T WithNew<T>(this ISessionFactory sessionFactory, Func<ISession, T> func)
        {
            if (sessionFactory == null) throw new ArgumentNullException("sessionFactory");
            if (func == null) throw new ArgumentNullException("func");
            var result = default(T);
            WithNew(sessionFactory, s => { result = func(s); });
            return result;
        }
    }
}