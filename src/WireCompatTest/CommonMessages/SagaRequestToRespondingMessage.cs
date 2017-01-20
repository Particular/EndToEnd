using System;

namespace CommonMessages
{
    public class SagaRequestToRespondingMessage 
    {
        public string Sender { get; set; }
        public Guid MessageId { get; set; }

        public SagaRequestToRespondingMessage()
        {
            MessageId = Guid.NewGuid();
        }
    }
}