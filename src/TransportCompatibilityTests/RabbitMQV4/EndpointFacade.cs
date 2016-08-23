namespace RabbitMQV4
{
    using System;
    using System.Threading.Tasks;
    using NServiceBus;
    using TransportCompatibilityTests.Common;
    using TransportCompatibilityTests.Common.Messages;
    using TransportCompatibilityTests.Common.RabbitMQ;

    public class EndpointFacade : MarshalByRefObject, IEndpointFacade
    {
        MessageStore messageStore;
        CallbackResultStore callbackResultStore;
        IEndpointInstance endpointInstance;

        public void Bootstrap(EndpointDefinition endpointDefinition)
        {
            InitializeEndpoint(endpointDefinition.As<RabbitMqEndpointDefinition>())
               .GetAwaiter()
               .GetResult();
        }

        async Task InitializeEndpoint(RabbitMqEndpointDefinition endpointDefinition)
        {
            var endpointConfiguration = new EndpointConfiguration(endpointDefinition.Name);

            endpointConfiguration.Conventions()
                .DefiningMessagesAs(
                    t => t.Namespace != null && t.Namespace.EndsWith(".Messages") && t != typeof(TestEvent));
            endpointConfiguration.Conventions().DefiningEventsAs(t => t == typeof(TestEvent));
            endpointConfiguration.Conventions().DefiningCommandsAs(t => t.FullName.EndsWith("Command"));

            endpointConfiguration.UsePersistence<InMemoryPersistence>();
            endpointConfiguration.EnableInstallers();
            var transportExtensions = endpointConfiguration.UseTransport<RabbitMQTransport>()
                .ConnectionString(RabbitConnectionStringBuilder.Build());

            endpointConfiguration.CustomConfigurationSource(new CustomConfiguration(endpointDefinition.Mappings));
            endpointConfiguration.MakeInstanceUniquelyAddressable("A");

            if (endpointDefinition.RoutingTopology == Topology.Direct)
            {
                transportExtensions.UseDirectRoutingTopology();
            }

            messageStore = new MessageStore();
            callbackResultStore = new CallbackResultStore();

            endpointConfiguration.RegisterComponents(c => c.RegisterSingleton(messageStore));

            endpointInstance = await Endpoint.Start(endpointConfiguration);
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

        public int NumberOfSubscriptions => 0; 

        public void Dispose()
        {
            endpointInstance.Stop().GetAwaiter().GetResult();
        }
    }
}
