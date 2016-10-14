using System;
using System.Threading.Tasks;
using NLog;

public static class SessionExtensions
{
    static Logger Log = LogManager.GetLogger(nameof(SessionExtensions));

    public static async Task CloseWithSuppress(this ISession instance)
    {
        try
        {
            await instance.Close().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Log.Warn(ex, "CloseWithSuppress ({0}): {1}", ex.GetType(), ex.Message.Replace(Environment.NewLine,"; "));
        }
    }
}