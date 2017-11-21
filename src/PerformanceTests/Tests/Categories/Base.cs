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
    using Variables;
#if NET452
    using VisualStudioDebugHelper;
#endif

    public abstract class Base
    {
        public static string SessionId;
        static readonly bool InvokeEnabled;
        static readonly TimeSpan MaxDuration;

        static Base()
        {
            try
            {
//                InvokeEnabled = bool.Parse(ConfigurationManager.AppSettings["InvokeEnabled"]);
//                MaxDuration = TimeSpan.Parse(ConfigurationManager.AppSettings["MaxDuration"]);
                InvokeEnabled = true;
                MaxDuration = TimeSpan.FromMinutes(5);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

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

        public virtual void SagaInitiateRunner(Permutation permutation)
        {
            Tasks(permutation);
        }

        public virtual void ForRunner(Permutation permutation)
        {
            Tasks(permutation);
        }

        public virtual void ParallelForRunner(Permutation permutation)
        {
            Tasks(permutation);
        }

        public virtual void TaskArrayRunner(Permutation permutation)
        {
            Tasks(permutation);
        }

        public virtual void PublishOneOnOneRunner(Permutation permutation)
        {
            Tasks(permutation);
        }

        protected void Tasks(Permutation permutation, [CallerMemberName] string memberName = "")
        {
            var fixtureType = GetType();
            var fixture = fixtureType.GetCustomAttribute<TestFixtureAttribute>();
            permutation.Fixture = fixtureType.Name;

            permutation.Category = fixture.Category;
            permutation.Description = fixture.Description;
            permutation.Tests = new[] { memberName };

            var environment = new TestEnvironment(SessionId);
            var testDescriptor = environment.CreateTestEnvironments(permutation);

            Invoke(permutation, testDescriptor);
        }

        static void Invoke(Permutation permutation, TestDescriptor testDescriptor)
        {
            if (!InvokeEnabled) Assert.Inconclusive("Invoke disabled, set 'InvokeEnabled' appSetting to True.");

            LaunchAndWait(permutation, testDescriptor);
            Console.WriteLine(ScanLogs.ToIniString(new FileInfo(testDescriptor.ProjectAssemblyPath).DirectoryName));
        }

        static void LaunchAndWait(Permutation permutation, TestDescriptor testDescriptor)
        {
            var permutationArgs = PermutationParser.ToArgs(permutation);
            var sessionIdArgument = $" --sessionId={SessionId}";

            ProcessStartInfo pi;
            if (permutation.Platform == Platform.NetFramework)
            {
                // ReSharper disable once RedundantAssignment
                var processIdArgument = string.Empty;
#if NET452
                var processId = DebugAttacher.GetCurrentVisualStudioProcessId();
                processIdArgument = processId >= 0 ? $" --processId={processId}" : string.Empty;
#endif
                pi = new ProcessStartInfo(testDescriptor.ProjectAssemblyPath, permutationArgs + sessionIdArgument + processIdArgument)
                {
                    UseShellExecute = true,
                    WorkingDirectory = testDescriptor.ProjectAssemblyDirectory,
                };
            }
            else
            {
                pi = new ProcessStartInfo("dotnet", $"{testDescriptor.ProjectAssemblyPath} {permutationArgs}{sessionIdArgument}")
                {
                    UseShellExecute = true,
                    WorkingDirectory = testDescriptor.ProjectAssemblyDirectory,
                };
            }

            using (var p = Process.Start(pi))
            {
                if (!p.WaitForExit((int)MaxDuration.TotalMilliseconds))
                {
                    p.Kill();
                    Assert.Fail($"Killed process because execution took more then {MaxDuration}.");
                }
                if (p.ExitCode == (int)ReturnCodes.NotSupported)
                {
                    Assert.Inconclusive("Not supported");
                }
                Assert.AreEqual((int)ReturnCodes.OK, p.ExitCode, "Execution failed.");
            }
        }
    }
}
