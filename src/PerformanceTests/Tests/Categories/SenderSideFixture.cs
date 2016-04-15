namespace Categories
{
    using System.Collections.Generic;
    using NUnit.Framework;
    using Tests.Permutations;
    using Variables;

    [TestFixture(Description = "Sender-side distribution", Category = "Performance"), Ignore]
    public class SenderSideFixture : Base
    {
        [TestCaseSource(nameof(CreatePermutations))]
        public override void GatedSendLocalRunner(Permutation permutation)
        {
            base.GatedSendLocalRunner(permutation);
        }

        static IEnumerable<Permutation> CreatePermutations()
        {
            return PermutationGenerator.Generate(new Permutations
            {
                Versions = new[] { NServiceBusVersion.V6 },
                IOPS = new[] { IOPS.Default },
                Platforms = new[] { Platform.x64, },
                GarbageCollectors = new[] { GarbageCollector.Client, },
                Transports = new[] { Transport.MSMQ },
                Persisters = new[] { Persistence.InMemory },
                Serializers = new[] { Serialization.Json },
                MessageSizes = new[] { MessageSize.Tiny },
                OutboxModes = new[] { Outbox.Off, },
                DTCModes = new[] { DTC.On, },
                TransactionMode = new[] { TransactionMode.Default, },
                AuditModes = new[] { Audit.Off, },
                ConcurrencyLevels = new[] { ConcurrencyLevel.EnvCores4x },
                ScaleOuts = new[] { ScaleOut.SenderSide, }
            });
        }
    }
}