#if Version6 || Version7
using Configuration = NServiceBus.EndpointConfiguration;
#else
using Configuration = NServiceBus.BusConfiguration;
#endif
namespace NServiceBus5
{
    using NServiceBus;
    using Tests.Permutations;

    class ErrorProfile : IProfile, INeedPermutation
    {
        public Permutation Permutation { private get; set; }

        public void Configure(Configuration cfg)
        {
#if Version6 || Version7
            cfg.SendFailedMessagesTo(System.Configuration.ConfigurationManager.AppSettings["NServiceBus/ErrorQueue"]);
#endif
        }
    }
}