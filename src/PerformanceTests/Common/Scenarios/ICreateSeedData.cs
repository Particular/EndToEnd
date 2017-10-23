using System.Threading.Tasks;

public interface ICreateSeedData
{
    /// <summary>
    /// Sends or publishes a single message
    /// </summary>
    Task SendMessage(ISession session);
}
