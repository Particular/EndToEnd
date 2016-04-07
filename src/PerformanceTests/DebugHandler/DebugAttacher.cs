namespace VisualStudioDebugHelper
{
    using System.Diagnostics;

    public static class DebugAttacher
    {
        public static void AttachDebugger(int processId)
        {
            if (!Debugger.IsAttached && processId > 0)
            {
                var vsProcess = VisualStudioAttacher.GetVisualStudioByProcessId(processId);

                if (vsProcess != null)
                {
                    VisualStudioAttacher.AttachVisualStudioToProcess(vsProcess, Process.GetCurrentProcess());
                }
                else
                {
                    Debugger.Launch();
                }
            }
        }

        public static int GetCurrentVisualStudioProcessId()
        {
            if (!Debugger.IsAttached)
            {
                return -1;
            }

            var currentProcess = Process.GetCurrentProcess();
            return VisualStudioAttacher.GetParentProcessId(currentProcess.Id);
        }
    }
}
