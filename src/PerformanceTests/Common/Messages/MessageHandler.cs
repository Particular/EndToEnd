using NServiceBus;

namespace Common.Messages
{
    using System;
    using System.Transactions;
    using Common.Encryption;

    public class MessageHandler : IHandleMessages<TestMessage>, IHandleMessages<EncryptionTestMessage>
    {
        private static TwoPhaseCommitEnlistment enlistment = new TwoPhaseCommitEnlistment();

#if Version6
        public System.Threading.Tasks.Task Handle(TestMessage message, IMessageHandlerContext context)
        {
            Handle(message);

            return System.Threading.Tasks.Task.FromResult(0);
        }

        public System.Threading.Tasks.Task Handle(EncryptionTestMessage message, IMessageHandlerContext context)
        {
            Handle(message);

            return System.Threading.Tasks.Task.FromResult(0);
        }
#endif

        public void Handle(TestMessage message)
        {
        }

        public void Handle(EncryptionTestMessage message)
        {
            if (message.TwoPhaseCommit)
            {
                Transaction.Current.EnlistDurable(Guid.NewGuid(), enlistment, EnlistmentOptions.None);
            }
        }
    }
}
