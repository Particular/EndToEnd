#if Version6 || Version7
using Configuration = NServiceBus.EndpointConfiguration;
#if Version7
using NServiceBus.Configuration.AdvancedExtensibility;
#endif
#if Version6
using NServiceBus.Configuration.AdvanceExtensibility;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using NServiceBus;

partial class BaseRunner
{
    async Task CreateOrPurgeAndDrainQueues()
    {
        var configuration = CreateConfiguration();
        if (IsPurgingSupported) configuration.PurgeOnStartup(true);
        ShortcutBehavior.Shortcut = true; // Required, as instance already receives messages before DrainMessages() is called!
        var instance = await Endpoint.Start(configuration).ConfigureAwait(false);
        await DrainMessages().ConfigureAwait(false);
        await new Session(instance).CloseWithSuppress().ConfigureAwait(false);
    }

    async Task CreateSendOnlyEndpoint()
    {
        var configuration = CreateConfiguration();
        configuration.SendOnly();
        var instance = await Endpoint.Start(configuration).ConfigureAwait(false);
        Session = new Session(instance);
    }

    async Task CreateEndpoint()
    {
        var configuration = CreateConfiguration();

        ConfigureMessageMappings(configuration);

        if (IsSendOnly)
        {
            configuration.SendOnly();
            Session = new Session(await Endpoint.Start(configuration).ConfigureAwait(false));
            return;
        }

        var instance = Endpoint.Start(configuration).ConfigureAwait(false).GetAwaiter().GetResult();
        Session = new Session(instance);
    }

    void ConfigureMessageMappings(Configuration configuration)
    {
        var x = this as IConfigureUnicastBus;

        if (x != null)
        {
            var routing = new RoutingSettings<FakeAbstractTransport>(configuration.GetSettings());

            foreach (var m in x.GenerateMappings())
            {
                var isEvent = typeof(IEvent).IsAssignableFrom(m.MessageType);
                if (isEvent) routing.RegisterPublisher(m.MessageType, m.Endpoint);
                else routing.RouteToEndpoint(m.MessageType, m.Endpoint);

            }
        }
    }

    Configuration CreateConfiguration()
    {
        var configuration = new Configuration(EndpointName);
        configuration.EnableInstallers();
        configuration.AssemblyScanner().ExcludeTypes(GetTypesToExclude().ToArray());
        configuration.ApplyProfiles(this);
        configuration.DefineCriticalErrorAction(OnCriticalError);
        return configuration;
    }

    IEnumerable<Type> GetTypesToExclude()
    {
        return GetTypesToExclude(Assembly.GetAssembly(this.GetType()).GetTypes());
    }

    async Task OnCriticalError(ICriticalErrorContext context)
    {
        try
        {
            try
            {
                Log.Fatal("OnCriticalError", context.Exception);
                await context.Stop().ConfigureAwait(false);
            }
            finally
            {
                NLog.LogManager.Shutdown();
            }
        }
        finally
        {
            Environment.FailFast("NServiceBus critical error", context.Exception);
        }
    }
}
#endif
