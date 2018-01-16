using System;
using NServiceBus;
using NServiceBus.Extensibility;
using NServiceBus.SagaPersisters.NHibernate;
using NServiceBus.Sagas;
using Version_8_0;

public class Persister
{
    public Persister()
    {
        persister = new SagaPersister();
    }

    public void Save<T>(T data, string correlationPropertyName, string correlationPropertyValue)
        where T : IContainSagaData
    {
        var correlationProperty = new SagaCorrelationProperty(correlationPropertyName, correlationPropertyValue);

        using (var sessionFactory = NHibernateSessionFactory.Create())
        {
            using (var session = sessionFactory.OpenSession())
            {
                persister.Save(data, correlationProperty, new TestSessionProvider(session), new ContextBag())
                    .GetAwaiter()
                    .GetResult();

                session.Flush();
            }
        }
    }

    public void Update<T>(T data) where T : IContainSagaData
    {
        using (var sessionFactory = NHibernateSessionFactory.Create())
        {
            using (var session = sessionFactory.OpenSession())
            {
                persister.Update(data, new TestSessionProvider(session), new ContextBag())
                    .GetAwaiter()
                    .GetResult();

                session.Flush();
            }
        }
    }

    public void Complete<T>(T data) where T : IContainSagaData
    {
        using (var sessionFactory = NHibernateSessionFactory.Create())
        using (var session = sessionFactory.OpenSession())
        {
            persister.Complete(data, new TestSessionProvider(session), new ContextBag()).GetAwaiter().GetResult();

            session.Flush();
        }
    }

    public T Get<T>(Guid id) where T : class, IContainSagaData
    {
        using (var sessionFactory = NHibernateSessionFactory.Create())
        {
            using (var session = sessionFactory.OpenSession())
            {
                var data = persister.Get<T>(id, new TestSessionProvider(session), new ContextBag()).GetAwaiter().GetResult();

                //Make sure all lazy properties are fetched before returning result
                ObjectFetcher.Traverse(data, typeof(T));

                return data;
            }
        }
    }

    public T GetByCorrelationProperty<T>(string correlationPropertyName, object correlationPropertyValue) where T : class, IContainSagaData
    {
        using (var sessionFactory = NHibernateSessionFactory.Create())
        {
            using (var session = sessionFactory.OpenSession())
            {
                var data = persister.Get<T>(correlationPropertyName, correlationPropertyValue, new TestSessionProvider(session), new ContextBag()).GetAwaiter().GetResult();

                //Make sure all lazy properties are fetched before returning result
                ObjectFetcher.Traverse(data, typeof(T));

                return data;
            }
        }
    }

    ISagaPersister persister;
}