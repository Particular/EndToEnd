namespace Common.Saga
{
    using NServiceBus;
#if !Version6
    using NServiceBus.Saga;
#endif

    class TestSaga : Saga<SagaData>, IAmStartedByMessages<StartSagaMessage>, IHandleMessages<CompleteSagaMessage>
    {
        public void Handle(StartSagaMessage message)
        {
            Data.Number = message.Id;
            Data.NumCalls++;
        }

        public void Handle(CompleteSagaMessage message)
        {
            MarkAsComplete();
        }

#if Version3 || Version4
        public override void ConfigureHowToFindSaga()
#else
        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<SagaData> mapper)
#endif
        {
#if Version3
            ConfigureMapping<StartSagaMessage>(s => s.Number, m => m.Id);
            ConfigureMapping<CompleteSagaMessage>(sagaData => sagaData.Number, message => message.Id);
#elif Version4
            ConfigureMapping<StartSagaMessage>(m => m.Id).ToSaga(s => s.Number);
            ConfigureMapping<CompleteSagaMessage>(m => m.Id).ToSaga(s => s.Number);
#else
            mapper.ConfigureMapping<StartSagaMessage>(m => m.Id).ToSaga(s => s.Number);
            mapper.ConfigureMapping<CompleteSagaMessage>(m => m.Id).ToSaga(s => s.Number);
#endif
        }

#if Version6
        public System.Threading.Tasks.Task Handle(StartSagaMessage message, IMessageHandlerContext context)
        {
            Handle(message);

            return System.Threading.Tasks.Task.FromResult(0);
        }

        public System.Threading.Tasks.Task Handle(CompleteSagaMessage message, IMessageHandlerContext context)
        {
            Handle(message);

            return System.Threading.Tasks.Task.FromResult(0);
        }
#endif
    }
}
