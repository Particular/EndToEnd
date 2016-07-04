using System.Configuration;
using NServiceBus;

namespace ServiceControlCompatibilityTests
{
    using System.Threading.Tasks;

    public class RabbitMQTransportDetails : ITransportDetails
    {
        const string TransportTypeName = "NServiceBus.RabbitMQTransport, NServiceBus.Transports.RabbitMQ";

        public RabbitMQTransportDetails(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public string TransportName => "RabbitMQ";

        public Task Initialize()
        {
            return Task.FromResult(0);
        }

        public void ApplyTo(Configuration configuration)
        {
            configuration.ConnectionStrings.ConnectionStrings.Set("NServiceBus/Transport", connectionString);
            var settings = configuration.AppSettings.Settings;
            settings.Set(SettingsList.TransportType, TransportTypeName);
        }

        public void ConfigureEndpoint(string endpointName, EndpointConfiguration endpointConfig)
        {
            endpointConfig.UseTransport<RabbitMQTransport>()
                .ConnectionString(connectionString);

            endpointConfig.PurgeOnStartup(true);
        }

        string connectionString;
    }
}