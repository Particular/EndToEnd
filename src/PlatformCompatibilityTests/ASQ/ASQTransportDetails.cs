using System.Configuration;
using NServiceBus;

namespace ServiceControlCompatibilityTests
{
    public class ASQTransportDetails : ITransportDetails
    {
        const string TransportTypeName = "NServiceBus.AzureStorageQueueTransport, NServiceBus.Azure.Transports.WindowsAzureStorageQueues";

        public ASQTransportDetails(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public string TransportName => "AzureStorageQueue";

        public void Initialize() { }

        public void ApplyTo(Configuration configuration)
        {
            configuration.ConnectionStrings.ConnectionStrings.Set("NServiceBus/Transport", connectionString);
            var settings = configuration.AppSettings.Settings;
            settings.Set(SettingsList.TransportType, TransportTypeName);
        }

        public void ConfigureEndpoint(string endpointName, EndpointConfiguration endpointConfig)
        {
            endpointConfig.UseTransport<AzureStorageQueueTransport>()
                .ConnectionString(connectionString);

            endpointConfig.PurgeOnStartup(true);
        }

        string connectionString;
    }
}