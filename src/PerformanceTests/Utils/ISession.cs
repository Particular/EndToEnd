using System.Threading.Tasks;

public interface ISession
{
    Task Send(object message);
    Task Send(string destination, object message);
    Task Publish(object message);
    Task Close();
}
