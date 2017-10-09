namespace AzureStorageQueuesV8
{
    using System;
    using System.Threading.Tasks;
    using NServiceBus;
    using NServiceBus.Logging;
    using NServiceBus.Pipeline;
    using TransportCompatibilityTests.Common;
    using TransportCompatibilityTests.Common.AzureStorageQueues;
    using TransportCompatibilityTests.Common.Messages;

    public class EndpointFacade : MarshalByRefObject, IEndpointFacade
    {
        //private IBusSession busSession;
        IEndpointInstance endpointInstance;
        CallbackResultStore callbackResultStore;
        MessageStore messageStore;
        SubscriptionStore subscriptionStore;

        public void Bootstrap(EndpointDefinition endpointDefinition)
        {
            InitializeEndpoint(endpointDefinition.As<AzureStorageQueuesEndpointDefinition>())
                .GetAwaiter()
                .GetResult();
        }

        async Task InitializeEndpoint(AzureStorageQueuesEndpointDefinition endpointDefinition)
        {
            var defaultFactory = LogManager.Use<DefaultFactory>();
            defaultFactory.Level(LogLevel.Error);

            var endpointConfiguration = new EndpointConfiguration(endpointDefinition.Name);

            endpointConfiguration.SendFailedMessagesTo("error");

            endpointConfiguration.Conventions().DefiningMessagesAs(t => t.Namespace != null && t.Namespace.EndsWith(".Messages") && t != typeof(TestEvent));
            endpointConfiguration.Conventions().DefiningEventsAs(t => t == typeof(TestEvent));

            endpointConfiguration.EnableInstallers();
            endpointConfiguration.UsePersistence<InMemoryPersistence>();
            var transportConfiguration = endpointConfiguration.UseTransport<AzureStorageQueueTransport>()
                .ConnectionString(AzureStorageQueuesConnectionStringBuilder.Build())
                .SanitizeQueueNamesWith(q => q.Replace(".", "-"));

            var routing = transportConfiguration.Routing();
            foreach (var mapping in endpointDefinition.Mappings)
            {
                routing.RouteToEndpoint(mapping.MessageType, mapping.TransportAddress);
            }
            foreach (var publisher in endpointDefinition.Publishers)
            {
                routing.RegisterPublisher(publisher.MessageType, publisher.TransportAddress);
            }

            endpointConfiguration.UseSerialization<NewtonsoftSerializer>();

            messageStore = new MessageStore();
            subscriptionStore = new SubscriptionStore();
            callbackResultStore = new CallbackResultStore();

            endpointConfiguration.RegisterComponents(c => c.RegisterSingleton(messageStore));
            endpointConfiguration.RegisterComponents(c => c.RegisterSingleton(subscriptionStore));

            endpointConfiguration.Pipeline.Register<SubscriptionMonitoringBehavior.Registration>();

            try
            {
                endpointInstance = await Endpoint.Start(endpointConfiguration);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
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
            throw new NotImplementedException();
        }

        public void SendAndCallbackForEnum(CallbackEnum value)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            endpointInstance.Stop().GetAwaiter().GetResult();
        }

        public Guid[] ReceivedMessageIds => messageStore.GetAll();

        public Guid[] ReceivedResponseIds => messageStore.Get<TestResponse>();

        public Guid[] ReceivedEventIds => messageStore.Get<TestEvent>();

        public int[] ReceivedIntCallbacks => callbackResultStore.Get<int>();

        public CallbackEnum[] ReceivedEnumCallbacks => callbackResultStore.Get<CallbackEnum>();

        public int NumberOfSubscriptions => subscriptionStore.NumberOfSubscriptions;

        class SubscriptionMonitoringBehavior : Behavior<IIncomingPhysicalMessageContext>
        {
            public SubscriptionStore SubscriptionStore { get; set; }

            public override async Task Invoke(IIncomingPhysicalMessageContext context, Func<Task> next)
            {
                await next();
                string intent;

                if (context.Message.Headers.TryGetValue(Headers.MessageIntent, out intent) && intent == "Subscribe")
                {
                    SubscriptionStore.Increment();
                }
            }

            internal class Registration : RegisterStep
            {
                public Registration()
                    : base("SubscriptionBehavior", typeof(SubscriptionMonitoringBehavior), "So we can get subscription events")
                {
                    InsertBefore("ProcessSubscriptionRequests");
                }
            }
        }
    }
}
