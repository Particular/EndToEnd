#if !Version7 && !Version6
using Configuration = NServiceBus.BusConfiguration;
using System;
using System.Configuration;
using NServiceBus.Config;
using NServiceBus.Config.ConfigurationSource;

partial class BaseRunner : IConfigurationSource
{
    public T GetConfiguration<T>() where T : class, new()
    {
        IConfigureUnicastBus configureUnicastBus;

        //read from existing config 
        var config = (UnicastBusConfig)ConfigurationManager.GetSection(typeof(UnicastBusConfig).Name);
        if (config != null) throw new InvalidOperationException("UnicastBus Configuration should be in code using IConfigureUnicastBus interface.");

        if (typeof(T) == typeof(UnicastBusConfig) && null != (configureUnicastBus = this as IConfigureUnicastBus))
        {
            var mappings = new MessageEndpointMappingCollection();
            foreach (var m in configureUnicastBus.GenerateMappings())
            {
                mappings.Add(new MessageEndpointMapping
                {
                    AssemblyName = m.MessageType.Assembly.FullName,
                    TypeFullName = m.MessageType.FullName,
                    Endpoint = m.Endpoint
                });
            }

            return new UnicastBusConfig
            {
                MessageEndpointMappings = mappings
            } as T;
        }

        return ConfigurationManager.GetSection(typeof(T).Name) as T;
    }
}
#endif
