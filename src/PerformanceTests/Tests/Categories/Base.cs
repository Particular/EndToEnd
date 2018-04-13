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

    public abstract class Base
    {
        public static string SessionId;
        static readonly bool InvokeEnabled;
        static readonly TimeSpan MaxDuration;

        static Base()
        {
            // we need to get the correct config file, as .net core will try to load nunits test harness configuration file
            // see https://github.com/dotnet/corefx/issues/22101
            var assemblyLocation = Assembly.GetExecutingAssembly().Location;
            var configurationFile = ConfigurationManager.OpenExeConfiguration(assemblyLocation);
            InvokeEnabled = bool.Parse(configurationFile.AppSettings.Settings["InvokeEnabled"].Value);
            MaxDuration = TimeSpan.Parse(configurationFile.AppSettings.Settings["MaxDuration"].Value);
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
            TestContext.WriteLine($"Running test {fixtureType} ({TestContext.CurrentContext.Test.FullName}/{TestContext.CurrentContext.Test.MethodName}");
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
                var processId = VisualStudioDebugHelper.DebugAttacher.GetCurrentVisualStudioProcessId();
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
                pi = new ProcessStartInfo("/home/perftest/dotnet/dotnet", $"{testDescriptor.ProjectAssemblyPath} {permutationArgs}{sessionIdArgument}")
                {
                    UseShellExecute = true,
                    WorkingDirectory = testDescriptor.ProjectAssemblyDirectory,
                };
            }

            TestContext.WriteLine($"Run test using: '{pi.FileName} {pi.Arguments}'");
            using (var p = Process.Start(pi))
            {
                if (!p.WaitForExit((int)MaxDuration.TotalMilliseconds))
                {
#pragma warning disable PC001
                    p.Kill();
#pragma warning restore PC001
                    Assert.Fail($"Killed process because execution took more then {MaxDuration}.");
                }
#pragma warning disable PC001
                if (p.ExitCode == (int)ReturnCodes.NotSupported)
                {
                    Assert.Ignore("Not supported");
                }
                Assert.AreEqual((int)ReturnCodes.OK, p.ExitCode, "Execution failed.");
#pragma warning restore PC001
            }
        }
    }
}
