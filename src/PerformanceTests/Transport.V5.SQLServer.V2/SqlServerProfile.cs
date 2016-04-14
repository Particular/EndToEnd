using NServiceBus;
using Common;

class SqlServerProfile : IProfile
{
    public void Configure(BusConfiguration busConfiguration)
    {
        busConfiguration
            .UseTransport<SqlServerTransport>()
            .ConnectionString(this.GetConnectionString("SqlServer"));
    }
}
