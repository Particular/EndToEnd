#if Version6 || Version7
using System;
using System.Threading.Tasks;
using NServiceBus;

partial class SagaInitiateRunner
{
    public partial class CreateSaga
        : IAmStartedByMessages<Command>
    {
        public Task Handle(Command message, IMessageHandlerContext context)
        {
            Data.Identifier = message.Identifier;
            return Task.FromResult(0);
        }
    }

    public class SagaCreateData : ContainSagaData
    {
        public virtual Guid Identifier { get; set; }
    }
#if !CustomSaga
    partial class CreateSaga
        : Saga<SagaCreateData>
    {
        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<SagaCreateData> mapper)
        {
            mapper.ConfigureMapping<Command>(m => m.Identifier).ToSaga(s => s.Identifier);
        }
    }
#endif
}
#endif