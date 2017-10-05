#if Version6 || Version7
using Configuration = NServiceBus.EndpointConfiguration;
#else
using Configuration = NServiceBus.BusConfiguration;
#endif

using NServiceBus;
using Tests.Permutations;
using Variables;

class SerializerProfile : IProfile, INeedPermutation
{
    public Permutation Permutation { private get; set; }

    public void Configure(Configuration cfg)
    {
        switch (Permutation.Serializer)
        {
            case Serialization.Xml:
                cfg.UseSerialization<XmlSerializer>();
                break;
            case Serialization.Json:
                cfg.UseSerialization<JsonSerializer>();
                break;
        }
    }
}
