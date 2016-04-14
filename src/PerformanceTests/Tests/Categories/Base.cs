namespace Categories
{
    using System;
    using System.Configuration;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using NUnit.Framework;
    using Tests.Permutations;
    using Tests.Tools;
    using VisualStudioDebugHelper;
    using Variables;

    public abstract class Base
    {
        public static string SessionId;
        static readonly bool InvokeEnabled = bool.Parse(ConfigurationManager.AppSettings["InvokeEnabled"]);

        public virtual void ReceiveRunner(Permutation permutation)
        {
            Tasks(permutation);
        }

        public virtual void GatedSendLocalRunner(Permutation permutation)
        {
            Tasks(permutation);
        }

        public virtual void SendLocalOneOnOneRunner(Permutation permutation)
        {
            Tasks(permutation);
        }

        public virtual void GatedPublishRunner(Permutation permutation)
        {
            Tasks(permutation);
        }

        void Tasks(Permutation permutation, [CallerMemberName] string memberName = "")
        {
            var fixture = GetType().GetCustomAttribute<TestFixtureAttribute>();
            permutation.Category = fixture.Category;
            permutation.Description = fixture.Description;
            permutation.Tests = new[] { memberName };

            var environment = new TestEnvironment(SessionId);
            environment.CreateTestEnvironments(permutation);

            Invoke(permutation);
        }

        static void Invoke(Permutation permutation)
        {
            if (!InvokeEnabled) Assert.Inconclusive("Invoke disabled, set 'InvokeEnabled' appSetting to True.");

            DeleteOtherPlatformHost(permutation);
            LaunchAndWait(permutation);
        }

        static void LaunchAndWait(Permutation permutation)
        {
            var processId = DebugAttacher.GetCurrentVisualStudioProcessId();
            var processIdArgument = processId >= 0 ? string.Format(" --processId={0}", processId) : string.Empty;
            var sessionIdArgument = string.Format(" --sessionId={0}", SessionId);

            var exe = new FileInfo(permutation.Exe);
            var arguments = PermutationParser.ToArgs(permutation) + processIdArgument + sessionIdArgument;

            var pi = new ProcessStartInfo(exe.FullName)
            {
                Arguments = arguments,
                UseShellExecute = false,
                WorkingDirectory = exe.DirectoryName,
            };


            using (var p = Process.Start(pi))
            {
                if (!p.WaitForExit(150000))
                {
                    p.Kill();
                    Assert.Fail("Killed!");

                }
                Assert.AreEqual(0, p.ExitCode, "Execution failed.");
            }
        }

        static void DeleteOtherPlatformHost(Permutation permutation)
        {
            var x64 = new FileInfo(permutation.Exe.Replace("x86.exe", "x64.exe"));
            var x86 = new FileInfo(permutation.Exe.Replace("x64.exe", "x86.exe"));

            if (permutation.Platform == Platform.x86)
            {
                x64.Delete();
            }
            else
            {
                x86.Delete();
            }
        }
    }
    
    [SetUpFixture]
    public class TestSessionInitializer
    {
        [SetUp]
        public void Setup()
        {
            Base.SessionId = DateTime.UtcNow.Ticks.ToString();
        }
    }
}
