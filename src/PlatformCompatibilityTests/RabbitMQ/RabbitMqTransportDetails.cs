using System.Configuration;
using NServiceBus;

namespace ServiceControlCompatibilityTests
{
    public class RabbitMQTransportDetails : ITransportDetails
    {
        const string TransportTypeName = "NServiceBus.RabbitMQTransport, NServiceBus.Transports.RabbitMQ";

        public RabbitMQTransportDetails(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public string TransportName => "RabbitMQ";

        public void Initialize() { }

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