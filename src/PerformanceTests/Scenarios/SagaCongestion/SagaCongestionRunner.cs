using System.Threading.Tasks;
using NServiceBus;

/// <summary>
/// Seeds a set of message greater then the allowed max concurrency but share
/// the same saga instance identifier. This results in single item congestion.
/// Persistence configurations that support pessimistic locking should not
/// have any issues with this.
/// 
/// Part of the processing operation is sending the same messages to itself
/// to keep this running until the test stops
/// </summary>
partial class SagaCongestionRunner
    : PerpetualRunner
{
    protected override Task Seed(int i, ISession session)
    {
        return session.SendLocal(new Command { Identifier = 1, Data = Data });
    }

    public class Command : ICommand
    {
        public int Identifier { get; set; }
        public byte[] Data { get; set; }
    }
}
