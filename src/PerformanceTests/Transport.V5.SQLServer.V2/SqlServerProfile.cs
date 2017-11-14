using System;
using System.Reflection;
using NServiceBus;
using NServiceBus.Settings;
using Tests.Permutations;
using Variables;

class SqlServerProfile : IProfile, ISetup, INeedPermutation
{
    public Permutation Permutation { private get; set; }

    public void Configure(BusConfiguration busConfiguration)
    {
        busConfiguration
            .UseTransport<SqlServerTransport>()
            .DefaultSchema("V5")
            .ConnectionString(ConfigurationHelper.GetConnectionString(Permutation.Transport.ToString()));

        InitTransactionMode(busConfiguration.Transactions());
    }

    void InitTransactionMode(TransactionSettings transactionSettings)
    {
        switch (Permutation.TransactionMode)
        {
            case TransactionMode.Default:
                return;
            case TransactionMode.None:
                transactionSettings.Disable();
                return;
            case TransactionMode.Transactional:
                transactionSettings.EnableDistributedTransactions();
                return;
            case TransactionMode.Atomic:
                transactionSettings.DisableDistributedTransactions();
                return;
            case TransactionMode.Receive:
            default:
                throw new NotSupportedException("TransactionMode: " + Permutation.TransactionMode);
        }
    }

    void ISetup.Setup()
    {
        var cs = ConfigurationHelper.GetConnectionString(Permutation.Transport.ToString());
        var sql = ResourceHelper.GetManifestResourceTextThatEndsWith("init.sql");
        SqlHelper.CreateDatabase(cs);
        SqlHelper.ExecuteScript(cs, sql);
    }
}
