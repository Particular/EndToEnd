using System.Reflection;
using NServiceBus;

class SqlServerProfile : IProfile, ISetup
{
    public void Configure(BusConfiguration busConfiguration)
    {
        busConfiguration
            .UseTransport<SqlServerTransport>()
            .DefaultSchema("V5")
            .ConnectionString(ConfigurationHelper.GetConnectionString("SqlServer"));
    }

    void ISetup.Setup()
    {
        var cs = ConfigurationHelper.GetConnectionString("SqlServer");
        var sql = Assembly.GetExecutingAssembly().GetManifestResourceText("Transport.V5.SQLServer.init.sql");
        SqlHelper.CreateDatabase(cs);
        SqlHelper.ExecuteScript(cs, sql);
    }
}
