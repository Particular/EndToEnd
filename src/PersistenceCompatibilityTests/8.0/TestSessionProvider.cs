using NHibernate;
using NServiceBus.Persistence;

public class TestSessionProvider : SynchronizedStorageSession
{
    public TestSessionProvider(ISession session)
    {
        Session = session;
    }

    public ISession Session { get; }
}