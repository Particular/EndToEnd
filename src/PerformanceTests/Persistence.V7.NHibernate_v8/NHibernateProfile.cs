using System.Reflection;
using NHibernate;
using NHibernate.Cfg;
using NServiceBus;
using NServiceBus.Persistence;
using Tests.Permutations;

class NHibernateProfile : IProfile, ISetup, INeedPermutation
{
    public Permutation Permutation { private get; set; }

    public void Configure(EndpointConfiguration cfg)
    {
        var nhCfg = new Configuration();

        nhCfg.SetProperty(Environment.UseSecondLevelCache, bool.TrueString);
        nhCfg.SetProperty(Environment.UseQueryCache, bool.TrueString);
        nhCfg.SetProperty(Environment.CacheProvider, typeof(NHibernate.Caches.SysCache.SysCacheProvider).FullName);
        nhCfg.EntityCache<SagaUpdateRunner.SagaUpdateData>(ce =>
        {
            ce.Strategy = EntityCacheUsage.NonStrictReadWrite;
            ce.RegionName = "MyRegion";
        });
        nhCfg.EntityCache<SagaCongestionRunner.SagaCongestionData>(ce =>
        {
            ce.Strategy = EntityCacheUsage.NonStrictReadWrite;
            ce.RegionName = "MyRegion";
        });
        nhCfg.EntityCache<SagaInitiateRunner.SagaCreateData>(ce =>
        {
            ce.Strategy = EntityCacheUsage.NonStrictReadWrite;
            ce.RegionName = "MyRegion";
        });

        cfg
            .UsePersistence<NHibernatePersistence>()
            .UseConfiguration(nhCfg)
            .ConnectionString(ConfigurationHelper.GetConnectionString(Permutation.Persister.ToString()));
    }

    void ISetup.Setup()
    {
        var cs = ConfigurationHelper.GetConnectionString(Permutation.Persister.ToString());
        var sql = Assembly.GetExecutingAssembly().GetManifestResourceText("Persistence.V7.NHibernate.init.sql");
        SqlHelper.CreateDatabase(cs);
        SqlHelper.ExecuteScript(cs, sql);
    }
}
