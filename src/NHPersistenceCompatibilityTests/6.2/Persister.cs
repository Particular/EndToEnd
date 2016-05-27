using System;
using NServiceBus.Saga;
using NServiceBus.SagaPersisters.NHibernate;
using Version_6_2;

public class Persister
{
    public void Save<T>(T data, string correlationPropertyName = null, string correlationPropertyValue = null)
        where T : IContainSagaData
    {
        using (var sessionFactory = NHibernateSessionFactory.Create())
        using (var session = sessionFactory.OpenSession())
        {
            var persister = new SagaPersister(new TestSessionProvider(session));

            persister.Save(data);

            session.Flush();
        }
    }

    public void Update<T>(T data) where T : IContainSagaData
    {
        using (var sessionFactory = NHibernateSessionFactory.Create())
        using (var session = sessionFactory.OpenSession())
        {
            var persister = new SagaPersister(new TestSessionProvider(session));

            persister.Update(data);

            session.Flush();
        }
    }

    public T Get<T>(Guid id) where T : IContainSagaData
    {
        using (var sessionFactory = NHibernateSessionFactory.Create())
        using (var session = sessionFactory.OpenSession())
        {
            var persister = new SagaPersister(new TestSessionProvider(session));

            var data = persister.Get<T>(id);

            //Make sure all lazy properties are fetched before returning result
            ObjectFetcher.Traverse(data, typeof (T));

            return data;
        }
    }

    public T GetByCorrelationProperty<T>(string correlationPropertyName, object correlationPropertyValue) where T : IContainSagaData
    {
        using (var sessionFactory = NHibernateSessionFactory.Create())
        using (var session = sessionFactory.OpenSession())
        {
            var persister = (ISagaPersister)new SagaPersister(new TestSessionProvider(session));

            var data = persister.Get<T>(correlationPropertyName, correlationPropertyValue);

            //Make sure all lazy properties are fetched before returning result
            ObjectFetcher.Traverse(data, typeof(T));

            return data;
        }
    }
}