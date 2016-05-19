using System;
using NServiceBus.Saga;
using NServiceBus.SagaPersisters.NHibernate;
using NServiceBus.UnitOfWork;
using NServiceBus.UnitOfWork.NHibernate;
using Version_5_0;

public class Persister
{
    readonly NHibernateSessionFactory factory;

    public Persister()
    {
        factory = new NHibernateSessionFactory();
    }

    public void Save<T>(T data, string correlationPropertyName = null, string correlationPropertyValue = null) where T : IContainSagaData
    {
        var unitOfWorkManager = new UnitOfWorkManager { SessionFactory = factory.SessionFactory.Value };
        var persister = new SagaPersister { UnitOfWorkManager = unitOfWorkManager };

        ((IManageUnitsOfWork)unitOfWorkManager).Begin();

        persister.Save(data);

        ((IManageUnitsOfWork)unitOfWorkManager).End();
    }

    public T Get<T>(Guid id) where T : IContainSagaData
    {
        var unitOfWorkManager = new UnitOfWorkManager { SessionFactory = factory.SessionFactory.Value };
        var persister = new SagaPersister { UnitOfWorkManager = unitOfWorkManager };

        var data = persister.Get<T>(id);

        return data;
    }
}