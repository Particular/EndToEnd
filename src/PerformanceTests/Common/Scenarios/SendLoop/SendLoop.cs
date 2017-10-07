using System.Threading.Tasks;
using NServiceBus;

abstract class SendLoop : BaseLoop
{

    protected SendLoop()
    {
        IsSendOnly = true;
    }
    protected override Task SendMessage(ISession session)
    {
        return session.SendLocal(new Command { Data = Data });
    }

    class Command : ICommand
    {
        public byte[] Data { get; set; }
    }
}