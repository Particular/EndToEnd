using System;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using NServiceBus;
using NServiceBus.Persistence.Sql;
using Tests.Permutations;

class SqlProfile : IProfile, ISetup, INeedPermutation
{
    public Permutation Permutation { private get; set; }

    public void Configure(EndpointConfiguration configuration)
    {
        var connectionString = ConfigurationHelper.GetConnectionString("Sql");
        var persistence = configuration.UsePersistence<SqlPersistence>();
        persistence.SqlDialect<SqlDialect.MsSqlServer>().Schema("V7");
        persistence.ConnectionBuilder(() => new SqlConnection(connectionString));

        var subscriptions = persistence.SubscriptionSettings();
        subscriptions.CacheFor(TimeSpan.FromSeconds(1));
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
