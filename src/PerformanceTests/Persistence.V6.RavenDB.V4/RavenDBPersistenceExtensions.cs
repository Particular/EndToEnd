using System;
using System.Configuration;
using System.Data.Common;
using NServiceBus;
using NServiceBus.Persistence.RavenDB;
using Raven.Client.Document;
using Raven.Client.Document.DTC;

static class RavenDBPersistenceExtensions
{
    public static PersistenceExtensions<RavenDBPersistence> SetConnectionStringName(this PersistenceExtensions<RavenDBPersistence> cfg, string name)
    {
        var value = ConfigurationManager.ConnectionStrings[name];

        if (value == null) throw new InvalidOperationException($"Connection string '{name}' not configured.");

        return SetConnectionString(cfg, value.ConnectionString);
    }

    public static PersistenceExtensions<RavenDBPersistence> SetConnectionString(this PersistenceExtensions<RavenDBPersistence> cfg, string connectionstring)
    {
        var builder = new DbConnectionStringBuilder { ConnectionString = connectionstring };

        //var cp = new ConnectionParameters();

        var store = new DocumentStore();

        if(builder.ContainsKey("url")) store.Url = builder[ "url" ] as string;
        if(builder.ContainsKey("database")) store.DefaultDatabase = builder[ "database" ] as string;
        if(builder.ContainsKey("defaultdatabase")) store.DefaultDatabase = builder[ "defaultdatabase" ] as string;
        if(builder.ContainsKey("apikey")) store.ApiKey = builder[ "apikey" ] as string;
        if(builder.ContainsKey("domain")) store.Credentials = new System.Net.NetworkCredential(builder[ "user" ] as string, builder[ "password" ] as string, builder[ "domain" ] as string);
        else if(builder.ContainsKey("password")) store.Credentials = new System.Net.NetworkCredential(builder[ "user" ] as string, builder[ "password" ] as string);

        store.TransactionRecoveryStorage = new LocalDirectoryTransactionRecoveryStorage(@"c:\temp\raven-dtc");
        store.Initialize();

        return cfg.SetDefaultDocumentStore(store);
    }
}