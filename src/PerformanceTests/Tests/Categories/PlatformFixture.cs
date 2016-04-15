namespace Categories
{
    using System.Collections.Generic;
    using NUnit.Framework;
    using Tests.Permutations;
    using Variables;

    [TestFixture(Description = "Platform x86/x64, gc client/server", Category = "Performance")]
    public class PlatformFixture : Base
    {
        [TestCaseSource(nameof(CreatePermutations))]
        public override void SendLocalOneOnOneRunner(Permutation permutation)
        {
            base.SendLocalOneOnOneRunner(permutation);
        }

        static IEnumerable<Permutation> CreatePermutations()
        {
            return PermutationGenerator.Generate(new Permutations
            {
                Versions = new[] { NServiceBusVersion.V5, NServiceBusVersion.V6, },
                IOPS = new[] { IOPS.Default },
                Platforms = new[] { Platform.x86, Platform.x64, },
                GarbageCollectors = new[] { GarbageCollector.Client, GarbageCollector.Server, },
                Transports = new[] { Transport.MSMQ },
                Persisters = new[] { Persistence.InMemory },
                Serializers = new[] { Serialization.Json, },
                MessageSizes = new[] { MessageSize.Tiny, },
                OutboxModes = new[] { Outbox.Off, },
                DTCModes = new[] { DTC.On, },
                TransactionMode = new[] { TransactionMode.Default, },
                AuditModes = new[] { Audit.Off, },
                ConcurrencyLevels = new[] { ConcurrencyLevel.EnvCores }
            });
        }
    }
}