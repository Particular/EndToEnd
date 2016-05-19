using System;
using NHibernate;
using NServiceBus.Persistence.NHibernate;

namespace Version_6_2
{
    public class TestSessionProvider : IStorageSessionProvider
    {
        private readonly ISession session;

        public TestSessionProvider(ISession session)
        {
            this.session = session;
        }
        
        public void ExecuteInTransaction(Action<ISession> operation)
        {
            operation(session);
        }
    }
}