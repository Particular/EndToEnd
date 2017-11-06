using System.Reflection;
using NServiceBus;
using NServiceBus.Persistence;
using Tests.Permutations;

class NHibernateProfile : IProfile, ISetup, INeedPermutation
{
    public Permutation Permutation { private get; set; }

    public void Configure(EndpointConfiguration cfg)
    {
        cfg
            .UsePersistence<NHibernatePersistence>()
            .ConnectionString(ConfigurationHelper.GetConnectionString(Permutation.Persister.ToString()));
    }

    void ISetup.Setup()
    {
        var cs = ConfigurationHelper.GetConnectionString(Permutation.Persister.ToString());
        var sql = Assembly.GetExecutingAssembly().GetManifestResourceText("Persistence.V6.NHibernate.init.sql");
        SqlHelper.CreateDatabase(cs);
        SqlHelper.ExecuteScript(cs, sql);
    }
}
