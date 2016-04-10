namespace Host
{
    using Common;
    using NServiceBus;
    using Tests.Permutations;
    using Utils;


    partial class Program
    {
        public static IBus Instance;

        static void Run(BusCreationOptions options, Permutation permutation, IStartAndStop[] tasks)
        {
            using (Instance = CreateBus(options, permutation))
            {
                Run(tasks);
            }
        }

        static IBus CreateBus(BusCreationOptions options, Permutation permutation)
        {
            if (options.Cleanup)
            {
                QueueUtils.DeleteQueuesForEndpoint(endpointName);
            }

            var configuration = new BusConfiguration();
            configuration.EndpointName(endpointName);
            configuration.EnableInstallers();
            configuration.DiscardFailedMessagesInsteadOfSendingToErrorQueue();
            configuration.ApplyProfiles(permutation);
            configuration.EnableFeature<NServiceBus.Performance.SimpleStatisticsFeature>();
            var startableBus = Bus.Create(configuration);


            return startableBus.Start();
        }
    }
}
