namespace TransportCompatibilityTests.RabbitMQ
{
    using System;
    using System.Linq;
    using NUnit.Framework;
    using Common;
    using Common.Messages;
    using Common.RabbitMQ;
    using Infrastructure;

    [TestFixture]
    public class MessageExchangePatterns : RabbitMqContext
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
        public void It_is_possible_to_send_command_between_different_versions(int sourceVersion, int destinationVersion, Topology topology)
        {
            destinationEndpointDefinition.RoutingTopology = sourceEndpointDefinition.RoutingTopology = topology;
            sourceEndpointDefinition.Mappings = new []
            {
                new MessageMapping
                {
                    MessageType = typeof(TestCommand),
                    TransportAddress = destinationEndpointDefinition.TransportAddressForVersion(destinationVersion)
                }
            };

            using (var source = EndpointFacadeBuilder.CreateAndConfigure(sourceEndpointDefinition, sourceVersion))
            using (var destination = EndpointFacadeBuilder.CreateAndConfigure(destinationEndpointDefinition, destinationVersion))
            {
                var messageId = Guid.NewGuid();

                source.SendCommand(messageId);

                // ReSharper disable once AccessToDisposedClosure
                AssertEx.WaitUntilIsTrue(() => destination.ReceivedMessageIds.Any(mi => mi == messageId));
            }
        }

        [Category("RabbitMQ")]
        [Test, TestCaseSource(typeof(RabbitMqContext), nameof(GenerateVersionsPairs))]
        public void It_is_possible_to_publish_events(int sourceVersion, int destinationVersion, Topology topology)
        {
            destinationEndpointDefinition.RoutingTopology = sourceEndpointDefinition.RoutingTopology = topology;

            using (var source = EndpointFacadeBuilder.CreateAndConfigure(sourceEndpointDefinition, sourceVersion))
            using (var destination = EndpointFacadeBuilder.CreateAndConfigure(destinationEndpointDefinition, destinationVersion))
            {
                var eventId = Guid.NewGuid();

                source.PublishEvent(eventId);

                // ReSharper disable once AccessToDisposedClosure
                AssertEx.WaitUntilIsTrue(() => destination.ReceivedEventIds.Any(ei => ei == eventId));
            }
        }

        [Category("RabbitMQ")]
        [Test, TestCaseSource(typeof(RabbitMqContext), nameof(GenerateVersionsPairs))]
        public void It_is_possible_to_send_request_and_receive_reply(int sourceVersion, int destinationVersion, Topology topology)
        {
            destinationEndpointDefinition.RoutingTopology = sourceEndpointDefinition.RoutingTopology = topology;
            sourceEndpointDefinition.Mappings = new[]
            {
                new MessageMapping
                {
                    MessageType = typeof(TestRequest),
                    TransportAddress = destinationEndpointDefinition.TransportAddressForVersion(destinationVersion)
                }
            };

            using (var source = EndpointFacadeBuilder.CreateAndConfigure(sourceEndpointDefinition, sourceVersion))
            using (EndpointFacadeBuilder.CreateAndConfigure(destinationEndpointDefinition, destinationVersion))
            {
                var requestId = Guid.NewGuid();

                source.SendRequest(requestId);

                // ReSharper disable once AccessToDisposedClosure
                AssertEx.WaitUntilIsTrue(() => source.ReceivedResponseIds.Any(responseId => responseId == requestId));
            }
        }
    }
}
