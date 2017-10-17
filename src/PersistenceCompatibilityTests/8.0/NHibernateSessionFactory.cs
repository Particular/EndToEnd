using System;
using Common;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;
using NHibernate.Util;
using NServiceBus.SagaPersisters.NHibernate.AutoPersistence;
using NServiceBus.Sagas;

namespace Version_8_0
{
    public class NHibernateSessionFactory
    {
        public static ISessionFactory Create()
        {
            var configuration = new Configuration().AddProperties(NHibernateConnectionInfo.Settings);

            var types = new[]
            {
                typeof(TestSaga),
                typeof(TestSagaWithList),
                typeof(TestSagaWithComposite),
            };

            types.ForEach(t => AddMapping(configuration, t));

            new SchemaUpdate(configuration).Execute(false, true);

            return configuration.BuildSessionFactory();
        }

        static void AddMapping(Configuration configuration, Type type)
        {
            var metaModel = new SagaMetadataCollection();
            metaModel.Initialize(new[] { type });
            var metadata = metaModel.Find(type);
            var mapper = new SagaModelMapper(metaModel, new[] { metadata.SagaEntityType });

            configuration.AddMapping(mapper.Compile());
        }
    }
}