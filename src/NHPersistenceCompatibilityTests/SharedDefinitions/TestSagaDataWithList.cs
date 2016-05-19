using System;
using System.Collections.Generic;

namespace DataDefinitions
{
    // ReSharper disable once PartialTypeWithSinglePart
    public partial class TestSagaDataWithList
    {
        public virtual Guid Id { get; set; }
        public virtual string Originator { get; set; }
        public virtual string OriginalMessageId { get; set; }
        public virtual IList<int> Ints { get; set; }
    }
}
