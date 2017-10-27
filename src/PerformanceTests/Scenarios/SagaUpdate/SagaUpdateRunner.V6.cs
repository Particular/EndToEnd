#if Version6 || Version7
using System.Threading.Tasks;
using NServiceBus;

partial class SagaUpdateRunner
{
    public partial class UpdateSaga
        : IAmStartedByMessages<Command>
    {
        public Task Handle(Command message, IMessageHandlerContext context)
        {
            if (Shutdown) return Task.FromResult(0);
            Data.UniqueIdentifier = message.Identifier;
            Data.Receives++;
            return context.SendLocal(message);
        }
    }

    public class SagaUpdateData : ContainSagaData
    {
        public virtual int UniqueIdentifier { get; set; }
        public virtual long Receives { get; set; }
    }
#if !CustomSaga
    partial class UpdateSaga
        : Saga<SagaUpdateData>
    {
        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<SagaUpdateData> mapper)
        {
            mapper.ConfigureMapping<Command>(m => m.Identifier).ToSaga(s => s.UniqueIdentifier);
        }
    }
#endif
}
#endif