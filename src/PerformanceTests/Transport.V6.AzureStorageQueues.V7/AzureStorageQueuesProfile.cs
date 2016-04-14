using Common;
using NServiceBus;

class AzureStorageQueuesProfile : IProfile
{
    public void Configure(EndpointConfiguration endpointConfiguration)
    {
        endpointConfiguration
            .UseTransport<AzureStorageQueueTransport>()
            .ConnectionString(this.GetConnectionString("AzureStorageQueue"));
    }

    //public void CreateTestData(EndpointConfiguration configuration)
    //{
    //    throw new NotImplementedException();
    //}

    //public void CleanUpTestData(EndpointConfiguration configuration)
    //{
    //    var connectionString = configuration.GetSettings().GetOrDefault<string>(WellKnownConfigurationKeys.ReceiverConnectionString);

    //    var storageAccount = CloudStorageAccount.Parse(connectionString);
    //    var queueClient = storageAccount.CreateCloudQueueClient();
    //    IEnumerable<CloudQueue> allQueues = queueClient.ListQueues().ToArray();

    //    var countdown = new CountdownEvent(allQueues.Count());

    //    foreach (var queue in allQueues)
    //    {
    //        queue.ClearAsync().ContinueWith(t => countdown.Signal());
    //    }

    //    if (countdown.Wait(TimeSpan.FromMinutes(1)) == false)
    //    {
    //        throw new TimeoutException("Waiting for cleaning queues took too much.");
    //    }
    //}
}
