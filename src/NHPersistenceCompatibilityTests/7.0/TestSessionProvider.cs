using NHibernate;
using NServiceBus;
using NServiceBus.Persistence;

public class TestSessionProvider : SynchronizedStorageSession, INHibernateSynchronizedStorageSession
{
    public TestSessionProvider(ISession session)
    {
        Session = session;
    }

    public ISession Session { get; }
}