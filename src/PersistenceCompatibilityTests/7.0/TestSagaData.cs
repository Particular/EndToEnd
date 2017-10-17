using NServiceBus;

namespace DataDefinitions
{
    public partial class TestSagaData : IContainSagaData
    {
        public virtual string CorrelationProperty { get; set; }
    }
}