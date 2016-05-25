using Common;
using DataDefinitions;
using NHibernate.Tool.hbm2ddl;
using NServiceBus.SagaPersisters.NHibernate.AutoPersistence;
using NHibernate;

namespace Version_4_5
{
    public class NHibernateSessionFactory
    {
        public static ISessionFactory Create()
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

            var sessionFactory = configuration.BuildSessionFactory();

            return sessionFactory;
        }
    }
}