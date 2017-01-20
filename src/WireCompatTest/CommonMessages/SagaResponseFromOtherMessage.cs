using System;

namespace CommonMessages
{
    public class SagaResponseFromOtherMessage
    {
        public string Sender { get; set; }
        public Guid MessageId { get; set; }

        public SagaResponseFromOtherMessage()
        {
            MessageId = Guid.NewGuid();
        }
    }
}