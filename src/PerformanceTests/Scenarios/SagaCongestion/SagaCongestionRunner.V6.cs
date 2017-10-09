#if Version6 || Version7
using System.Threading.Tasks;
using NServiceBus;

partial class SagaCongestionRunner
{
    public partial class SagaCongestion
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

    public class SagaCongestionData : ContainSagaData
    {
        public virtual int UniqueIdentifier { get; set; }
        public virtual long Receives { get; set; }
    }

#if !CustomSaga
    public partial class SagaCongestion
        : Saga<SagaCongestionData>
    {
        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<SagaCongestionData> mapper)
        {
            mapper.ConfigureMapping<Command>(m => m.Identifier).ToSaga(s => s.UniqueIdentifier);
        }
    }
#endif
}
#endif