namespace Utils
{
    using CommandLine;

    public class BusCreationOptions
    {
        public static BusCreationOptions Parse(string[] args)
        {
            var result = new BusCreationOptions();
            Parser.Default.ParseArguments(args, result);

            return result;
        }

        [Option(nameof(NumberOfMessages))]
        public int NumberOfMessages { get; set; }

        [Option(nameof(Volatile))]
        public bool Volatile { get; set; }

        [Option(nameof(SuppressDTC))]
        public bool SuppressDTC { get; set; }

        [Option(nameof(TwoPhaseCommit))]
        public bool TwoPhaseCommit { get; set; }

        [Option(nameof(Saga))]
        public bool Saga { get; set; }

        [Option(nameof(UseEncryption))]
        public bool UseEncryption { get; set; }

        [Option(nameof(Concurrency))]
        public int Concurrency { get; set; }

        [Option(nameof(Transport))]
        public TransportKind Transport { get; set; }

        [Option(nameof(Serialization))]
        public SerializationKind Serialization { get; set; }

        [Option(nameof(NumberOfThreads))]
        public int NumberOfThreads { get; set; } = 10;

        [Option(nameof(Persistence))]
        public PersistenceKind Persistence { get; set; }

        [Option(nameof(Cleanup))]
        public bool Cleanup { get; set; } = true;
    }

    public enum TransportKind
    {
        Msmq
    }

    public enum PersistenceKind
    {
        InMemory,
    }

    public enum SerializationKind
    {
        Xml,
        Json,
        Bson,
        Bin
    }
}