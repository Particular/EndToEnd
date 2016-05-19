using System;
using System.Collections.Generic;
using System.Text;

namespace DataDefinitions
{
    public partial class TestSagaDataWithComposite 
    {
        public virtual Guid Id { get; set; }
        public virtual string Originator { get; set; }
        public virtual string OriginalMessageId { get; set; }
        public virtual SagaComposite Composite { get; set; }

        public class SagaComposite
        {
            public virtual string Value { get; set; }
        }
    }
}
