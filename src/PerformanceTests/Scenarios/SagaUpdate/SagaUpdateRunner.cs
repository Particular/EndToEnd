using System.Threading.Tasks;
using NServiceBus;

partial class SagaUpdateRunner
    : PerpetualRunner
{
    protected override Task Seed(int i, ISession session)
    {
        return session.SendLocal(new Command { Identifier = ++i, Data = Data });
    }

    public class Command : ICommand
    {
        public int Identifier { get; set; }
        public byte[] Data { get; set; }
    }
}
