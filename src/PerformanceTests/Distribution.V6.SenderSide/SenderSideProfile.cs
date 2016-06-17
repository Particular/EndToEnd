using System;
using System.Linq;
using NServiceBus;
using NServiceBus.Routing;
using Variables;

class SenderSideProfile : IProfile, INeedContext
{
    public IContext Context { private get; set; }

    public void Configure(EndpointConfiguration cfg)
    {
        if (Context.Permutation.ScaleOut != ScaleOut.SenderSide) return;
        if (Context.Permutation.Transport != Transport.MSMQ) throw new NotSupportedException("SenderSide should only be used with MSMQ");

        var senderSideArgs = ConfigurationHelper.FetchSetting("SenderSide");

        if(string.IsNullOrWhiteSpace(senderSideArgs)) throw new InvalidOperationException("Setting `SenderSide` not resolved.");

        var machines = senderSideArgs.Split('|');
        var routing = cfg.UnicastRouting();
        var endpoint = new EndpointName(Context.EndpointName);
        var instances = machines.Select(x => new EndpointInstance(endpoint).AtMachine(x)).ToArray();
        routing.Mapping.Physical.Add(instances);
    }
}
