namespace TransportCompatibilityTests.AmazonSQS
{
    using System;
    using System.Linq;
    using Common;
    using Common.AmazonSQS;
    using Common.Messages;
    using NUnit.Framework;

    [TestFixture]
    public class MessageExchangePatterns : AmazonSqsContext
    {
        [SetUp]
        public void SetUp()
        {
            sourceEndpointDefinition = new AmazonSQSEndpointDefinition { Name = "Source" };
            destinationEndpointDefinition = new AmazonSQSEndpointDefinition { Name = "Destination" };
        }

        [Category("AmazonSQS")]
        [Test, TestCaseSource(nameof(GenerateVersionsPairs))]
        public void It_is_possible_to_send_command_between_different_versions(int sourceVersion, int destinationVersion)
        {
            sourceEndpointDefinition.Mappings = new[]
            {
                new MessageMapping
                {
                    MessageType = typeof(TestCommand),
                    TransportAddress = destinationEndpointDefinition.Name
                }
            };

            using (var source = EndpointFacadeBuilder.CreateAndConfigure(sourceEndpointDefinition, sourceVersion))
            using (var destination = EndpointFacadeBuilder.CreateAndConfigure(destinationEndpointDefinition, destinationVersion))
            {
                var messageId = Guid.NewGuid();

                source.SendCommand(messageId);

                AssertEx.WaitUntilIsTrue(() => destination.ReceivedMessageIds.Any(mi => mi == messageId));
            }
        }

        [Category("AmazonSQS")]
        [Test, TestCaseSource(nameof(GenerateVersionsPairs))]
        public void It_is_possible_to_send_request_and_receive_reply(int sourceVersion, int destinationVersion)
        {
            sourceEndpointDefinition.Mappings = new[]
            {
                new MessageMapping
                {
                    MessageType = typeof(TestRequest),
                    TransportAddress = sourceEndpointDefinition.Name
                }
            };

            using (var source = EndpointFacadeBuilder.CreateAndConfigure(sourceEndpointDefinition, sourceVersion))
            using (EndpointFacadeBuilder.CreateAndConfigure(destinationEndpointDefinition, destinationVersion))
            {
                var requestId = Guid.NewGuid();

                source.SendRequest(requestId);

                AssertEx.WaitUntilIsTrue(() => source.ReceivedResponseIds.Any(responseId => responseId == requestId));
            }
        }

        [Category("AmazonSQS")]
        [Test, TestCaseSource(nameof(GenerateVersionsPairs))]
        public void It_is_possible_to_publish_events(int sourceVersion, int destinationVersion)
        {
            destinationEndpointDefinition.Publishers = new[]
            {
                new MessageMapping
                {
                    MessageType = typeof(TestEvent),
                    TransportAddress = sourceEndpointDefinition.Name
                }
            };

            using (var source = EndpointFacadeBuilder.CreateAndConfigure(sourceEndpointDefinition, sourceVersion))
            using (var destination = EndpointFacadeBuilder.CreateAndConfigure(destinationEndpointDefinition, destinationVersion))
            {
                AssertEx.WaitUntilIsTrue(() => source.NumberOfSubscriptions > 0);

                var eventId = Guid.NewGuid();

                source.PublishEvent(eventId);

                AssertEx.WaitUntilIsTrue(() => destination.ReceivedEventIds.Any(ei => ei == eventId));
            }
        }

        static object[][] GenerateVersionsPairs()
        {
            var transportVersions = new[]
            {
                3,
                4
            };

            var pairs = from l in transportVersions
                from r in transportVersions
                where l != r
                select new object[]
                {
                    l,
                    r
                };

            return pairs.ToArray();
        }

        AmazonSQSEndpointDefinition sourceEndpointDefinition;
        AmazonSQSEndpointDefinition destinationEndpointDefinition;
    }
}