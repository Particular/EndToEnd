using System;
using NServiceBus;
using NServiceBus.Extensibility;
using NServiceBus.SagaPersisters.NHibernate;
using NServiceBus.Sagas;
using Version_7_0;

public class Persister
{
    readonly SagaPersister persister;
    readonly NHibernateSessionFactory factory;

    public Persister()
    {
        factory = new NHibernateSessionFactory();
        persister = new SagaPersister();
    }

    public void Save<T>(T data, string correlationPropertyName, string correlationPropertyValue)
        where T : IContainSagaData
    {
        var correlationProperty = new SagaCorrelationProperty(correlationPropertyName, correlationPropertyValue);

        using (var session = factory.SessionFactory.Value.OpenSession())
        {
            persister.Save(data, correlationProperty, new TestSessionProvider(session), new ContextBag())
                .GetAwaiter()
                .GetResult();

            session.Flush();
        }
    }

    public T Get<T>(Guid id) where T : IContainSagaData
    {
        var session = factory.SessionFactory.Value.OpenSession();

        var data = persister.Get<T>(id, new TestSessionProvider(session), new ContextBag()).GetAwaiter().GetResult();

        return data;
    }
}