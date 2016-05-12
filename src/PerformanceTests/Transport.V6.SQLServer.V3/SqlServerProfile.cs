using System.Reflection;
using NServiceBus;
using NServiceBus.Transport.SQLServer;
using Tests.Permutations;

class SqlServerProfile : IProfile, ISetup, INeedPermutation
{
    public Permutation Permutation { private get; set; }

    public void Configure(EndpointConfiguration endpointConfiguration)
    {
        endpointConfiguration
            .UseTransport<SqlServerTransport>()
            .DefaultSchema("V6")
            .ConnectionString(ConfigurationHelper.GetConnectionString(Permutation.Transport.ToString()));
    }

    void ISetup.Setup()
    {
        var cs = ConfigurationHelper.GetConnectionString(Permutation.Transport.ToString());
        var sql = Assembly.GetExecutingAssembly().GetManifestResourceText("Transport.V6.SQLServer.init.sql");
        SqlHelper.CreateDatabase(cs);
        SqlHelper.ExecuteScript(cs, sql);
    }
}
