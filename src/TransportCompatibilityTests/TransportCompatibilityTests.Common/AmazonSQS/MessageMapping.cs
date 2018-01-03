namespace TransportCompatibilityTests.Common.AmazonSQS
{
    using System;

    [Serializable]
    public class MessageMapping
    {
        public Type MessageType { get; set; }
        public string TransportAddress { get; set; }
    }
}