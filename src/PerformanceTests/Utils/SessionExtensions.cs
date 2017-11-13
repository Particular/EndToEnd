using System;
using System.Threading.Tasks;
using log4net;

public static class SessionExtensions
{
    static ILog Log = LogManager.GetLogger(typeof(SessionExtensions));

    public static async Task CloseWithSuppress(this ISession instance)
    {
        try
        {
            await instance.Close().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Log.Warn($"CloseWithSuppress ({ex.GetType()}): {ex.Message.Replace(Environment.NewLine, "; ")}", ex);
        }
    }
}