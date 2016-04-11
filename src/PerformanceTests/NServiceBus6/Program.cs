namespace NServiceBus6
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Common;
    using NServiceBus;
    using Tests.Permutations;
    using Utils;
    using VisualStudioDebugHelper;

    class Program
    {
        public static IEndpointInstance Instance;
        static string endpointName = "PerformanceTests_" + AppDomain.CurrentDomain.FriendlyName.Replace(' ', '_');
        static void Main(string[] args)
        {
            DebugAttacher.AttachDebuggerToVisualStudioProcessFromCommandLineParameter();

            try
            {
                Statistics.Initialize();

                Log.Env();

                var permutation = PermutationParser.FromCommandlineArgs();
                var options = BusCreationOptions.Parse(args);

                if (Environment.UserInteractive) Console.Title = permutation.ToString();

                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                var tasks = permutation.Tests.Select(x => (IStartAndStop)assembly.CreateInstance(x)).ToArray();

                var tests = permutation.Tests.Select(x => (BaseRunner)assembly.CreateInstance(x)).ToList();
                tests.ForEach(s => s.Execute(permutation, endpointName));

                var runDuration = TimeSpan.FromSeconds(30);

                Instance = CreateBus(options, permutation).Result;
                try
                {
                    //TestRunner.EndpointName = endpointName;
                    //TestRunner.RunTests(endpointInstance, options);
                    foreach (var t in tasks) t.Start();
                    System.Threading.Thread.Sleep(5000);
                    Statistics.Instance.Reset();
                    System.Threading.Thread.Sleep(runDuration);
                    Statistics.Instance.Dump();
                    foreach (var t in tasks) t.Stop();
                }
                finally
                {
                    Instance.Stop();
                }
            }
            catch (Exception ex)
            {
                NServiceBus.Logging.LogManager.GetLogger(typeof(Program)).Fatal("Main", ex);
                throw;
            }
        }

        static async Task<IEndpointInstance> CreateBus(BusCreationOptions options, Permutation permutation)
        {
            if (options.Cleanup)
            {
                QueueUtils.DeleteQueuesForEndpoint(endpointName);
            }

            var configuration = new EndpointConfiguration(endpointName);
            configuration.EnableInstallers();
            configuration.LimitMessageProcessingConcurrencyTo(options.NumberOfThreads);
            configuration.ApplyProfiles(permutation);
            configuration.EnableFeature<NServiceBus.Performance.SimpleStatisticsFeature>();

            var endpoint = await Endpoint.Create(configuration);
            return await endpoint.Start();
        }
    }
}
