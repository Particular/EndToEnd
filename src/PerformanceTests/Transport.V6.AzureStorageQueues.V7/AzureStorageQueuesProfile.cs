using System;
using NServiceBus;
using Variables;
using NServiceBus.Config.ConfigurationSource;
using NServiceBus.Config;
using System.Configuration;

class AzureStorageQueuesProfile : IProfile, INeedContext
{
    public IContext Context { private get; set; }

    public void Configure(EndpointConfiguration endpointConfiguration)
    {
        if ((int)MessageSize.Medium < (int)Context.Permutation.MessageSize) throw new NotSupportedException($"Message size {Context.Permutation.MessageSize} not supported by ASQ.");

        endpointConfiguration
            .CustomConfigurationSource(new FlrConfig());

        endpointConfiguration
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

        // To provide SLR Config
        if (typeof(T) == typeof(SecondLevelRetriesConfig))
        {
            SecondLevelRetriesConfig slrConfig = new SecondLevelRetriesConfig
            {
                Enabled = true,
                NumberOfRetries = 2,
                TimeIncrease = TimeSpan.FromSeconds(10)
            };

            return slrConfig as T;
        }

        // To in app.config for other sections not defined in this method, otherwise return null.
        return ConfigurationManager.GetSection(typeof(T).Name) as T;
    }
}