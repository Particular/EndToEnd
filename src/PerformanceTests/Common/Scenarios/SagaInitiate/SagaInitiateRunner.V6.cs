#if Version6
using System.Threading.Tasks;
using NServiceBus;

partial class SagaInitiateRunner
{
    public class TheSaga : Saga<SagaData>,
        IAmStartedByMessages<Command>
    {
        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<SagaData> mapper)
        {
        }

        public Task Handle(Command message, IMessageHandlerContext context)
        {
            Data.UniqueIdentifier = message.Identifier;
            return Task.CompletedTask;
        }
    }

    public class SagaData : ContainSagaData
    {
        public virtual int UniqueIdentifier { get; set; }
    }
}
#endif