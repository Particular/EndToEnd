namespace Host
{
    using System;
    using System.Linq;
    using System.Net;
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

                ValidateServicePointManager(permutation);

                if (Environment.UserInteractive) Console.Title = PermutationParser.ToString(permutation);

                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                var tasks = permutation.Tests.Select(x => (IStartAndStop)assembly.CreateInstance(x)).ToArray();

                var tests = permutation.Tests.Select(x => assembly.CreateInstance(x)).ToList();
                tests.ForEach(s =>
                {
                    if (s is BaseRunner)
                        ((BaseRunner)s).Execute(permutation, endpointName);
                });

                Run(options, permutation, tasks);
            }
            catch (Exception ex)
            {
                Log.Fatal("Main", ex);
                throw;
            }
        }

        static void ValidateServicePointManager(Permutation permutation)
        {
            var value = ConcurrencyLevelConverter.Convert(permutation.ConcurrencyLevel);
            if (ServicePointManager.DefaultConnectionLimit < value)
            {
                Log.WarnFormat("ServicePointManager.DefaultConnectionLimit value {0} is lower then maximum concurrency limit of {1} this can limit performance.",
                    ServicePointManager.DefaultConnectionLimit,
                    value
                    );
            }

            if (ServicePointManager.Expect100Continue)
            {
                Log.WarnFormat("ServicePointManager.Expect100Continue is set to True, consider setting this value to False to increase PUT operations.");
            }

            if (ServicePointManager.UseNagleAlgorithm)
            {
                Log.WarnFormat("ServicePointManager.UseNagleAlgorithm is set to True, consider setting this value to False to decrease Latency.");
            }
        }

        static void Run(IStartAndStop[] tasks)
        {
            foreach (var t in tasks) t.Start();
            Log.InfoFormat("Warmup: {0}", Settings.WarmupDuration);
            System.Threading.Thread.Sleep(Settings.WarmupDuration);
            Statistics.Instance.Reset();
            Log.InfoFormat("Run: {0}", Settings.RunDuration);
            System.Threading.Thread.Sleep(Settings.RunDuration);
            Statistics.Instance.Dump();
            foreach (var t in tasks) t.Stop();
        }
    }
}
