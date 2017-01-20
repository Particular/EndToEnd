
using System;

namespace CommonMessages
{
    public class SagaInitiateRequestingMessage
    {
        public string TargetEndpoint { get; set; }
        public Guid MessageId { get; set; }

        public SagaInitiateRequestingMessage()
        {
            MessageId = Guid.NewGuid();
        }
    }
}