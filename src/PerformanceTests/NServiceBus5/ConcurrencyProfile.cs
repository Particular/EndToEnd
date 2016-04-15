using NServiceBus;
using NServiceBus.Config;
using NServiceBus.Config.ConfigurationSource;
using Tests.Permutations;

class ConcurrencyProfile : IProfile, INeedPermutation, IProvideConfiguration<TransportConfig>
{
    static int MaximumConcurrencyLevel;
    public Permutation Permutation { private get; set; }


    public void Configure(BusConfiguration cfg)
    {
        MaximumConcurrencyLevel = ConcurrencyLevelConverter.Convert(Permutation.ConcurrencyLevel);
    }

    public TransportConfig GetConfiguration()
    {
        return new TransportConfig
        {
            MaximumConcurrencyLevel = MaximumConcurrencyLevel,
        };
    }
}
