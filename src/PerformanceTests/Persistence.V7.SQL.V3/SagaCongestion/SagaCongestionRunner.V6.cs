using NServiceBus.Persistence.Sql;

partial class SagaCongestionRunner
{
    public partial class SagaCongestion
        : SqlSaga<SagaCongestionData>
    {
        protected override string CorrelationPropertyName => nameof(SagaCongestionData.UniqueIdentifier);

        protected override void ConfigureMapping(IMessagePropertyMapper mapper)
        {
            mapper.ConfigureMapping<Command>(m => m.Identifier);
        }
    }
}
