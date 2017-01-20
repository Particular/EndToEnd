using System.Threading.Tasks;
using CommonMessages;
using NServiceBus;

namespace Common.Saga
{

    public class RequestingSaga : Saga<RequestingSagaData>,
        IAmStartedByMessages<SagaInitiateRequestingMessage>,
        IHandleMessages<SagaResponseFromOtherMessage>
    {
        public Task Handle(SagaInitiateRequestingMessage message, IMessageHandlerContext context)
        {
            Data.MessageId = message.MessageId;
            var newMessage = new SagaRequestToRespondingMessage
            {
                Sender = TestRunner.EndpointName
            };
            return context.Send(message.TargetEndpoint, newMessage);
        }

        public Task Handle(SagaResponseFromOtherMessage message, IMessageHandlerContext context)
        {
            SagaVerifier.RequestingSagaGotTheResponse.Add(message.Sender);
            MarkAsComplete();

            return Task.FromResult(0);
        }

        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<RequestingSagaData> mapper)
        {
            mapper.ConfigureMapping<SagaInitiateRequestingMessage>(message => message.MessageId)
                .ToSaga(data => data.MessageId);
        }
    }
}