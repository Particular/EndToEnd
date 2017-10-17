using NServiceBus;

namespace DataDefinitions
{
    public partial class TestSagaDataWithList : IContainSagaData
    {
        public virtual string CorrelationProperty { get; set; }
    }
}
