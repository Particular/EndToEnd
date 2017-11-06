#if Version5
using Configuration = NServiceBus.BusConfiguration;

using System;
using System.Linq;
using System.Reflection;
using NServiceBus;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using NServiceBus.Config;
using NServiceBus.Config.ConfigurationSource;

partial class BaseRunner
    : IConfigurationSource
{
    async Task CreateOrPurgeAndDrainQueues()
    {
        var configuration = CreateConfiguration();
        if (IsPurgingSupported) configuration.PurgeOnStartup(true);
        ShortcutBehavior.Shortcut = true; // Required, as instance already receives messages before DrainMessages() is called!
        var instance = Bus.Create(configuration).Start();
        await DrainMessages().ConfigureAwait(false);
        await new Session(instance).CloseWithSuppress().ConfigureAwait(false);
    }

    Task CreateSendOnlyEndpoint()
    {
        var configuration = CreateConfiguration();
        var instance = Bus.CreateSendOnly(configuration);
        Session = new Session(instance);
        return Task.FromResult(0);
    }

    Task CreateEndpoint()
    {
        var configuration = CreateConfiguration();
        configuration.CustomConfigurationSource(this);
        configuration.DefineCriticalErrorAction(OnCriticalError);

        if (IsSendOnly)
        {
            Session = new Session(Bus.CreateSendOnly(configuration));
            return Task.FromResult(0);
        }

        configuration.PurgeOnStartup(!IsSeedingData && IsPurgingSupported);
        Session = new Session(Bus.Create(configuration).Start());
        return Task.FromResult(0);
    }

    BusConfiguration CreateConfiguration()
    {
        var configuration = new Configuration();
        configuration.EndpointName(EndpointName);
        configuration.EnableInstallers();

        var scanableTypes = GetTypesToInclude();
        configuration.TypesToScan(scanableTypes);

        configuration.ApplyProfiles(this);

        return configuration;
    }

    List<Type> GetTypesToInclude()
    {
        var location = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var asm = new NServiceBus.Hosting.Helpers.AssemblyScanner(location).GetScannableAssemblies();

        var allTypes = (from a in asm.Assemblies
                        from b in a.GetLoadableTypes()
                        select b).ToList();

        var allTypesToExclude = GetTypesToExclude(allTypes);

        var finalInternalListToScan = allTypes.Except(allTypesToExclude);

        return finalInternalListToScan.ToList();
    }

    void OnCriticalError(string errorMessage, Exception exception)
    {
        try
        {
            Log.Fatal("OnCriticalError", exception);
            Session.CloseWithSuppress().ConfigureAwait(false).GetAwaiter().GetResult();
        }
        finally
        {
            Environment.FailFast("NServiceBus critical error", exception);
        }
    }

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