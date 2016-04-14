namespace Host
{
    using System;
    using System.Linq;
    using System.Net;
    using NServiceBus;
    using NServiceBus.Logging;
    using Tests.Permutations;
    using VisualStudioDebugHelper;

    class Program
    {
        static ILog Log;
        static string endpointName = "PerformanceTests_" + AppDomain.CurrentDomain.FriendlyName.Replace(' ', '_');
        static void Main()
        {
            LogManager.Use<NLogFactory>();

            Log = LogManager.GetLogger(typeof(Program));

            DebugAttacher.AttachDebuggerToVisualStudioProcessFromCommandLineParameter();

            AppDomain.CurrentDomain.FirstChanceException += (o, ea) => { Log.Debug("FirstChanceException", ea.Exception); };
            AppDomain.CurrentDomain.UnhandledException += (o, ea) => { Log.Error("UnhandledException", ea.ExceptionObject as Exception); };

            try
            {
                var permutation = PermutationParser.FromCommandlineArgs();

                Statistics.Initialize(permutation);

                EnvironmentStats.Write();

                ValidateServicePointManager(permutation);

                if (Environment.UserInteractive) Console.Title = PermutationParser.ToString(permutation);

                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                var runnableTest = permutation.Tests.Select(x => (BaseRunner) assembly.CreateInstance(x)).Single();
                runnableTest.Execute(permutation, endpointName);
            }
            catch (Exception ex)
            {
                Log.Fatal("Main", ex);
                NLog.LogManager.Shutdown();
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
    }
}
