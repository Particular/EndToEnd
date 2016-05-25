namespace AzureStorageQueuesV6
{
    using System;
    using NServiceBus;
    using NServiceBus.Pipeline;
    using NServiceBus.Pipeline.Contexts;
    using TransportCompatibilityTests.Common;
    using TransportCompatibilityTests.Common.AzureStorageQueues;
    using TransportCompatibilityTests.Common.Messages;

    public class EndpointFacade : MarshalByRefObject, IEndpointFacade
    {
        IBus bus;
        MessageStore messageStore;
        CallbackResultStore callbackResultStore;
        SubscriptionStore subscriptionStore;

        public void Bootstrap(EndpointDefinition endpointDefinition)
        {
            InitializeEndpoint(endpointDefinition.As<AzureStorageQueuesEndpointDefinition>());
        }

        public void InitializeEndpoint(AzureStorageQueuesEndpointDefinition endpointDefinition)
        {
            var busConfiguration = new BusConfiguration();

            busConfiguration.Conventions()
                .DefiningMessagesAs(t => t.Namespace != null && t.Namespace.EndsWith(".Messages") && t != typeof(TestEvent));
            busConfiguration.Conventions()
                .DefiningEventsAs(t => t == typeof(TestEvent));

            busConfiguration.EndpointName(endpointDefinition.Name);
            busConfiguration.UsePersistence<InMemoryPersistence>();
            busConfiguration.EnableInstallers();
            busConfiguration.UseTransport<AzureStorageQueueTransport>()
                .ConnectionString(AzureStorageQueuesConnectionStringBuilder.Build());

            busConfiguration.CustomConfigurationSource(new CustomConfiguration(endpointDefinition.Mappings));

            messageStore = new MessageStore();
            subscriptionStore = new SubscriptionStore();
            callbackResultStore = new CallbackResultStore();

            busConfiguration.RegisterComponents(c => c.RegisterSingleton(messageStore));
            busConfiguration.RegisterComponents(c => c.RegisterSingleton(subscriptionStore));

            busConfiguration.Pipeline.Register<SubscriptionBehavior.Registration>();

            var startableBus = Bus.Create(busConfiguration);

            bus = startableBus.Start();
        }

        public void SendCommand(Guid messageId)
        {
            bus.Send(new TestCommand { Id = messageId });
        }

        public void SendRequest(Guid requestId)
        {
            bus.Send(new TestRequest { RequestId = requestId });
        }

        public void PublishEvent(Guid eventId)
        {
            bus.Publish(new TestEvent { EventId = eventId });
        }

        public void SendAndCallbackForInt(int value)
        {
            throw new NotImplementedException();
        }

        public void SendAndCallbackForEnum(CallbackEnum value)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            bus.Dispose();
        }

        public Guid[] ReceivedMessageIds => messageStore.GetAll();

        public Guid[] ReceivedResponseIds => messageStore.Get<TestResponse>();

        public Guid[] ReceivedEventIds => messageStore.Get<TestEvent>();

        public int[] ReceivedIntCallbacks => callbackResultStore.Get<int>();

        public CallbackEnum[] ReceivedEnumCallbacks => callbackResultStore.Get<CallbackEnum>();

        public int NumberOfSubscriptions => subscriptionStore.NumberOfSubscriptions;

        class SubscriptionBehavior : IBehavior<IncomingContext>
        {
            public SubscriptionStore SubscriptionStore { get; set; }

            public void Invoke(IncomingContext context, Action next)
            {
                next();

                string intent;

                if (context.PhysicalMessage.Headers.TryGetValue(Headers.MessageIntent, out intent) && intent == "Subscribe")
                {
                    SubscriptionStore.Increment();
                }
            }

            internal class Registration : RegisterStep
            {
                public Registration()
                    : base("SubscriptionBehavior", typeof(SubscriptionBehavior), "So we can get subscription events")
                {
                    InsertBefore(WellKnownStep.CreateChildContainer);
                }
            }
        }
    }
}
