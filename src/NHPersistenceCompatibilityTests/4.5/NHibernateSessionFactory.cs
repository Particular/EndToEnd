using Common;
using DataDefinitions;
using NHibernate;
using NHibernate.Tool.hbm2ddl;
using NServiceBus.SagaPersisters.NHibernate.AutoPersistence;

namespace Version_4_5
{
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
                typeof(TestSagaData),
                typeof(TestSagaDataWithComposite),
                typeof(TestSagaDataWithList)
            });

            configuration.AddMapping(modelMapper.Compile());

            new SchemaUpdate(configuration).Execute(false, true);

            return configuration.BuildSessionFactory();
        }

        public static ISessionFactory SessionFactory { get; }
    }
}