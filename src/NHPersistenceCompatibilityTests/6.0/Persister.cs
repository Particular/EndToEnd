using System;
using System.Collections.Generic;
using NServiceBus.Saga;
using NServiceBus.SagaPersisters.NHibernate;
using Version_6_0;

public class Persister
{
    public void Save<T>(T data, string correlationPropertyName = null, string correlationPropertyValue = null)
        where T : IContainSagaData
    {
        using (var sessionFactory = NHibernateSessionFactory.Create())
        using (var sessionProvider = new TestSessionProvider(sessionFactory))
        {
            var persister = new SagaPersister(sessionProvider);

            persister.Save(data);
        }
    }

    public T Get<T>(Guid id) where T : IContainSagaData
    {
        using (var sessionFactory = NHibernateSessionFactory.Create())
        using (var sessionProvider = new TestSessionProvider(sessionFactory))
        {
            var persister = new SagaPersister(sessionProvider);

            var data = persister.Get<T>(id);

            //Make sure all lazy properties are fetched before returning result
            Fetcher.Traverse(data, typeof (T));

            return data;
        }
    }

    public void Update<T>(T data) where T : IContainSagaData
    {
        using (var sessionFactory = NHibernateSessionFactory.Create())
        using (var sessionProvider = new TestSessionProvider(sessionFactory))
        {
            var persister = new SagaPersister(sessionProvider);

            persister.Update(data);
        }
    }

    public T GetByCorrelationProperty<T>(string correlationPropertyName, object correlationPropertyValue) where T : IContainSagaData
    {
        using (var sessionFactory = NHibernateSessionFactory.Create())
        using (var sessionProvider = new TestSessionProvider(sessionFactory))
        {
            var persister = (ISagaPersister)new SagaPersister(sessionProvider);

            var data = persister.Get<T>(correlationPropertyName, correlationPropertyValue);

            //Make sure all lazy properties are fetched before returning result
            ObjectFetcher.Traverse(data, typeof(T));

            return data;
        }
    }

    static class Fetcher
    {
        public static void Traverse(object instance, Type instanceType)
        {
            foreach (var propertyInfo in instanceType.GetProperties())
            {
                if (ReferenceEquals(propertyInfo.DeclaringType, typeof (object)))
                {
                    continue;
                }

                if (propertyInfo.PropertyType.GetInterface(typeof (IEnumerable<>).FullName) != null)
                {
                    propertyInfo.GetValue(instance, null)?.ToString();
                }
                else
                {
                    var propertyValue = propertyInfo.GetValue(instance, null);

                    if (propertyValue != null)
                    {
                        Traverse(propertyValue, propertyInfo.PropertyType);
                    }
                }
            }
        }
    }
}