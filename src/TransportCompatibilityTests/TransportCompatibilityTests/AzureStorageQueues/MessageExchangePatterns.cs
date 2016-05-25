namespace TransportCompatibilityTests.AzureStorageQueues
{
    using System;
    using System.Linq;
    using NUnit.Framework;
    using Common;
    using Common.AzureStorageQueues;
    using Common.Messages;

    [TestFixture]
    public class MessageExchangePatterns : AzureStorageQueuesContext
    {
        AzureStorageQueuesEndpointDefinition sourceEndpointDefinition;
        AzureStorageQueuesEndpointDefinition destinationEndpointDefinition;

        [SetUp]
        public void SetUp()
        {
            sourceEndpointDefinition = new AzureStorageQueuesEndpointDefinition { Name = "Source" };
            destinationEndpointDefinition = new AzureStorageQueuesEndpointDefinition { Name = "Destination" };
        }

        [Category("AzureStorageQueues")]
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

                // ReSharper disable once AccessToDisposedClosure
                AssertEx.WaitUntilIsTrue(() => destination.ReceivedMessageIds.Any(mi => mi == messageId));
            }
        }


        [Category("AzureStorageQueues")]
        [Test, TestCaseSource(nameof(GenerateVersionsPairs))]
        public void It_is_possible_to_send_request_and_receive_replay(int sourceVersion, int destinationVersion)
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

                // ReSharper disable once AccessToDisposedClosure
                AssertEx.WaitUntilIsTrue(() => source.ReceivedResponseIds.Any(responseId => responseId == requestId));
            }
        }

        [Category("AzureStorageQueues")]
        [Test, TestCaseSource(nameof(GenerateVersionsPairs))]
        public void It_is_possible_to_publish_events(int sourceVersion, int destinationVersion)
        {
            destinationEndpointDefinition.Mappings = new[]
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
                // ReSharper disable once AccessToDisposedClosure
                AssertEx.WaitUntilIsTrue(() => source.NumberOfSubscriptions > 0);

                var eventId = Guid.NewGuid();

                source.PublishEvent(eventId);

                // ReSharper disable once AccessToDisposedClosure
                AssertEx.WaitUntilIsTrue(() => destination.ReceivedEventIds.Any(ei => ei == eventId));
            }
        }


        static object[][] GenerateVersionsPairs()
        {
            var transportVersions = new[]
            {
                6,
                7
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
    }
}
