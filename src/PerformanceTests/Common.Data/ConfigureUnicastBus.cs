using System.Configuration;
using NServiceBus.Config;
using NServiceBus.Config.ConfigurationSource;
using NServiceBus.Logging;

public class ConfigureUnicastBus : IProvideConfiguration<UnicastBusConfig>
{
    readonly ILog Log = LogManager.GetLogger(typeof(ConfigureUnicastBus));
    public static string EndpointName;

    public UnicastBusConfig GetConfiguration()
    {
        //read from existing config 
        var config = (UnicastBusConfig)ConfigurationManager.GetSection(typeof(UnicastBusConfig).Name);

        if (config == null)
        {
            //create new config if it doesn't exist
            config = new UnicastBusConfig
            {
                MessageEndpointMappings = GenerateMappings()
            };
        }
        return config;
    }

    MessageEndpointMappingCollection GenerateMappings()
    {
        var mappings = new MessageEndpointMappingCollection();

        var messageType = typeof(GatedPublishRunner.Event);

        Log.InfoFormat("Mapping {0} to {1}", messageType, EndpointName);

        mappings.Add(new MessageEndpointMapping
        {
            AssemblyName = messageType.Assembly.FullName,
            TypeFullName = messageType.FullName,
            Endpoint = EndpointName
        });

        return mappings;
    }

}
