using System;
using System.Runtime.InteropServices;
using System.Threading;
using log4net;

public class Powerplan
{
    static ILog Log = LogManager.GetLogger(typeof(Powerplan));

    public static void CheckPowerPlan()
    {
        if (Environment.OSVersion.Platform != PlatformID.Win32NT)
        {
            //only supported on windows
            return;
        }

        try
        {
            var highperformance = new Guid("8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c");
            var id = Powerplan.GetActive();

            Log.InfoFormat("Powerplan: {0}", id);

            if (id != highperformance)
            {
                Log.WarnFormat("Power option not set to High Performance, consider setting it to high performance!");
                Thread.Sleep(3000);
            }
        }
        catch (Exception ex)
        {
            Log.Debug("Powerplan check failed, ignoring", ex);
        }
    }

    static Guid GetActive()
    {
        var ActiveScheme = Guid.Empty;
        var ptr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(IntPtr)));
        if (PowerGetActiveScheme((IntPtr)null, ref ptr) != 0) return ActiveScheme;
        ActiveScheme = (Guid)Marshal.PtrToStructure(ptr, typeof(Guid));
        if (ptr != IntPtr.Zero) Marshal.FreeHGlobal(ptr);
        return ActiveScheme;
    }

#pragma warning disable PC003
    [DllImport("powrprof.dll")]
    static extern UInt32 PowerGetActiveScheme(IntPtr UserRootPowerKey, ref IntPtr ActivePolicyGuid);
#pragma warning restore PC003
}
