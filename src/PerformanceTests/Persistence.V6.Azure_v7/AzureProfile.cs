using System;
using NServiceBus;
using NServiceBus.Persistence;
using Tests.Permutations;
using Variables;

class AzureProfile : IProfile, INeedPermutation
{
    public Permutation Permutation { private get; set; }

    public void Configure(EndpointConfiguration cfg)
    {
        if (Permutation.OutboxMode == Outbox.On) throw new NotSupportedException("Outbox is not supported with Azure storage.");

        var connectionString = ConfigurationHelper.GetConnectionString("AzureStorage");

        cfg.UsePersistence<AzureStoragePersistence>()
            .ConnectionString(connectionString);

        cfg.UsePersistence<AzureStoragePersistence, StorageType.Sagas>()
            .AssumeSecondaryIndicesExist();
    }
}
