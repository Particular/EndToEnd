using System;
using NServiceBus;
using TransportCompatibilityTests.Common;
using TransportCompatibilityTests.Common.Messages;

namespace AzureServiceBusV6
{
    using System.Threading.Tasks;
    using TransportCompatibilityTests.Common.AzureServiceBus;

    public class EndpointFacade : MarshalByRefObject, IEndpointFacade
    {
        IBus bus;
        MessageStore messageStore;
        CallbackResultStore callbackResultStore;
        SubscriptionStore subscriptionStore;

        public void Bootstrap(EndpointDefinition endpointDefinition)
        {
            var busConfiguration = new BusConfiguration();

            busConfiguration.Conventions()
                .DefiningMessagesAs(
                    t => t.Namespace != null && t.Namespace.EndsWith(".Messages") && t != typeof(TestEvent));
            busConfiguration.Conventions().DefiningEventsAs(t => t == typeof(TestEvent));

            busConfiguration.EndpointName(endpointDefinition.Name);
            busConfiguration.UsePersistence<InMemoryPersistence>();
            busConfiguration.EnableInstallers();
            busConfiguration.UseTransport<AzureServiceBusTransport>().ConnectionString(AzureServiceBusConnectionStringBuilder.Build());

            busConfiguration.ScaleOut().UseSingleBrokerQueue();

            var customConfiguration = new CustomConfiguration(endpointDefinition.As<AzureServiceBusEndpointDefinition>().Mappings);
            busConfiguration.CustomConfigurationSource(customConfiguration);

            messageStore = new MessageStore();
            subscriptionStore = new SubscriptionStore();
            callbackResultStore = new CallbackResultStore();

            busConfiguration.RegisterComponents(c => c.RegisterSingleton(messageStore));
            busConfiguration.RegisterComponents(c => c.RegisterSingleton(subscriptionStore));

            var startableBus = Bus.Create(busConfiguration);

            bus = startableBus.Start();
        }

        public void SendCommand(Guid messageId)
        {
            bus.Send(new TestCommand {Id = messageId});
        }

        public void SendRequest(Guid requestId)
        {
            bus.Send(new TestRequest {RequestId = requestId});
        }

        public void PublishEvent(Guid eventId)
        {
            bus.Publish(new TestEvent {EventId = eventId});
        }

        public void SendAndCallbackForEnum(CallbackEnum value)
        {
            Task.Run(async () =>
            {
                var res = await bus.Send(new TestEnumCallback {CallbackEnum = value}).Register<CallbackEnum>();

                callbackResultStore.Add(res);
            });
        }

        public void SendAndCallbackForInt(int value)
        {
            Task.Run(async () =>
            {
                var res = await bus.Send(new TestIntCallback {Response = value}).Register();

                callbackResultStore.Add(res);
            });
        }

        public Guid[] ReceivedMessageIds => messageStore.GetAll();

        public Guid[] ReceivedResponseIds => messageStore.Get<TestResponse>();

        public Guid[] ReceivedEventIds => messageStore.Get<TestEvent>();

        public int[] ReceivedIntCallbacks => callbackResultStore.Get<int>();

        public CallbackEnum[] ReceivedEnumCallbacks => callbackResultStore.Get<CallbackEnum>();

        public int NumberOfSubscriptions => subscriptionStore.NumberOfSubscriptions;

        public void Dispose()
        {
            bus.Dispose();
        }
    }
}