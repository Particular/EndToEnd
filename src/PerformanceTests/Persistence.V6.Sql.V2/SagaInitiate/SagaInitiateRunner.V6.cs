#if Version6 || Version7
using NServiceBus.Persistence.Sql;

partial class SagaInitiateRunner
{
    public partial class CreateSaga
        : SqlSaga<SagaCreateData>
    {
        protected override string CorrelationPropertyName => nameof(SagaCreateData.Identifier);

        protected override void ConfigureMapping(IMessagePropertyMapper mapper)
        {
            mapper.ConfigureMapping<Command>(m => m.Identifier);
        }
    }
}
#endif