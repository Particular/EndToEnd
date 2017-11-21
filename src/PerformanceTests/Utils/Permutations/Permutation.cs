namespace Tests.Permutations
{
    using Variables;

    public class Permutation
    {
        public int PrefetchMultiplier => 4;
        public string Category { get; set; }
        public string Description { get; set; }
        public string Id => GenerateSplunkId();
        public string Code { get; set; }
        public string Fixture { get; set; }

        public string[] Tests;

        public Audit AuditMode; //0
        public MessageSize MessageSize;//3
        public NServiceBusVersion Version;//4
        public Outbox OutboxMode;//5
        public Persistence Persister;//6
        public Platform Platform;//7
        public Serialization Serializer;//8
        public Transport Transport;//9
        public GarbageCollector GarbageCollector;//10
        public TransactionMode TransactionMode;//11
        public ConcurrencyLevel ConcurrencyLevel;//12
        public ScaleOut ScaleOut = ScaleOut.NoScaleOut;//13

        string GenerateSplunkId()
        {
            return string.Join(";",
                Version.GetEnumDescription(),
                "Default",
                Platform.GetEnumDescription(),
                GarbageCollector.GetEnumDescription(),
                Transport.GetEnumDescription(),
                Persister.GetEnumDescription(),
                Serializer.GetEnumDescription(),
                MessageSize.GetEnumDescription(),
                OutboxMode.GetEnumDescription(),
                TransactionMode == TransactionMode.Transactional ? "On" : "Off",
                TransactionMode.GetEnumDescription(),
                AuditMode.GetEnumDescription(),
                ConcurrencyLevel.GetEnumDescription(),
                ScaleOut.GetEnumDescription()
                );
        }

        public override string ToString()
        {
            return Code;
        }
    }
}
