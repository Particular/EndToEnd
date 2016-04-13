using NServiceBus.Config;

public interface IConfigureUnicastBus
{
    MessageEndpointMappingCollection GenerateMappings();
}

//public class ConfigurationSource : IConfigurationSource
//{
//    readonly BaseRunner baseRunner;
//    readonly ILog Log = LogManager.GetLogger(typeof(ConfigurationSource));

//    public ConfigurationSource(BaseRunner baseRunner)
//    {
//        this.baseRunner = baseRunner;
//    }

//    public T GetConfiguration<T>() where T : class, new()
//    {
//        var xxx = baseRunner as IConfigureUnicastBus;
//        if (xxx == null) return ConfigurationManager.GetSection(typeof(T).Name) as T;

//        if (typeof(T) == typeof(UnicastBusConfig))
//        {
//            //read from existing config 
//            var config = (UnicastBusConfig)ConfigurationManager.GetSection(typeof(UnicastBusConfig).Name);
//            if (config != null) throw new InvalidOperationException("UnicastBUs Configuration should be in code.");

//            return new UnicastBusConfig
//            {
//                MessageEndpointMappings = xxx.GenerateMappings()
//            } as T;
//        }

//        return ConfigurationManager.GetSection(typeof(T).Name) as T;
//    }
//}
