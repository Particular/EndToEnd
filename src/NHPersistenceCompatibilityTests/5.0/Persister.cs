using System;
using NServiceBus.Saga;
using NServiceBus.SagaPersisters.NHibernate;
using NServiceBus.UnitOfWork;
using NServiceBus.UnitOfWork.NHibernate;
using Version_5_0;

public class Persister
{
    public void Save<T>(T data, string correlationPropertyName = null, string correlationPropertyValue = null) where T : IContainSagaData
    {
        var unitOfWorkManager = new UnitOfWorkManager { SessionFactory = NHibernateSessionFactory.SessionFactory };
        var persister = new SagaPersister { UnitOfWorkManager = unitOfWorkManager };

        ((IManageUnitsOfWork)unitOfWorkManager).Begin();

        persister.Save(data);

        ((IManageUnitsOfWork)unitOfWorkManager).End();
    }

    public void Update<T>(T data) where T : IContainSagaData
    {
        var unitOfWorkManager = new UnitOfWorkManager { SessionFactory = NHibernateSessionFactory.SessionFactory };
        var persister = new SagaPersister { UnitOfWorkManager = unitOfWorkManager };

        ((IManageUnitsOfWork)unitOfWorkManager).Begin();

        persister.Update(data);

        ((IManageUnitsOfWork)unitOfWorkManager).End();
    }

    public T Get<T>(Guid id) where T : IContainSagaData
    {
        var unitOfWorkManager = new UnitOfWorkManager { SessionFactory = NHibernateSessionFactory.SessionFactory };
        var persister = new SagaPersister { UnitOfWorkManager = unitOfWorkManager };

        ((IManageUnitsOfWork)unitOfWorkManager).Begin();

        var data = persister.Get<T>(id);

        //Make sure all lazy properties are fetched before returning result
        ObjectFetcher.Traverse(data, typeof(T));

        ((IManageUnitsOfWork)unitOfWorkManager).End();

        return data;
    }

    public T GetByCorrelationProperty<T>(string correlationPropertyName, object correlationPropertyValue) where T : IContainSagaData
    {
        var unitOfWorkManager = new UnitOfWorkManager { SessionFactory = NHibernateSessionFactory.SessionFactory };
        var persister = (ISagaPersister)new SagaPersister { UnitOfWorkManager = unitOfWorkManager };

        ((IManageUnitsOfWork)unitOfWorkManager).Begin();

        var data = persister.Get<T>(correlationPropertyName, correlationPropertyValue);

        //Make sure all lazy properties are fetched before returning result
        ObjectFetcher.Traverse(data, typeof(T));

        ((IManageUnitsOfWork)unitOfWorkManager).End();

        return data;
    }
}