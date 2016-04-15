namespace Tests.Permutations
{
    using Variables;

    public class Permutation
    {
        public string Category { get; set; }
        public string Description { get; set; }
        public string Id => GenerateId();
        public string Exe { get; set; }
        public string Code {get; set;}

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
        public ScaleOut ScaleOut = ScaleOut.NoScaleOut;

        string GenerateId()
        {
            return string.Join(";",
                Version.GetEnumDescription(),
                IOPS.GetEnumDescription(),
                Platform.GetEnumDescription(),
                GarbageCollector.GetEnumDescription(),
                Transport.GetEnumDescription(),
                Persister.GetEnumDescription(),
                Serializer.GetEnumDescription(),
                MessageSize.GetEnumDescription(),
                OutboxMode.GetEnumDescription(),
                DTCMode.GetEnumDescription(),
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
