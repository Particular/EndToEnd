using System;
using NServiceBus;
using NServiceBus.Persistence;
using NServiceBus.SagaPersisters;
using Tests.Permutations;
using Variables;

public class AzureProfile : IProfile, INeedPermutation
{
    public Permutation Permutation { private get; set; }

    public void Configure(BusConfiguration cfg)
    {
        if (Permutation.OutboxMode == Outbox.On) throw new NotSupportedException("Outbox is not supported with Azure storage.");

        var connectionString = this.GetConnectionString("AzureStorage");
        cfg.UsePersistence<AzureStoragePersistence, StorageType.Sagas>().ConnectionString(connectionString);
        cfg.UsePersistence<AzureStoragePersistence, StorageType.Subscriptions>().ConnectionString(connectionString);
        cfg.UsePersistence<AzureStoragePersistence, StorageType.Timeouts>().ConnectionString(connectionString);
    }

}
