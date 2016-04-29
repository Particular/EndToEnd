using System;
using NServiceBus;
using Variables;
using NServiceBus.Config;
using NServiceBus.Config.ConfigurationSource;
using System.Configuration;

class AzureStorageQueueProfile : IProfile, INeedContext
{
    public IContext Context { private get; set; }

    public void Configure(BusConfiguration busConfiguration)
    {
        if ((int)MessageSize.Medium < (int)Context.Permutation.MessageSize) throw new NotSupportedException($"Message size {Context.Permutation.MessageSize} not supported by ASQ.");

        busConfiguration
            .CustomConfigurationSource(new FlrConfig());

        busConfiguration
            .UseTransport<AzureStorageQueueTransport>()
            .ConnectionString(ConfigurationHelper.GetConnectionString("AzureStorageQueue"));
    }
}

class FlrConfig : IConfigurationSource
{
    public T GetConfiguration<T>() where T : class, new()
    {
        //To Provide FLR Config
        if (typeof(T) == typeof(TransportConfig))
        {
            TransportConfig flrConfig = new TransportConfig
            {
                MaxRetries = 2
            };

            return flrConfig as T;
        }

        // To in app.config for other sections not defined in this method, otherwise return null.
        return ConfigurationManager.GetSection(typeof(T).Name) as T;
    }
}