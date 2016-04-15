using NServiceBus.Config;

public interface IConfigureUnicastBus
{
    MessageEndpointMappingCollection GenerateMappings();
}