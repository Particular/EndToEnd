using System;
using System.Configuration;
using NServiceBus.Config;
using NServiceBus.Config.ConfigurationSource;
using NServiceBus.Logging;

public interface IConfigureUnicastBus
{
    MessageEndpointMappingCollection GenerateMappings();
}

public class ConfigurationSource : IConfigurationSource
{
    readonly string endpointName;
    readonly ILog Log = LogManager.GetLogger(typeof(ConfigurationSource));

    public ConfigurationSource(string endpointName)
    {
        this.endpointName = endpointName;
    }

    public T GetConfiguration<T>() where T : class, new()
    {
        if (typeof(T) == typeof(UnicastBusConfig))
        {
            //read from existing config 
            var config = (UnicastBusConfig)ConfigurationManager.GetSection(typeof(UnicastBusConfig).Name);
            if (config != null) throw new InvalidOperationException("UnicastBUs Configuration should be in code.");

            return new UnicastBusConfig
            {
                MessageEndpointMappings = GenerateMappings()
            } as T;
        }

        // To in app.config for other sections not defined in this method, otherwise return null.
        return ConfigurationManager.GetSection(typeof(T).Name) as T;
    }

    MessageEndpointMappingCollection GenerateMappings()
    {
        var mappings = new MessageEndpointMappingCollection();

        var messageType = typeof(GatedPublishRunner.Event);

        Log.InfoFormat("Mapping {0} to {1}", messageType, endpointName);

        mappings.Add(new MessageEndpointMapping
        {
            AssemblyName = messageType.Assembly.FullName,
            TypeFullName = messageType.FullName,
            Endpoint = endpointName
        });

        return mappings;
    }
}
