using NServiceBus;

namespace DataDefinitions
{
    public partial class TestSagaDataWithComposite : IContainSagaData
    {
        public virtual string CorrelationProperty { get; set; }
    }
}