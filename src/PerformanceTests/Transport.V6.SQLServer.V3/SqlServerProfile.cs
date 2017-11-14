using System;
using System.Reflection;
using NServiceBus;
using NServiceBus.Transport.SQLServer;
using Tests.Permutations;
using Variables;

class SqlServerProfile : IProfile, ISetup, INeedPermutation
{
    public Permutation Permutation { private get; set; }

    public void Configure(EndpointConfiguration endpointConfiguration)
    {
        var transport = endpointConfiguration
            .UseTransport<SqlServerTransport>();

        transport
            .DefaultSchema("V6")
            .ConnectionString(ConfigurationHelper.GetConnectionString(Permutation.Transport.ToString()));

        if (Permutation.TransactionMode != TransactionMode.Default
            && Permutation.TransactionMode != TransactionMode.None
            && Permutation.TransactionMode != TransactionMode.Receive
            && Permutation.TransactionMode != TransactionMode.Atomic
            && Permutation.TransactionMode != TransactionMode.Transactional
            ) throw new NotSupportedException("TransactionMode: " + Permutation.TransactionMode);

        if (Permutation.TransactionMode != TransactionMode.Default) transport.Transactions(Permutation.GetTransactionMode());
    }

    void ISetup.Setup()
    {
        var cs = ConfigurationHelper.GetConnectionString(Permutation.Transport.ToString());
        var sql = ResourceHelper.GetManifestResourceTextThatEndsWith("init.sql");
        SqlHelper.CreateDatabase(cs);
        SqlHelper.ExecuteScript(cs, sql);
    }
}
