using System;
using System.Runtime.InteropServices;

public class Powerplan
{
    public static Guid GetActive()
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
