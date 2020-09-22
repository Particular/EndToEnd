namespace TransportCompatibilityTests.AmazonSQS
{
    using System.Linq;
    using NUnit.Framework;
    using Common;
    using Common.AmazonSQS;
    using Common.Messages;

    [TestFixture]
    public class Callbacks: AmazonSqsContext
    {
        AmazonSQSEndpointDefinition sourceEndpointDefinition;
        AmazonSQSEndpointDefinition destinationEndpointDefinition;

        [SetUp]
        public void SetUp()
        {
            sourceEndpointDefinition = new AmazonSQSEndpointDefinition { Name = "Source" };
            destinationEndpointDefinition = new AmazonSQSEndpointDefinition { Name = "Destination" };
        }

        [Category("AmazonSQS")]
        [Test, TestCaseSource(nameof(GenerateVersionsPairs))]
        public void Int_callbacks_work(int sourceVersion, int destinationVersion)
        {
            sourceEndpointDefinition.Mappings = new[]
            {
                new MessageMapping
                {
                    MessageType = typeof(TestIntCallback),
                    TransportAddress = destinationEndpointDefinition.Name
                }
            };

            using (var source = EndpointFacadeBuilder.CreateAndConfigure(sourceEndpointDefinition, sourceVersion))
            using (EndpointFacadeBuilder.CreateAndConfigure(destinationEndpointDefinition, destinationVersion))
            {
                var value = 42;

                source.SendAndCallbackForInt(value);

                AssertEx.WaitUntilIsTrue(() => source.ReceivedIntCallbacks.Contains(value));
            }
        }

        [Category("AmazonSQS")]
        [Test, TestCaseSource(nameof(GenerateVersionsPairs))]
        public void Enum_callbacks_work(int sourceVersion, int destinationVersion)
        {
            sourceEndpointDefinition.Mappings = new[]
            {
                new MessageMapping
                {
                    MessageType = typeof(TestEnumCallback),
                    TransportAddress = destinationEndpointDefinition.Name
                }
            };

            using (var source = EndpointFacadeBuilder.CreateAndConfigure(sourceEndpointDefinition, sourceVersion))
            using (EndpointFacadeBuilder.CreateAndConfigure(destinationEndpointDefinition, destinationVersion))
            {
                var value = CallbackEnum.Three;

                source.SendAndCallbackForEnum(value);

                AssertEx.WaitUntilIsTrue(() => source.ReceivedEnumCallbacks.Contains(value));
            }
        }

        static object[][] GenerateVersionsPairs()
        {
            var versions = new[]
            {
                3,
                4
            };

            var pairs = from l in versions
                        from r in versions
                        where l != r
                        select new object[] { l, r };

            return pairs.ToArray();
        }
    }
}
