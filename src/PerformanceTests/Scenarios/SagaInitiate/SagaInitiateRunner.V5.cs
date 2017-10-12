#if Version5
using System;
using NServiceBus.Saga;

partial class SagaInitiateRunner
{
    public class CreateSaga : Saga<SagaCreateData>,
        IAmStartedByMessages<Command>
    {
        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<SagaCreateData> mapper)
        {
            mapper.ConfigureMapping<Command>(m => m.Identifier).ToSaga(s => s.Identifier);
        }

        public void Handle(Command message)
        {
            Data.Identifier = message.Identifier;
        }
    }

    public class SagaCreateData : ContainSagaData
    {
        [Unique]
        public virtual Guid Identifier { get; set; }
    }
}
#endif