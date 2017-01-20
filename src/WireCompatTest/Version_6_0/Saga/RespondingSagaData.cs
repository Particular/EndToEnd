using System;
using NServiceBus;

namespace Common.Saga
{
    public class RespondingSagaData : ContainSagaData
    {
        public Guid MessageId { get; set; }
    }
}