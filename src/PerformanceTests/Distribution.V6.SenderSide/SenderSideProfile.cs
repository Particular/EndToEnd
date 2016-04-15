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

        var senderSideArgs = this.FetchSetting("SenderSide").Split(';');

        cfg.SendFailedMessagesTo("error@" + senderSideArgs[0]);

        var machines = senderSideArgs[1].Split('|');

        var routing = cfg.UnicastRouting();

        var endpoint = new EndpointName(Context.EndpointName);

        var instances = machines.Select(x => new EndpointInstance(endpoint, x)).ToArray();
        routing.Mapping.Physical.Add(endpoint, instances);
    }
}
