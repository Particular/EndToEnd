namespace Host
{
    using System;
    using System.Linq;
    using NServiceBus.Logging;
    using Tests.Permutations;
    using Utils;
    using VisualStudioDebugHelper;

    partial class Program
    {
        static readonly ILog Log = LogManager.GetLogger(typeof(Program));
        static string endpointName = "PerformanceTests_" + AppDomain.CurrentDomain.FriendlyName.Replace(' ', '_');
        static void Main(string[] args)
        {
            DebugAttacher.AttachDebuggerToVisualStudioProcessFromCommandLineParameter();

            try
            {
                TraceLogger.Initialize();

                Statistics.Initialize();

                EnvironmentStats.Write();

                var permutation = PermutationParser.FromCommandlineArgs();
                var options = BusCreationOptions.Parse(args);

                if (Environment.UserInteractive) Console.Title = PermutationParser.ToString(permutation);

                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                var tasks = permutation.Tests.Select(x => (IStartAndStop)assembly.CreateInstance(x)).ToArray();

                Run(options, permutation, tasks);
            }
            catch (Exception ex)
            {
                Log.Fatal("Main", ex);
                throw;
            }
        }

        static void Run(IStartAndStop[] tasks)
        {
            foreach (var t in tasks) t.Start();
            Log.InfoFormat("Warmup");
            System.Threading.Thread.Sleep(Settings.WarmupDuration);
            Statistics.Instance.Reset();
            Log.InfoFormat("Run");
            System.Threading.Thread.Sleep(Settings.RunDuration);
            Statistics.Instance.Dump();
            foreach (var t in tasks) t.Stop();
        }
    }
}