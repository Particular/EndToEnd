#if Version6 || Version7
using NServiceBus.Routing;
using NServiceBus.Transport;

abstract class FakeAbstractTransport : TransportDefinition, IMessageDrivenSubscriptionTransport
{
}
#endif