using System;
using System.Collections.Generic;
using NServiceBus.Saga;
using NServiceBus.SagaPersisters.NHibernate;
using Version_6_2;

public class Persister
{
    readonly NHibernateSessionFactory factory;

    public Persister()
    {
        factory = new NHibernateSessionFactory();
    }

    public void Save<T>(T data, string correlationPropertyName = null, string correlationPropertyValue = null)
        where T : IContainSagaData
    {
        using (var session = factory.SessionFactory.Value.OpenSession())
        {
            var persister = new SagaPersister(new TestSessionProvider(session));

            persister.Save(data);

            session.Flush();
        }
    }

    public T Get<T>(Guid id) where T : IContainSagaData
    {
        using (var session = factory.SessionFactory.Value.OpenSession())
        {
            var persister = new SagaPersister(new TestSessionProvider(session));

            var data = persister.Get<T>(id);

            //Make sure all lazy properties are fetched before returning result
            Fetcher.Traverse(data, typeof (T));

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