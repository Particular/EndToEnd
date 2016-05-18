using System;
using System.Collections.Generic;
using DataDefinitions;
using NServiceBus.SagaPersisters.NHibernate;
using NServiceBus.UnitOfWork;
using NServiceBus.UnitOfWork.NHibernate;
using NUnit.Framework;
using PersistenceCompatibilityTests;
using Version_4_5;

class TestPersistence : MarshalByRefObject, ITestPersistence
{
    private readonly NHibernateSessionFactory factory;

    public TestPersistence()
    {
        factory = new NHibernateSessionFactory();
    }

    public void Persist(Guid id, string originator)
    {
        var unitOfWorkManager = new UnitOfWorkManager {SessionFactory = factory.SessionFactory.Value };
        var persister = new SagaPersister {UnitOfWorkManager = unitOfWorkManager};

        ((IManageUnitsOfWork) unitOfWorkManager).Begin();

        persister.Save(new TestSagaData
        {
            Id = id,
            OriginalMessageId = id.ToString(),
            Originator = originator
        });

        ((IManageUnitsOfWork) unitOfWorkManager).End();
    }

    public void Verify(Guid id, string originator)
    {
        var unitOfWorkManager = new UnitOfWorkManager { SessionFactory = factory.SessionFactory.Value };
        var persister = new SagaPersister { UnitOfWorkManager = unitOfWorkManager };

        var data = persister.Get<TestSagaData>(id);

        Assert.AreEqual(id, data.Id);
        Assert.AreEqual(originator, data.Originator);
    }

    public void Persist(Guid id, IList<int> data, string originator)
    {
        var unitOfWorkManager = new UnitOfWorkManager { SessionFactory = factory.SessionFactory.Value };
        var persister = new SagaPersister { UnitOfWorkManager = unitOfWorkManager };

        ((IManageUnitsOfWork)unitOfWorkManager).Begin();

        persister.Save(new TestSagaDataWithList
        {
            Id = id,
            OriginalMessageId = id.ToString(),
            Originator = originator,
            Ints = data
        });

        ((IManageUnitsOfWork)unitOfWorkManager).End();
    }

    public void Verify(Guid id, IList<int> ints, string originator)
    {
        var unitOfWorkManager = new UnitOfWorkManager { SessionFactory = factory.SessionFactory.Value };
        var persister = new SagaPersister { UnitOfWorkManager = unitOfWorkManager };

        var data = persister.Get<TestSagaDataWithList>(id);

        Assert.AreEqual(id, data.Id);
        CollectionAssert.AreEqual(originator, data.Originator);
    }

    public void Persist(Guid id, string compositeValue, string originator)
    {
        var unitOfWorkManager = new UnitOfWorkManager { SessionFactory = factory.SessionFactory.Value };
        var persister = new SagaPersister { UnitOfWorkManager = unitOfWorkManager };

        ((IManageUnitsOfWork)unitOfWorkManager).Begin();

        persister.Save(new TestSagaDataWithComposite
        {
            Id = id,
            Originator = originator,
            Composite = new TestSagaDataWithComposite.SagaComposite { Value = compositeValue }
        });

        ((IManageUnitsOfWork)unitOfWorkManager).End();
    }

    public void Verify(Guid id, string compositeValue, string originator)
    {
        var unitOfWorkManager = new UnitOfWorkManager { SessionFactory = factory.SessionFactory.Value };
        var persister = new SagaPersister { UnitOfWorkManager = unitOfWorkManager };

        var data = persister.Get<TestSagaDataWithComposite>(id);

        Assert.AreEqual(id, data.Id);
        Assert.AreEqual(originator, data.Originator);
        Assert.AreEqual(data.Composite.Value, compositeValue);
    }
}