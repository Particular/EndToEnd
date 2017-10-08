using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using NServiceBus;
using NServiceBus.Logging;
using NServiceBus.Settings;
using Raven.Abstractions.Connection;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Document.DTC;
using Raven.Client.Embedded;
using Variables;

public class RavenDBProfile : IProfile, INeedContext, ISetup
{
    static readonly string ConnectionString = ConfigurationHelper.GetConnectionString("RavenDB");

    ILog Log = LogManager.GetLogger(nameof(RavenDBProfile));

    public IContext Context { private get; set; }

    public void Configure(EndpointConfiguration cfg)
    {
        cfg.UsePersistence<RavenDBPersistence>()
           .DoNotSetupDatabasePermissions()
           .SetDefaultDocumentStore(CreateDocumentStore)
           .DisableSubscriptionVersioning();
    }

    public bool blaat;
    IDocumentStore CreateDocumentStore(ReadOnlySettings settings)
    {
        // Calculate a ResourceManagerId unique to this endpoint using just LocalAddress
        // Not suitable for side-by-side installations!
        var resourceManagerId = DeterministicGuidBuilder(Context.EndpointName);

        // Calculate a DTC transaction recovery storage path including the ResourceManagerId
        var programDataPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
        var txRecoveryPath = Path.Combine(programDataPath, "NServiceBus.RavenDB", Context.EndpointName);

        if (Context.Permutation.Persister == Persistence.RavenDB)
        {
            var store = new DocumentStore
            {
                DefaultDatabase = Context.EndpointName
            };

            store.ParseConnectionString(ConnectionString);
            store.ResourceManagerId = resourceManagerId;
            store.TransactionRecoveryStorage = new LocalDirectoryTransactionRecoveryStorage(txRecoveryPath);

            Log.InfoFormat("ResourceManagerId = {0}", resourceManagerId);
            Log.InfoFormat("TransactionRecoveryStorage = {0}", txRecoveryPath);

            return store;
        }
        else
        {
            if (instance != null) return instance;

            var store = new EmbeddableDocumentStore
            {
                DataDirectory = @"d:\temp\perftest\data\ravendb",
                DefaultDatabase = "PerfTests",
                ResourceManagerId = resourceManagerId,
                EnlistInDistributedTransactions = Context.Permutation.TransactionMode == TransactionMode.Transactional,
                //  TransactionRecoveryStorage = new LocalDirectoryTransactionRecoveryStorage(txRecoveryPath)
            };

            store.Initialize();

            var configuration = store.Configuration;
            configuration.Settings.Add("Raven/Esent/CacheSizeMax", "256");
            configuration.Settings.Add("Raven/Esent/MaxVerPages", "32");
            configuration.Settings.Add("Raven/MemoryCacheLimitMegabytes", "512");
            configuration.Settings.Add("Raven/MaxNumberOfItemsToIndexInSingleBatch", "4096");
            configuration.Settings.Add("Raven/MaxNumberOfItemsToPreFetchForIndexing", "4096");
            configuration.Settings.Add("Raven/InitialNumberOfItemsToIndexInSingleBatch", "64");

            return instance = store;
        }
    }

    static IDocumentStore instance;

    static Guid DeterministicGuidBuilder(string input)
    {
        // use MD5 hash to get a 16-byte hash of the string
        using (var provider = new MD5CryptoServiceProvider())
        {
            var inputBytes = Encoding.UTF8.GetBytes(input);
            var hashBytes = provider.ComputeHash(inputBytes);
            // generate a guid from the hash:
            return new Guid(hashBytes);
        }
    }

    public void Setup()
    {
        try
        {
            using (var store = new DocumentStore())
            {
                store.ParseConnectionString(ConnectionString);
                store.Initialize();
                store.DatabaseCommands.GlobalAdmin.DeleteDatabase("PerformanceTest", true);
            }
        }
        catch (ErrorResponseException)
        {
            Log.Info("Database does not exists, ignoring...");
        }
    }
}