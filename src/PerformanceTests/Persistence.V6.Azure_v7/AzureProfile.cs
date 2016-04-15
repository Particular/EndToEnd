using System;
using NServiceBus;

class AzureProfile : IProfile
{
    public void Configure(EndpointConfiguration busConfiguration)
    {
        throw new NotSupportedException("NServiceBus.Azure v7 is not yet released.");
    }
}
