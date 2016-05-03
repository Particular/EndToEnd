using System;
using System.Configuration;
using System.Data.Common;
using NServiceBus;
using NServiceBus.Persistence.RavenDB;

static class RavenDBPersistenceExtentions
{
    public static PersistenceExtentions<RavenDBPersistence> SetConnectionStringName(this PersistenceExtentions<RavenDBPersistence> cfg, string name)
    {
        var value = ConfigurationManager.ConnectionStrings[name];

        if (value == null) throw new InvalidOperationException($"Connection string '{name}' not configured.");

        return SetConnectionString(cfg, value.ConnectionString);
    }

    public static PersistenceExtentions<RavenDBPersistence> SetConnectionString(this PersistenceExtentions<RavenDBPersistence> cfg, string connectionstring)
    {
        var builder = new DbConnectionStringBuilder { ConnectionString = connectionstring };

        var cp = new ConnectionParameters();

        if (builder.ContainsKey("url")) cp.Url = builder["url"] as string;
        if (builder.ContainsKey("database")) cp.DatabaseName = builder["database"] as string;
        if (builder.ContainsKey("defaultdatabase")) cp.DatabaseName = builder["defaultdatabase"] as string;
        if (builder.ContainsKey("apikey")) cp.ApiKey = builder["apikey"] as string;
        if (builder.ContainsKey("domain")) cp.Credentials = new System.Net.NetworkCredential(builder["user"] as string, builder["password"] as string, builder["domain"] as string);
        else if (builder.ContainsKey("password")) cp.Credentials = new System.Net.NetworkCredential(builder["user"] as string, builder["password"] as string);

        return cfg.SetDefaultDocumentStore(cp);
    }
}