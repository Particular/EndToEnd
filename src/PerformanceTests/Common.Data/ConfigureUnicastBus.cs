using System.Configuration;
using System.IO;
using System.Reflection;
using NServiceBus.Config;
using NServiceBus.Config.ConfigurationSource;
using NServiceBus.Logging;
using Variables;

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

        ////append mapping to config
        //config.MessageEndpointMappings.Add(
        //    new MessageEndpointMapping
        //    {
        //        AssemblyName = "NServiceBus5.x86",//new FileInfo(Assembly.GetExecutingAssembly().Location).Name, //.Replace(Platform.x64.ToString(), Platform.x86.ToString()),
        //        Endpoint = Host.Program.endpointName
        //    });
        return config;
    }

    MessageEndpointMappingCollection GenerateMappings()
    {
        var mappings = new MessageEndpointMappingCollection();

        //foreach (var templateMapping in configuration.EndpointMappings)
        //{
            var messageType = typeof(GatedPublishRunner.Event);
            var endpoint = EndpointName;

        Log.InfoFormat("Mapping {0} to {1}", messageType, endpoint);

        mappings.Add(new MessageEndpointMapping
            {
                AssemblyName = messageType.Assembly.FullName,
                TypeFullName = messageType.FullName,
                Endpoint = endpoint
            });
        //}

        return mappings;
    }

}
