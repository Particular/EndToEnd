using System;
using System.Collections.Generic;
using System.Text;

namespace DataDefinitions
{
    public partial class TestSagaData
    {
        public virtual Guid Id { get; set; }
        public virtual string Originator { get; set; }
        public virtual string OriginalMessageId { get; set; }
    }
}
