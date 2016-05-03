using System.Reflection;
using NServiceBus;
using NServiceBus.Persistence;

class NHibernateProfile : IProfile, ISetup
{
    public void Configure(BusConfiguration cfg)
    {
        cfg
            .UsePersistence<NHibernatePersistence>()
            .ConnectionString(ConfigurationHelper.GetConnectionString("NHibernate"));
    }

    void ISetup.Setup()
    {
        var cs = ConfigurationHelper.GetConnectionString("NHibernate");
        var sql = Assembly.GetExecutingAssembly().GetManifestResourceText("Persistence.V5.NHibernate.init.sql");
        SqlHelper.CreateDatabase(cs);
        SqlHelper.ExecuteScript(cs, sql);
    }
}
