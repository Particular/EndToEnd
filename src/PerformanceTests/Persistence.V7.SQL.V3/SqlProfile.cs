using System;
using System.Data.SqlClient;
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
        var sql = ResourceHelper.GetManifestResourceTextThatEndsWith("init.sql");
        SqlHelper.CreateDatabase(cs);
        SqlHelper.ExecuteScript(cs, sql);
    }
}
