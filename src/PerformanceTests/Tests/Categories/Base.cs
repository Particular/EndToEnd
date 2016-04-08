namespace Categories
{
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.CompilerServices;
    using NUnit.Framework;
    using Tests.Permutations;
    using Tests.Tools;
    using VisualStudioDebugHelper;
    using Variables;

    public class Base
    {
        public virtual void GatedSendLocalRunner(Permutation permutation)
        {
            Tasks(permutation);
        }

        public virtual void SendLocalOneOnOneRunner(Permutation permutation)
        {
            Tasks(permutation);
        }

        void Tasks(Permutation permutation, [CallerMemberName] string memberName = "")
        {
            permutation.Tests = new[] { memberName };
            var environment = new TestEnvironment();
            environment.CreateTestEnvironments(permutation);
            Invoke(permutation);
        }

        static void Invoke(Permutation permutation)
        {
            var processId = DebugAttacher.GetCurrentVisualStudioProcessId();
            var processIdArgument = processId >= 0 ? string.Format(" --processId={0}", processId) : string.Empty;
            
            var x64 = new FileInfo(permutation.Exe);
            var x86 = new FileInfo(permutation.Exe.Replace(".exe", ".x86.exe"));

            var exe = (permutation.Platform == Platform.x86 ? x86 : x64);

            if (permutation.Platform == Platform.x86)
            {
                x64.Delete();
            }
            else
            {
                x86.Delete();
            }

            var pi = new ProcessStartInfo(exe.FullName, PermutationParser.ToArgs(permutation) + processIdArgument)
            {
                UseShellExecute = false,
                WorkingDirectory = exe.DirectoryName,
            };


            using (var p = Process.Start(pi))
            {
                if (!p.WaitForExit(70000))
                {
                    p.Kill();
                    Assert.Fail("Killed!");

                }
                Assert.AreEqual(0, p.ExitCode, "Execution failed.");
            }
        }
    }
}
