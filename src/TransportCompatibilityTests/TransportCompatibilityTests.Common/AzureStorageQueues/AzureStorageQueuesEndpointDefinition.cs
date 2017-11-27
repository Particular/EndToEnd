namespace TransportCompatibilityTests.Common.AzureStorageQueues
{
    using System;

    [Serializable]
    public class AzureStorageQueuesEndpointDefinition : EndpointDefinition
    {
        public override string TransportName => "AzureStorageQueues";

        public MessageMapping[] Mappings { get; set; }
        public MessageMapping[] Publishers { get; set; }

        public AzureStorageQueuesEndpointDefinition()
        {
            Mappings = new MessageMapping[0];
            Publishers = new MessageMapping[0];
        }
    }
}
