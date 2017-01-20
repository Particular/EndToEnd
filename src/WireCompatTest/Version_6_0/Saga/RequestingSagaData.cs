using System;
using NServiceBus;

namespace Common.Saga
{
    public class RequestingSagaData : ContainSagaData
    {
        public Guid MessageId { get; set; }
    }
}