using CommonMessages;
using NServiceBus;
using NServiceBus.Saga;

namespace Common.Saga
{
#if(Version3)

    public class RequestingSaga : Saga<RequestingSagaData>,
        IAmStartedByMessages<SagaInitiateRequestingMessage>,
        IHandleMessages<SagaResponseFromOtherMessage>
    {
        public void Handle(SagaInitiateRequestingMessage message)
        {
            Data.MessageId = message.MessageId;
            var newMessage = new SagaRequestToRespondingMessage
                {
                    Sender = TestRunner.EndpointName
                };
            Bus.Send(message.TargetEndpoint, newMessage);
        }

        public void Handle(SagaResponseFromOtherMessage message)
        {
            SagaVerifier.RequestingSagaGotTheResponse.Add(message.Sender);
            MarkAsComplete();
        }

        public override void ConfigureHowToFindSaga()
        {
            ConfigureMapping<SagaInitiateRequestingMessage>(
                sagaData => sagaData.MessageId,
                message => message.MessageId);
        }
    }

#endif
#if(Version4)

    public class RequestingSaga : Saga<RequestingSagaData>,
        IAmStartedByMessages<SagaInitiateRequestingMessage>,
        IHandleMessages<SagaResponseFromOtherMessage>
    {
        public void Handle(SagaInitiateRequestingMessage message)
        {
            Data.MessageId = message.MessageId;
            var newMessage = new SagaRequestToRespondingMessage
                {
                    Sender = TestRunner.EndpointName
                };
            Bus.Send(message.TargetEndpoint, newMessage);
        }

        public void Handle(SagaResponseFromOtherMessage message)
        {
            SagaVerifier.RequestingSagaGotTheResponse.Add(message.Sender);
            MarkAsComplete();
        }

        public override void ConfigureHowToFindSaga()
        {
           ConfigureMapping<SagaInitiateRequestingMessage>(message => message.MessageId)
                .ToSaga(data => data.MessageId);
        }
    }

#endif
#if(Version5)

    public class RequestingSaga : Saga<RequestingSagaData>,
        IAmStartedByMessages<SagaInitiateRequestingMessage>,
        IHandleMessages<SagaResponseFromOtherMessage>
    {
        public void Handle(SagaInitiateRequestingMessage message)
        {
            Data.MessageId = message.MessageId;
            var newMessage = new SagaRequestToRespondingMessage
            {
                Sender = TestRunner.EndpointName
            };
            Bus.Send(message.TargetEndpoint, newMessage);
        }

        public void Handle(SagaResponseFromOtherMessage message)
        {
            SagaVerifier.RequestingSagaGotTheResponse.Add(message.Sender);
            MarkAsComplete();
        }

        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<RequestingSagaData> mapper)
        {
            mapper.ConfigureMapping<SagaInitiateRequestingMessage>(message => message.MessageId)
                .ToSaga(data => data.MessageId);
        }
    }

#endif
}