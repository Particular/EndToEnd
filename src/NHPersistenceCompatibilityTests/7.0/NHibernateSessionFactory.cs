using System;
using Common;
using NHibernate;
using NHibernate.Tool.hbm2ddl;
using NServiceBus.SagaPersisters.NHibernate.AutoPersistence;
using NServiceBus.Sagas;

namespace Version_7_0
{
    public class NHibernateSessionFactory
    {
        public NHibernateSessionFactory()
        {
            SessionFactory = new Lazy<ISessionFactory>(Init);
        }

        private ISessionFactory Init()
        {
            var configuration = new NHibernate.Cfg.Configuration().AddProperties(NHibernateConnectionInfo.Settings);

            var metaModel = new SagaMetadataCollection();
            metaModel.Initialize(new[] { typeof(TestSaga) });
            var metadata = metaModel.Find(typeof(TestSaga));
            var mapper = new SagaModelMapper(metaModel, new[] { metadata.SagaEntityType });
            configuration.AddMapping(mapper.Compile());

            metaModel = new SagaMetadataCollection();
            metaModel.Initialize(new[] { typeof(TestSagaWithList) });
            metadata = metaModel.Find(typeof(TestSagaWithList));
            mapper = new SagaModelMapper(metaModel, new[] { metadata.SagaEntityType });
            configuration.AddMapping(mapper.Compile());

            metaModel = new SagaMetadataCollection();
            metaModel.Initialize(new[] { typeof(TestSagaWithComposite) });
            metadata = metaModel.Find(typeof(TestSagaWithComposite));
            mapper = new SagaModelMapper(metaModel, new[] { metadata.SagaEntityType });
            configuration.AddMapping(mapper.Compile());

            new SchemaUpdate(configuration).Execute(false, true);

            return configuration.BuildSessionFactory();
        }

        public Lazy<ISessionFactory> SessionFactory { get; }
    }
}