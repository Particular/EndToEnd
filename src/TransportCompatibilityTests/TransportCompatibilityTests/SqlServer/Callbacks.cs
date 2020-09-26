namespace TransportCompatibilityTests.SqlServer
{
    using System.Linq;
    using NUnit.Framework;
    using Common;
    using Common.Messages;
    using Common.SqlServer;

    [TestFixture]
    public class Callbacks: SqlServerContext
    {
        SqlServerEndpointDefinition sourceEndpointDefinition;
        SqlServerEndpointDefinition destinationEndpointDefinition;

        [SetUp]
        public void SetUp()
        {
            sourceEndpointDefinition = new SqlServerEndpointDefinition { Name = "Source" };
            destinationEndpointDefinition = new SqlServerEndpointDefinition { Name = "Destination" };
        }

        [Category("SqlServer")]
        [Test, TestCaseSource(nameof(GenerateVersionsPairs))]
        public void Int_callbacks_work(int sourceVersion, int destinationVersion)
        {
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

                AssertEx.WaitUntilIsTrue(() => source.ReceivedIntCallbacks.Contains(value));
            }
        }

        [Category("SqlServer")]
        [Test, TestCaseSource(nameof(GenerateVersionsPairs))]
        public void Enum_callbacks_work(int sourceVersion, int destinationVersion)
        {
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

                AssertEx.WaitUntilIsTrue(() => source.ReceivedEnumCallbacks.Contains(value));
            }
        }

        static object[][] GenerateVersionsPairs()
        {
            var sqlTransportVersions = new[] { 1, 2, 3 };

            var pairs = from l in sqlTransportVersions
                        from r in sqlTransportVersions
                        where l != r
                        select new object[] { l, r };

            return pairs.ToArray();
        }
    }
}
