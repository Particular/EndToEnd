﻿namespace Version_6_1_0
{
    using Common;
    using DataDefinitions;
    using NHibernate;
    using NHibernate.Tool.hbm2ddl;
    using NServiceBus.SagaPersisters.NHibernate.AutoPersistence;

    public class NHibernateSessionFactory
    {
        static NHibernateSessionFactory()
        {
            SessionFactory = Init();
        }

        static ISessionFactory Init()
        {
            var configuration = new NHibernate.Cfg.Configuration().AddProperties(NHibernateConnectionInfo.Settings);
            var modelMapper = new SagaModelMapper(new[]
            {
                typeof(TestSagaDataWithList),
                typeof(TestSagaDataWithComposite),
                typeof(TestSagaData)
            });

            configuration.AddMapping(modelMapper.Compile());

            new SchemaUpdate(configuration).Execute(false, true);

            return configuration.BuildSessionFactory();
        }

        public static ISessionFactory SessionFactory { get; }
    }
}