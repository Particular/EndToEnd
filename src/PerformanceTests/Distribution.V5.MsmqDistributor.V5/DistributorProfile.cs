using System;
using NServiceBus;
using Tests.Permutations;
using Variables;
using NServiceBus.Config;
using NServiceBus.Config.ConfigurationSource;

class DistributorProfile : IProfile, INeedPermutation, IProvideConfiguration<MasterNodeConfig>
{
    const string DistributorKey = "Distributor";
    string node;
    public Permutation Permutation { private get; set; }

        public void Configure(BusConfiguration cfg)
        {
            if (Permutation.ScaleOut != ScaleOut.MsmqDistributor) return;

        var value = this.FetchSetting(DistributorKey);

        if (value == null) throw new InvalidOperationException($"Expected setting {DistributorKey}");

        var isDistributor = string.Equals("distributor", value, StringComparison.InvariantCultureIgnoreCase);
        var isMaster = string.Equals("master", value, StringComparison.InvariantCultureIgnoreCase);

        if (isDistributor || isMaster)
        {
            cfg.RunMSMQDistributor(withWorker: isMaster);
            return;
        }

        cfg.EnlistWithMSMQDistributor();
        node = value;
    }

    public MasterNodeConfig GetConfiguration()
    {
        return new MasterNodeConfig
        {
            Node = node
        };
    }
}
