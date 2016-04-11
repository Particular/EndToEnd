using NServiceBus;

public interface IProfile
{
    void Configure(EndpointConfiguration busConfiguration);
}
