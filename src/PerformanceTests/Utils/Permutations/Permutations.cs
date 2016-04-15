namespace Tests.Permutations
{
    using Variables;

    public class Permutations
    {
        public Audit[] AuditModes;
        public DTC[] DTCModes;
        public IOPS[] IOPS;
        public MessageSize[] MessageSizes;
        public NServiceBusVersion[] Versions;
        public Outbox[] OutboxModes;
        public Persistence[] Persisters;
        public Platform[] Platforms;
        public Serialization[] Serializers;
        public Transport[] Transports;
        public GarbageCollector[] GarbageCollectors;
        public TransactionMode[] TransactionMode;
        public ConcurrencyLevel[] ConcurrencyLevels;
        public ScaleOut[] ScaleOuts = { ScaleOut.NoScaleOut };
    }
}