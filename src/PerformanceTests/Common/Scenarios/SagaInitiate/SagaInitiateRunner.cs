using System;
using System.Threading.Tasks;
using Common.Scenarios;
using NServiceBus;

partial class SagaInitiateRunner : BaseRunner, ICreateSeedData
{
    public Task SendMessage(ISession session)
    {
        return session.SendLocal(new Command { Identifier = Guid.NewGuid() });
    }

    public class Command : ICommand
    {
        public Guid Identifier { get; set; }
    }
}
