namespace Tests.Permutations
{
    using System.Collections.Generic;
    using System.Linq;

    public class PermutationGenerator
    {
        public static IEnumerable<Permutation> Generate(Permutations permutations)
        {
            var items =
                from Version in permutations.Versions
                from IOPS in permutations.IOPS
                from Platform in permutations.Platforms
                from GarbageCollector in permutations.GarbageCollectors
                from Transport in permutations.Transports
                from Persister in permutations.Persisters
                from Serializer in permutations.Serializers
                from MessageSize in permutations.MessageSizes
                from OutboxMode in permutations.OutboxModes
                from DTCMode in permutations.DTCModes
                from TransactionMode in permutations.TransactionMode
                from AuditMode in permutations.AuditModes
                from ConcurrencyLevel in permutations.ConcurrencyLevels

                select new Permutation
                {
                    Version = Version,
                    IOPS = IOPS,
                    Platform = Platform,
                    GarbageCollector = GarbageCollector,
                    Transport = Transport,
                    Persister = Persister,
                    Serializer = Serializer,
                    MessageSize = MessageSize,
                    OutboxMode = OutboxMode,
                    DTCMode = DTCMode,
                    TransactionMode = TransactionMode,
                    AuditMode = AuditMode,
                    ConcurrencyLevel = ConcurrencyLevel,
                };
            return items;
        }
    }
}