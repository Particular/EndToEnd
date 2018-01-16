using System;
using System.Threading.Tasks;
using NHibernate;
using NServiceBus;
using NServiceBus.Persistence;

public class TestSessionProvider : SynchronizedStorageSession, INHibernateSynchronizedStorageSession
{
    public TestSessionProvider(ISession session)
    {
        Session = session;
    }

    public void OnSaveChanges(Func<SynchronizedStorageSession, Task> callback)
    {
        // noop
    }

    public ISession Session { get; }
}