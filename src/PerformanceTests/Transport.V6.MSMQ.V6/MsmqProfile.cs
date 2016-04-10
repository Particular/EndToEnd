
using NServiceBus;
using Tests.Permutations;
using Variables;

class MsmqProfile : IProfile, INeedPermutation
{
    public Permutation Permutation { private get; set; }

    public void Configure(EndpointConfiguration endpointConfiguration)
    {
        var transport = endpointConfiguration.UseTransport<MsmqTransport>();
        transport.ConnectionString("deadLetter=false;journal=false");

        if (Permutation.DTCMode == DTC.Off)
            transport.Transactions(TransportTransactionMode.ReceiveOnly | TransportTransactionMode.SendsAtomicWithReceive);
    }
}
