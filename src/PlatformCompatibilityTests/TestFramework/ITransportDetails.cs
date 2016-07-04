namespace ServiceControlCompatibilityTests
{
    using NServiceBus;
    using System.Configuration;

    public interface ITransportDetails
    {
        string TransportName { get; }
        void ApplyTo(Configuration configuration);
        void ConfigureEndpoint(string endpointName, EndpointConfiguration endpointConfig);
        void Initialize();
    }
}