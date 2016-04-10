namespace Tests.Permutations
{
    using Variables;

    public class Permutation
    {
        public string Category { get; set; }
        public string Id { get; set; }
        public string Exe { get; set; }

        public string[] Tests;

        public Audit AuditMode; //0
        public DTC DTCMode; //1
        public IOPS IOPS;//2
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

        public override string ToString()
        {
            return Id;
        }
    }
}