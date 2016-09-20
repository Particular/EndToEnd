namespace Tests.Permutations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class PermutationGenerator
    {
        static readonly string Separator = "~";

        public static IEnumerable<Permutation> Generate(Permutations permutations, Func<Permutation, bool> filter = null)
        {
            var items =
                from Version in permutations.Versions
                from Platform in permutations.Platforms
                from GarbageCollector in permutations.GarbageCollectors
                from Transport in permutations.Transports
                from Persister in permutations.Persisters
                from Serializer in permutations.Serializers
                from MessageSize in permutations.MessageSizes
                from OutboxMode in permutations.OutboxModes
                from TransactionMode in permutations.TransactionMode
                from AuditMode in permutations.AuditModes
                from ConcurrencyLevel in permutations.ConcurrencyLevels
                from ScaleOut in permutations.ScaleOuts

                select new Permutation
                {
                    Version = Version,
                    Platform = Platform,
                    GarbageCollector = GarbageCollector,
                    Transport = Transport,
                    Persister = Persister,
                    Serializer = Serializer,
                    MessageSize = MessageSize,
                    OutboxMode = OutboxMode,
                    TransactionMode = TransactionMode,
                    AuditMode = AuditMode,
                    ConcurrencyLevel = ConcurrencyLevel,
                    ScaleOut = ScaleOut, 

                    Code = string.Empty
                     + (permutations.Versions.Length > 1 ? Version + Separator : string.Empty)
                     + (permutations.Platforms.Length > 1 ? Platform + Separator : string.Empty)
                     + (permutations.GarbageCollectors.Length > 1 ? GarbageCollector + Separator : string.Empty)
                     + (permutations.Transports.Length > 1 ? Transport + Separator : string.Empty)
                     + (permutations.Persisters.Length > 1 ? Persister + Separator : string.Empty)
                     + (permutations.Serializers.Length > 1 ? Serializer + Separator : string.Empty)
                     + (permutations.MessageSizes.Length > 1 ? MessageSize + Separator : string.Empty)
                     + (permutations.OutboxModes.Length > 1 ? OutboxMode + Separator : string.Empty)
                     + (permutations.TransactionMode.Length > 1 ? TransactionMode + Separator : string.Empty)
                     + (permutations.AuditModes.Length > 1 ? AuditMode + Separator : string.Empty)
                     + (permutations.ConcurrencyLevels.Length > 1 ? ConcurrencyLevel + Separator : string.Empty)
                     + (permutations.ScaleOuts.Length > 1 ? ScaleOut + Separator : string.Empty)
                };

            if (filter != null) items = items.Where(filter);

            return items
                .OrderBy(x => x.Code);
        }
    }
}
