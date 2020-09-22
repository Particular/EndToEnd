namespace TransportCompatibilityTests.SqlServer
{
    using System;
    using System.Linq;
    using NUnit.Framework;
    using Common;
    using Common.Messages;
    using Common.SqlServer;

    [TestFixture]
    public class UpgradingV2SubscriberToV3SubscriberWithV3Publisher : SqlServerContext
    {
        SqlServerEndpointDefinition subscriberDefinition;
        SqlServerEndpointDefinition publisherDefinition;

        [SetUp]
        public void SetUp()
        {
            subscriberDefinition = new SqlServerEndpointDefinition
            {
                Name = "Subscriber"
            };
            publisherDefinition = new SqlServerEndpointDefinition
            {
                Name = "Publisher"
            };
        }

        [Category("SqlServer")]
        [Test]
        public void Subscriber_doesnt_receive_duplicate_events()
        {
            subscriberDefinition.Publishers = new[]
            {
                new MessageMapping
                {
                    MessageType = typeof(TestEvent),
                    TransportAddress = publisherDefinition.TransportAddressForVersion(3)
                }
            };

            using (var publisher = EndpointFacadeBuilder.CreateAndConfigure(publisherDefinition, 3))
            using (var subscriber = EndpointFacadeBuilder.CreateAndConfigure(subscriberDefinition, 2))
            {
                AssertEx.WaitUntilIsTrue(() => publisher.NumberOfSubscriptions > 0);

                var eventId = Guid.NewGuid();

                publisher.PublishEvent(eventId);

                AssertEx.WaitUntilIsTrue(() => subscriber.ReceivedEventIds.Any(ei => ei == eventId));
                Assert.AreEqual(1, subscriber.ReceivedEventIds.Length);
            }

            // Let's upgrade

            subscriberDefinition.Publishers = new[]
            {
                new MessageMapping
                {
                    MessageType = typeof(TestEvent),
                    TransportAddress = publisherDefinition.TransportAddressForVersion(3)
                }
            };

            using (var publisher = EndpointFacadeBuilder.CreateAndConfigure(publisherDefinition, 3))
            using (var subscriber = EndpointFacadeBuilder.CreateAndConfigure(subscriberDefinition, 3))
            {
                AssertEx.WaitUntilIsTrue(() => publisher.NumberOfSubscriptions > 0);

                var eventId = Guid.NewGuid();

                publisher.PublishEvent(eventId);

                // Wait for the fiest message
                AssertEx.WaitUntilIsTrue(() => subscriber.ReceivedEventIds.Length > 0);

                // Wait 5s for another one (should not come)
                Assert.False(AssertEx.TryWaitUntilIsTrue(() => subscriber.ReceivedEventIds.Length > 1, TimeSpan.FromSeconds(5)));

                Assert.IsFalse(subscriber.ReceivedEventIds.Length == 2 && subscriber.ReceivedEventIds.All(k => k == eventId), "Received duplicated message!");
            }
        }
    }
}