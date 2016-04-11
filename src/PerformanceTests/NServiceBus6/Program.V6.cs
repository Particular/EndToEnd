namespace Host
{
    using System.Threading.Tasks;
    using Common;
    using NServiceBus;
    using Tests.Permutations;
    using Utils;

    partial class Program
    {
        public static IEndpointInstance Instance;

        static void Run(BusCreationOptions options, Permutation permutation, IStartAndStop[] tasks)
        {
            Instance = CreateBus(options, permutation).Result;
            try
            {
                Run(tasks);
            }
            finally
            {
                Instance.Stop();
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
            configuration.PurgeOnStartup(false);

            var endpoint = await Endpoint.Create(configuration);
            return await endpoint.Start();
        }
    }
}
