using System.Linq;
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
        var assembly = Assembly.GetExecutingAssembly();
        var key = assembly.GetManifestResourceNames().First(x => x.EndsWith("init.sql"));
        var sql = assembly.GetManifestResourceText(key);
        SqlHelper.CreateDatabase(cs);
        SqlHelper.ExecuteScript(cs, sql);
    }
}
