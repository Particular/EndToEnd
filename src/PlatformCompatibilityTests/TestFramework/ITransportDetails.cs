namespace ServiceControlCompatibilityTests
{
    using NServiceBus;
    using System.Configuration;
    using System.Threading.Tasks;

    public interface ITransportDetails
    {
        string TransportName { get; }
        void ApplyTo(Configuration configuration);
        void ConfigureEndpoint(string endpointName, EndpointConfiguration endpointConfig);
        Task Initialize();
    }
}