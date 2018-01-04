namespace TransportCompatibilityTests.Common.AmazonSQS
{
    using System;

    [Serializable]
    public class AmazonSQSEndpointDefinition : EndpointDefinition
    {
        public override string TransportName => "AmazonSQS";
        public MessageMapping[] Mappings { get; set; }
        public MessageMapping[] Publishers { get; set; }

        public AmazonSQSEndpointDefinition()
        {
            Mappings = new MessageMapping[0];
            Publishers = new MessageMapping[0];
        }
    }
}