using NServiceBus;

public class EncryptionProfile : IProfile
{
    public void Configure(EndpointConfiguration cfg)
    {
#pragma warning disable 618
        cfg.RijndaelEncryptionService();
#pragma warning restore 618
    }
}
