#if Version5
using NServiceBus.Logging;
using NServiceBus.Saga;

partial class SagaUpdateRunner
{
    public class UpdateSaga
        : Saga<SagaUpdateData>
        , IAmStartedByMessages<Command>
    {
        readonly ILog Log = LogManager.GetLogger(nameof(UpdateSaga));

        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<SagaUpdateData> mapper)
        {
            mapper.ConfigureMapping<Command>(m => m.Identifier).ToSaga(s => s.UniqueIdentifier);
        }

        public void Handle(Command message)
        {
            if (Shutdown)
            {
                Log.InfoFormat("Skip processing, shutting down...");
                return;
            }
            Data.UniqueIdentifier = message.Identifier;
            Data.Receives++;
            Bus.SendLocal(message);
        }
    }

    public class SagaUpdateData : ContainSagaData
    {
        [Unique]
        public virtual int UniqueIdentifier { get; set; }
        public virtual long Receives { get; set; }
    }
}
#endif