namespace TransportCompatibilityTests.RabbitMQ
{
    using System.Linq;
    using NUnit.Framework;
    using Common;
    using Common.Messages;
    using Common.RabbitMQ;
    using Infrastructure;

    [TestFixture]
    public class Callbacks : RabbitMqContext
    {
        RabbitMqEndpointDefinition sourceEndpointDefinition;
        RabbitMqEndpointDefinition destinationEndpointDefinition;

        [SetUp]
        public void SetUp()
        {
            sourceEndpointDefinition = new RabbitMqEndpointDefinition { Name = "src" };
            destinationEndpointDefinition = new RabbitMqEndpointDefinition { Name = "dst" };
        }

        [Category("RabbitMQ")]
        [Test, TestCaseSource(typeof(RabbitMqContext), nameof(GenerateVersionsPairs))]
        // ReSharper disable once InconsistentNaming
        public void Int_callbacks_work(int sourceVersion, int destinationVersion, Topology topology)
        {
            destinationEndpointDefinition.RoutingTopology = sourceEndpointDefinition.RoutingTopology = topology;
            sourceEndpointDefinition.Mappings = new[]
            {
                new MessageMapping
                {
                    MessageType = typeof(TestIntCallback),
                    TransportAddress = destinationEndpointDefinition.TransportAddressForVersion(destinationVersion)
                }
            };

            using (var source = EndpointFacadeBuilder.CreateAndConfigure(sourceEndpointDefinition, sourceVersion))
            using (EndpointFacadeBuilder.CreateAndConfigure(destinationEndpointDefinition, destinationVersion))
            {
                var value = 42;

                source.SendAndCallbackForInt(value);

                // ReSharper disable once AccessToDisposedClosure
                AssertEx.WaitUntilIsTrue(() => source.ReceivedIntCallbacks.Contains(value));
            }
        }

        [Category("RabbitMQ")]
        [Test, TestCaseSource(typeof(RabbitMqContext), nameof(GenerateVersionsPairs))]
        // ReSharper disable once InconsistentNaming
        public void Enum_callbacks_work(int sourceVersion, int destinationVersion, Topology topology)
        {
            destinationEndpointDefinition.RoutingTopology = sourceEndpointDefinition.RoutingTopology = topology;
            sourceEndpointDefinition.Mappings = new[]
            {
                new MessageMapping
                {
                    MessageType = typeof(TestEnumCallback),
                    TransportAddress = destinationEndpointDefinition.TransportAddressForVersion(destinationVersion)
                }
            };

            using (var source = EndpointFacadeBuilder.CreateAndConfigure(sourceEndpointDefinition, sourceVersion))
            using (EndpointFacadeBuilder.CreateAndConfigure(destinationEndpointDefinition, destinationVersion))
            {
                var value = CallbackEnum.Three;

                source.SendAndCallbackForEnum(value);

                // ReSharper disable once AccessToDisposedClosure
                AssertEx.WaitUntilIsTrue(() => source.ReceivedEnumCallbacks.Contains(value));
            }
        }
    }
}
