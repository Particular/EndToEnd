#if Version5
using NServiceBus.Saga;

partial class SagaCongestionRunner
{
    public class CongestionSaga
        : Saga<SagaCongestionData>
        , IAmStartedByMessages<Command>
    {
        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<SagaCongestionData> mapper)
        {
            mapper.ConfigureMapping<Command>(m => m.Identifier).ToSaga(s => s.UniqueIdentifier);
        }

        public void Handle(Command message)
        {
            if (Shutdown) return;
            Data.UniqueIdentifier = message.Identifier;
            Data.Receives++;
            Bus.SendLocal(message);
        }
    }

    public class SagaCongestionData : ContainSagaData
    {
        [Unique]
        public virtual int UniqueIdentifier { get; set; }
        public virtual long Receives { get; set; }
    }
}
#endif