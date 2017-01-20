using CommonMessages;
using NServiceBus.Saga;

namespace Common.Saga
{
#if(Version3)

    public class RespondingSaga : Saga<RespondingSagaData>,
        IAmStartedByMessages<SagaRequestToRespondingMessage>
    {
        public void Handle(SagaRequestToRespondingMessage message)
        {
            Data.MessageId = message.MessageId;
            var response = new SagaResponseFromOtherMessage
            {
                Sender = TestRunner.EndpointName
            };
            Bus.Reply(response);
        }

        public override void ConfigureHowToFindSaga()
        {
            ConfigureMapping<SagaResponseFromOtherMessage>(
                data => data.MessageId,
                message => message.MessageId);
        }
    }

#endif

#if(Version4)

    public class RespondingSaga : Saga<RespondingSagaData>,
        IAmStartedByMessages<SagaRequestToRespondingMessage>
    {
        public void Handle(SagaRequestToRespondingMessage message)
        {
            Data.MessageId = message.MessageId;
            var response = new SagaResponseFromOtherMessage
            {
                Sender = TestRunner.EndpointName
            };
            Bus.Reply(response);
        }

        public override void ConfigureHowToFindSaga()
        {
            ConfigureMapping<SagaResponseFromOtherMessage>(message => message.MessageId)
                .ToSaga(sagaData => sagaData.MessageId);
        }
    }

#endif

#if (Version5)

    public class RespondingSaga : Saga<RespondingSagaData>,
        IAmStartedByMessages<SagaRequestToRespondingMessage>
    {
        public void Handle(SagaRequestToRespondingMessage message)
        {
            Data.MessageId = message.MessageId;
            var response = new SagaResponseFromOtherMessage
            {
                Sender = TestRunner.EndpointName
            };
            Bus.Reply(response);
        }

        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<RespondingSagaData> mapper)
        {
            mapper.ConfigureMapping<SagaInitiateRequestingMessage>(message => message.MessageId)
                 .ToSaga(data => data.MessageId);
        }
    }

#endif
}