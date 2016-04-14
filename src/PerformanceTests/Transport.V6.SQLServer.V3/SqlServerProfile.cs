using NServiceBus;
using Common;

class SqlServerProfile : IProfile
{
    public void Configure(EndpointConfiguration endpointConfiguration)
    {
        endpointConfiguration
            .UseTransport<SqlServerTransport>()
            .ConnectionString(this.GetConnectionString("SqlServer"));
    }
}
