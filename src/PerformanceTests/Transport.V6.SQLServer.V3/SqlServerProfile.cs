using System.Reflection;
using NServiceBus;
using NServiceBus.Transports.SQLServer;

class SqlServerProfile : IProfile, ISetup
{
    public void Configure(EndpointConfiguration endpointConfiguration)
    {
        endpointConfiguration
            .UseTransport<SqlServerTransport>()
            .DefaultSchema("V6")
            .ConnectionString(ConfigurationHelper.GetConnectionString("SqlServer"));
    }

    void ISetup.Setup()
    {
        var cs = ConfigurationHelper.GetConnectionString("SqlServer");
        var sql = Assembly.GetExecutingAssembly().GetManifestResourceText("Transport.V6.SQLServer.init.sql");
        SqlHelper.CreateDatabase(cs);
        SqlHelper.ExecuteScript(cs, sql);
    }
}
