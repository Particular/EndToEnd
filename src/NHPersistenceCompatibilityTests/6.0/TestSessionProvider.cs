namespace Version_6_0
{
    using System;
    using NHibernate;
    using NServiceBus.Persistence.NHibernate;

    public class TestSessionProvider : IStorageSessionProvider, IDisposable
    {
        public TestSessionProvider(ISessionFactory sessionFactory)
        {
            this.sessionFactory = sessionFactory;
        }

        public void Dispose()
        {
            session?.Flush();
            session?.Dispose();
        }

        public IStatelessSession OpenStatelessSession()
        {
            return sessionFactory.OpenStatelessSession();
        }

        public ISession OpenSession()
        {
            return sessionFactory.OpenSession();
        }

        public ISession Session
        {
            get
            {
                if (session == null)
                {
                    session = OpenSession();
                }

                return session;
            }
        }

        readonly ISessionFactory sessionFactory;
        ISession session;
    }
}