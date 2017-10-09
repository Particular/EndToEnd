namespace AzureServiceBusV8
{
    using System;
    using System.Threading.Tasks;
    using NServiceBus;
    using TransportCompatibilityTests.Common;
    using TransportCompatibilityTests.Common.AzureServiceBus;
    using TransportCompatibilityTests.Common.Messages;

    public class EndpointFacade : MarshalByRefObject, IEndpointFacade
    {
        IEndpointInstance endpointInstance;
        MessageStore messageStore;
        CallbackResultStore callbackResultStore;
        SubscriptionStore subscriptionStore;

        async Task InitializeEndpoint(AzureServiceBusEndpointDefinition endpointDefinition)
        {
            var endpointConfiguration = new EndpointConfiguration(endpointDefinition.Name);

            endpointConfiguration.Conventions().DefiningMessagesAs(t => t.Namespace != null && t.Namespace.EndsWith(".Messages") && t != typeof(TestEvent));
            endpointConfiguration.Conventions().DefiningEventsAs(t => t == typeof(TestEvent));

            endpointConfiguration.EnableInstallers();
            endpointConfiguration.UsePersistence<InMemoryPersistence>();
            var transportConfiguration = endpointConfiguration.UseTransport<AzureServiceBusTransport>();
            transportConfiguration
                .UseEndpointOrientedTopology()
                .RegisterPublisher(typeof(TestEvent), "source")
                .ConnectionString(AzureServiceBusConnectionStringBuilder.Build)
                .Sanitization()
                .UseStrategy<ValidateAndHashIfNeeded>();

            var routing = transportConfiguration.Routing();
            foreach (var mapping in endpointDefinition.Mappings)
            {
                routing.RouteToEndpoint(mapping.MessageType, mapping.TransportAddress);
            }

            endpointConfiguration.SendFailedMessagesTo("error");

            endpointConfiguration.UseSerialization<NewtonsoftSerializer>();

            endpointConfiguration.MakeInstanceUniquelyAddressable(Guid.NewGuid() + "A");

            messageStore = new MessageStore();
            callbackResultStore = new CallbackResultStore();
            subscriptionStore = new SubscriptionStore();

            endpointConfiguration.RegisterComponents(c => c.RegisterSingleton(messageStore));
            endpointConfiguration.RegisterComponents(c => c.RegisterSingleton(subscriptionStore));

            endpointInstance = await Endpoint.Start(endpointConfiguration);
        }

        public void Bootstrap(EndpointDefinition endpointDefinition)
        {
            InitializeEndpoint(endpointDefinition.As<AzureServiceBusEndpointDefinition>())
                .GetAwaiter()
                .GetResult();
        }

        public void SendCommand(Guid messageId)
        {
            endpointInstance.Send(new TestCommand { Id = messageId }).GetAwaiter().GetResult();
        }

        public void SendRequest(Guid requestId)
        {
            endpointInstance.Send(new TestRequest { RequestId = requestId }).GetAwaiter().GetResult();
        }

        public void PublishEvent(Guid eventId)
        {
            endpointInstance.Publish(new TestEvent { EventId = eventId }).GetAwaiter().GetResult();
        }

        public void SendAndCallbackForInt(int value)
        {
            Task.Run(async () =>
            {
                var result = await endpointInstance.Request<int>(new TestIntCallback { Response = value }, new SendOptions());

                callbackResultStore.Add(result);
            });
        }

        public void SendAndCallbackForEnum(CallbackEnum value)
        {
            Task.Run(async () =>
            {
                var result = await endpointInstance.Request<CallbackEnum>(new TestEnumCallback { CallbackEnum = value }, new SendOptions());

                callbackResultStore.Add(result);
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
            endpointInstance.Stop().GetAwaiter().GetResult();
        }
    }
}