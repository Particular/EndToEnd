using System;

namespace DataDefinitions
{
    // ReSharper disable once PartialTypeWithSinglePart
    public partial class TestSagaData
    {
        public virtual Guid Id { get; set; }
        public virtual string Originator { get; set; }
        public virtual string OriginalMessageId { get; set; }
    }
}
