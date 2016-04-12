namespace Host
{
    using System;
    using System.Linq;
    using System.Net;
    using NServiceBus.Logging;
    using Tests.Permutations;
    using Utils;
    using VisualStudioDebugHelper;

    class Program
    {
        static readonly ILog Log = LogManager.GetLogger(typeof(Program));
        static string endpointName = "PerformanceTests_" + AppDomain.CurrentDomain.FriendlyName.Replace(' ', '_');
        static void Main(string[] args) // GatedSendLocalRunner, 
        {
            DebugAttacher.AttachDebuggerToVisualStudioProcessFromCommandLineParameter();

            try
            {
                var permutation = PermutationParser.FromCommandlineArgs();

                Statistics.Initialize(permutation.Id);

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
