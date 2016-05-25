namespace RabbitMQV1
{
    using System;
    using System.Threading.Tasks;
    using NServiceBus;
    using TransportCompatibilityTests.Common;
    using TransportCompatibilityTests.Common.Messages;
    using TransportCompatibilityTests.Common.RabbitMQ;

    public class EndpointFacade : MarshalByRefObject, IEndpointFacade
    {
        IBus bus;
        IStartableBus startableBus;
        MessageStore messageStore;
        CallbackResultStore callbackResultStore;
        
        public void Bootstrap(EndpointDefinition endpointDefinition)
        {
            var configure = Configure.With();
            configure.DefaultBuilder();

            configure.DefineEndpointName(endpointDefinition.Name);
            Address.InitializeLocalAddress(endpointDefinition.Name);

            configure.DefiningMessagesAs(t => t.Namespace != null && t.Namespace.EndsWith(".Messages") && t != typeof(TestEvent));
            configure.DefiningEventsAs(t => t == typeof(TestEvent));

            configure.UseInMemoryTimeoutPersister();
            configure.InMemorySubscriptionStorage();
            configure.UseTransport<RabbitMQ>(RabbitConnectionStringBuilder.Build);

            var rabbitMqEndpointDefinition = endpointDefinition.As<RabbitMqEndpointDefinition>();
            var customConfiguration = new CustomConfiguration(rabbitMqEndpointDefinition.Mappings);
            configure.CustomConfigurationSource(customConfiguration);
            configure.Configurer.ConfigureComponent<MessageStore>(DependencyLifecycle.SingleInstance);

            if (rabbitMqEndpointDefinition.RoutingTopology == Topology.Direct)
            {
                Configure.Transports.RabbitMq(settings => settings.UseDirectRoutingTopology());
            }

            startableBus = configure.UnicastBus().CreateBus();
            bus = startableBus.Start(() => configure.ForInstallationOn<NServiceBus.Installation.Environments.Windows>().Install());

            messageStore = (MessageStore)configure.Builder.Build(typeof(MessageStore));
            callbackResultStore = new CallbackResultStore();
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

        public void SendAndCallbackForEnum(CallbackEnum value)
        {
            Task.Run(async () =>
            {
                var res = await bus.Send(new TestEnumCallback { CallbackEnum = value }).Register<CallbackEnum>();

                callbackResultStore.Add(res);
            });
        }

        public void SendAndCallbackForInt(int value)
        {
            Task.Run(async () =>
            {
                var res = await bus.Send(new TestIntCallback { Response = value }).Register();

                callbackResultStore.Add(res);
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
            startableBus.Dispose();
        }
    }
}
