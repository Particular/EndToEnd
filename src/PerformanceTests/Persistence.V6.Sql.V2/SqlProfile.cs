using System;
using System.Data.SqlClient;
using NServiceBus;
using NServiceBus.Persistence.Sql;

class SqlProfile : IProfile
{
    public void Configure(EndpointConfiguration configuration)
    {
        var connectionString = ConfigurationHelper.GetConnectionString("Sql");
        var persistence = configuration.UsePersistence<SqlPersistence>();
        persistence.SqlVariant(SqlVariant.MsSqlServer);
        persistence.Schema("V6");
        persistence.ConnectionBuilder(() => new SqlConnection(connectionString));

        var subscriptions = persistence.SubscriptionSettings();
        subscriptions.CacheFor(TimeSpan.FromSeconds(1));
    }
}
