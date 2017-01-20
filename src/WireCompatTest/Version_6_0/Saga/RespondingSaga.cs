using System.Threading.Tasks;
using CommonMessages;
using NServiceBus;

namespace Common.Saga
{
    public class RespondingSaga : Saga<RespondingSagaData>,
        IAmStartedByMessages<SagaRequestToRespondingMessage>
    {
        public Task Handle(SagaRequestToRespondingMessage message, IMessageHandlerContext context)
        {
            Data.MessageId = message.MessageId;
            var response = new SagaResponseFromOtherMessage
            {
                Sender = TestRunner.EndpointName
            };
            return context.Reply(response);
        }

        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<RespondingSagaData> mapper)
        {
            mapper.ConfigureMapping<SagaRequestToRespondingMessage>(message => message.MessageId)
                .ToSaga(data => data.MessageId);
        }
    }
}