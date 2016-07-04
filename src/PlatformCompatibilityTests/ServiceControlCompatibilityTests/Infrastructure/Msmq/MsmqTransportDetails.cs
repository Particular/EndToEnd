using System.Configuration;
using NServiceBus;

namespace ServiceControlCompatibilityTests
{
    // Todo: Potentially move this out to a separate project
    // so we don't take a hard dependency on NServiceBus.SqlServer
    class MsmqTransportDetails : ITransportDetails
    {
        const string TransportTypeName = "NServiceBus.MsmqTransport, NServiceBus.Core";

        public string TransportName => "Msmq";

        public void Initialize() { }

        public void ApplyTo(Configuration configuration)
        {
            var settings = configuration.AppSettings.Settings;
            settings.Set(SettingsList.TransportType, TransportTypeName);
        }

        public void ConfigureEndpoint(string endpointName, EndpointConfiguration endpointConfig)
        {
            endpointConfig.UseTransport<MsmqTransport>();

            endpointConfig.PurgeOnStartup(true);
        }
    }
}