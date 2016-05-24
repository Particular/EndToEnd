using Common;
using DataDefinitions;
using NHibernate;
using NHibernate.Tool.hbm2ddl;
using NServiceBus.SagaPersisters.NHibernate.AutoPersistence;

namespace Version_6_2
{
    public class NHibernateSessionFactory
    {
        public static ISessionFactory Create()
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
    }
}