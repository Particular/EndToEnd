using System.Threading.Tasks;
using DataDefinitions;
using NServiceBus;

namespace Version_7_0
{
    class TestSaga : Saga<TestSagaData>, IAmStartedByMessages<TMsg>
    {
        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<TestSagaData> mapper)
        {
            mapper.ConfigureMapping<TMsg>(msg => msg.CorrelationProperty).ToSaga(d => d.CorrelationProperty);
        }

        public Task Handle(TMsg message, IMessageHandlerContext context)
        {
            return Task.FromResult(0);
        }
    }

    class TestSagaWithList : Saga<TestSagaDataWithList>, IAmStartedByMessages<TMsg>
    {
        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<TestSagaDataWithList> mapper)
        {
            mapper.ConfigureMapping<TMsg>(msg => msg.CorrelationProperty).ToSaga(d => d.CorrelationProperty);
        }

        public Task Handle(TMsg message, IMessageHandlerContext context)
        {
            return Task.FromResult(0);
        }
    }

    class TestSagaWithComposite : Saga<TestSagaDataWithComposite>, IAmStartedByMessages<TMsg>
    {
        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<TestSagaDataWithComposite> mapper)
        {
            mapper.ConfigureMapping<TMsg>(msg => msg.CorrelationProperty).ToSaga(d => d.CorrelationProperty);
        }

        public Task Handle(TMsg message, IMessageHandlerContext context)
        {
            return Task.FromResult(0);
        }
    }
    public class TMsg: IMessage
    {
        public string CorrelationProperty { get; set; }
    }
}
