namespace Common.Saga
{
    // ReSharper disable RedundantUsingDirective
    using System;
    using NServiceBus;
#if !Version6
    using NServiceBus.Saga;
#endif

    public class SagaData : IContainSagaData
    {
        public string Originator { get; set; }

        public string OriginalMessageId { get; set; }

        public Guid Id { get; set; }

#if !Version6
        [Unique]
#endif
        public int Number { get; set; }

        public int NumCalls { get; set; }
    }
}
