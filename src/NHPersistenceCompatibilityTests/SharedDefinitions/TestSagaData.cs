using System;

namespace DataDefinitions
{
    // ReSharper disable once PartialTypeWithSinglePart
    public partial class TestSagaData
    {
        public virtual Guid Id { get; set; }
#if VERSION_4_5 || VERSION_5_0 || VERSION_6_2
        [NServiceBus.Saga.Unique]
#endif
        public virtual string Originator { get; set; }
        public virtual string OriginalMessageId { get; set; }
        public virtual string SomeValue { get; set; }
    }
}
