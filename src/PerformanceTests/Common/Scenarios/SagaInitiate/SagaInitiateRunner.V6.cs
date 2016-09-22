#if Version6
using System;
using System.Threading.Tasks;
using NServiceBus;

partial class SagaInitiateRunner
{
    public class TheSaga : Saga<SagaCreateData>,
        IAmStartedByMessages<Command>
    {
        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<SagaCreateData> mapper)
        {
            mapper.ConfigureMapping<Command>(m => m.Identifier).ToSaga(s => s.Identifier);
        }

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
}
#endif