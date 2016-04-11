namespace NServiceBus5
{
    using System;
    using System.Linq;
    using Common;
    using NServiceBus;
    using Tests.Permutations;
    using Utils;
    using VisualStudioDebugHelper;

    class Program
    {
        public static IBus Instance;
        static string endpointName = "PerformanceTests_" + AppDomain.CurrentDomain.FriendlyName.Replace(' ', '_');
        static void Main(string[] args)
        {
            DebugAttacher.AttachDebuggerToVisualStudioProcessFromCommandLineParameter();

            try
            {
                TraceLogger.Initialize();

                Statistics.Initialize();

                Log.Env();

                var permutation = PermutationParser.FromCommandlineArgs();
                var options = BusCreationOptions.Parse(args);

                if (Environment.UserInteractive) Console.Title = permutation.ToString();

                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                var tasks = permutation.Tests.Select(x => (IStartAndStop) assembly.CreateInstance(x)).ToArray();

                var tests = permutation.Tests.Where(s => s is BaseRunner).Select(x => (BaseRunner)assembly.CreateInstance(x)).ToList();
                tests.ForEach(s => s.Execute(permutation, endpointName));

                var runDuration = TimeSpan.FromSeconds(30);

                using (Instance = CreateBus(options, permutation))
                {
                    //TestRunner.EndpointName = endpointName;
                    //TestRunner.RunTests(bus, options);
                    foreach (var t in tasks) t.Start();
                    System.Threading.Thread.Sleep(5000); // Warmup
                    Statistics.Instance.Reset();
                    System.Threading.Thread.Sleep(runDuration);
                    Statistics.Instance.Dump();
                    foreach (var t in tasks) t.Stop();
                }
            }
            catch (Exception ex)
            {
                NServiceBus.Logging.LogManager.GetLogger(typeof(Program)).Fatal("Main", ex);
                throw;
            }
        }

        static IBus CreateBus(BusCreationOptions options, Permutation permutation)
        {
            //if (options.Cleanup)
            //{
            //    QueueUtils.DeleteQueuesForEndpoint(endpointName);
            //}

            var configuration = new BusConfiguration();
            configuration.EndpointName(endpointName);
            configuration.EnableInstallers();
            configuration.DiscardFailedMessagesInsteadOfSendingToErrorQueue();
            configuration.ApplyProfiles(permutation);
            configuration.EnableFeature<NServiceBus.Performance.SimpleStatisticsFeature>();
            configuration.PurgeOnStartup(false);
            var startableBus = Bus.Create(configuration);

            return startableBus.Start();
        }
    }
}
