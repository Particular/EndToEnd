#if Version6 || Version7
using NServiceBus.Persistence.Sql;

partial class SagaUpdateRunner
{
    public partial class UpdateSaga
        : SqlSaga<SagaUpdateData>
    {
        protected override string CorrelationPropertyName => nameof(SagaUpdateData.UniqueIdentifier);

        protected override void ConfigureMapping(IMessagePropertyMapper mapper)
        {
            mapper.ConfigureMapping<Command>(m => m.Identifier);
        }
    }
}
#endif