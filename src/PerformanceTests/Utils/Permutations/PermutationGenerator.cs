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
                from ScaleOut in permutations.ScaleOuts

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
                    ScaleOut = ScaleOut,

                    Code = string.Empty
                     + (permutations.Versions.Length > 1 ? Version.ToString() : string.Empty)
                     + (permutations.IOPS.Length > 1 ? IOPS.ToString() : string.Empty)
                     + (permutations.Platforms.Length > 1 ? Platform.ToString() : string.Empty)
                     + (permutations.GarbageCollectors.Length > 1 ? GarbageCollector.ToString() : string.Empty)
                     + (permutations.Transports.Length > 1 ? Transport.ToString() : string.Empty)
                     + (permutations.Persisters.Length > 1 ? Persister.ToString() : string.Empty)
                     + (permutations.Serializers.Length > 1 ? Serializer.ToString() : string.Empty)
                     + (permutations.MessageSizes.Length > 1 ? MessageSize.ToString() : string.Empty)
                     + (permutations.OutboxModes.Length > 1 ? OutboxMode.ToString() : string.Empty)
                     + (permutations.DTCModes.Length > 1 ? DTCMode.ToString() : string.Empty)
                     + (permutations.TransactionMode.Length > 1 ? TransactionMode.ToString() : string.Empty)
                     + (permutations.AuditModes.Length > 1 ? AuditMode.ToString() : string.Empty)
                     + (permutations.ConcurrencyLevels.Length > 1 ? ConcurrencyLevel.ToString() : string.Empty)
                     + (permutations.ScaleOuts.Length > 1 ? ScaleOut.ToString() : string.Empty)
                };
            return items;
        }
    }
}
