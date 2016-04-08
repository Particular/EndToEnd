namespace VisualStudioDebugHelper
{
    using System;
    using System.Linq;
    using System.Diagnostics;

    public static class DebugAttacher
    {
        public static void AttachDebuggerToVisualStudioProcessFromCommandLineParameter()
        {
            var processId = Environment.GetCommandLineArgs().Where(arg => arg.StartsWith("--processId")).Select(arg => Convert.ToInt32(arg.Substring("--processId".Length + 1))).FirstOrDefault();
            AttachDebuggerToVisualStudioInstance(processId);
        }

        public static void AttachDebuggerToVisualStudioInstance(int visualStudioProcessId)
        {
            if (!Debugger.IsAttached && visualStudioProcessId > 0)
            {
                var vsProcess = VisualStudioAttacher.GetVisualStudioByProcessId(visualStudioProcessId);

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
