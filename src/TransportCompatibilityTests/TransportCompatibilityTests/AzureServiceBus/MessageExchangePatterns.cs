using System;
using System.Linq;
using NUnit.Framework;
using TransportCompatibilityTests.Common;
using TransportCompatibilityTests.Common.Messages;

// ReSharper disable InconsistentNaming

namespace TransportCompatibilityTests.AzureServiceBus
{
    using Common.AzureServiceBus;

    [TestFixture]
    public class MessageExchangePatterns : AzureServiceBusContext
    {
        AzureServiceBusEndpointDefinition sourceEndpointDefinition;
        AzureServiceBusEndpointDefinition destinationEndpointDefinition;

        [SetUp]
        public void SetUp()
        {
            sourceEndpointDefinition = new AzureServiceBusEndpointDefinition { Name = "Source" };
            destinationEndpointDefinition = new AzureServiceBusEndpointDefinition { Name = "Destination" };
        }

        [Category("AzureServiceBus")]
        [Test, TestCaseSource(nameof(GenerateVersionsPairs))]
        public void It_is_possible_to_send_command_between_different_versions(int sourceVersion, int destinationVersion)
        {
            sourceEndpointDefinition.Mappings = new[]
            {
                new MessageMapping
                {
                    MessageType = typeof(TestCommand),
                    TransportAddress = destinationEndpointDefinition.TransportAddressForVersion(destinationVersion)
                }
            };

            using (var source = EndpointFacadeBuilder.CreateAndConfigure(sourceEndpointDefinition, sourceVersion))
            {
                using (var destination = EndpointFacadeBuilder.CreateAndConfigure(destinationEndpointDefinition, destinationVersion))
                {
                    var messageId = Guid.NewGuid();

                    source.SendCommand(messageId);

                    // ReSharper disable once AccessToDisposedClosure
                    AssertEx.WaitUntilIsTrue(() => destination.ReceivedMessageIds.Any(mi => mi == messageId));
                }
            }
        }

        [Category("AzureServiceBus")]
        [Test, TestCaseSource(nameof(GenerateVersionsPairs))]
        public void It_is_possible_to_send_request_and_receive_replay(int sourceVersion, int destinationVersion)
        {
            sourceEndpointDefinition.Mappings = new[]
            {
                new MessageMapping
                {
                    MessageType = typeof(TestRequest),
                    TransportAddress = destinationEndpointDefinition.TransportAddressForVersion(destinationVersion)
                }
            };

            using (var source = EndpointFacadeBuilder.CreateAndConfigure(sourceEndpointDefinition, sourceVersion))
            {
                using (EndpointFacadeBuilder.CreateAndConfigure(destinationEndpointDefinition, destinationVersion))
                {
                    var requestId = Guid.NewGuid();

                    source.SendRequest(requestId);

                    // ReSharper disable once AccessToDisposedClosure
                    AssertEx.WaitUntilIsTrue(() => source.ReceivedResponseIds.Any(responseId => responseId == requestId));
                }
            }
        }

        [Category("AzureServiceBus")]
        [Test, TestCaseSource(nameof(GenerateVersionsPairs))]
        public void It_is_possible_to_publish_events(int sourceVersion, int destinationVersion)
        {
            destinationEndpointDefinition.Mappings = new[]
            {
                new MessageMapping
                {
                    MessageType = typeof(TestEvent),
                    TransportAddress = sourceEndpointDefinition.TransportAddressForVersion(sourceVersion)
                }
            };

            using (var source = EndpointFacadeBuilder.CreateAndConfigure(sourceEndpointDefinition, sourceVersion))
            {
                using (var destination = EndpointFacadeBuilder.CreateAndConfigure(destinationEndpointDefinition, destinationVersion))
                {
                    var eventId = Guid.NewGuid();

                    source.PublishEvent(eventId);

                    // ReSharper disable once AccessToDisposedClosure
                    AssertEx.WaitUntilIsTrue(() => destination.ReceivedEventIds.Any(ei => ei == eventId));
                }
            }
        }

        static object[][] GenerateVersionsPairs()
        {
            var asbTransportVersions = new[]
            {
                6,
                7
            };

            var pairs = from l in asbTransportVersions
                from r in asbTransportVersions
                where l != r
                select new object[]
                {
                    l,
                    r
                };

            return pairs.ToArray();
        }
    }
}