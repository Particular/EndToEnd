namespace Categories
{
    using System.Collections.Generic;
    using NUnit.Framework;
    using Tests.Permutations;
    using Variables;

    [TestFixture(Description = "Persisters", Category = "Performance"), Explicit]
    public class PersistersFixture : Base
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
                Versions = new[] { NServiceBusVersion.V5, NServiceBusVersion.V6, },
                IOPS = new[] { IOPS.Default },
                Platforms = new[] { Platform.x86, },
                GarbageCollectors = new[] { GarbageCollector.Client, },
                Transports = new[] { Transport.MSMQ },
                Persisters = new[] { Persistence.InMemory, Persistence.NHibernate, Persistence.RavenDB },
                Serializers = new[] { Serialization.Json, },
                MessageSizes = new[] { MessageSize.Tiny, },
                OutboxModes = new[] { Outbox.Off, Outbox.On, },
                DTCModes = new[] { DTC.Off, DTC.On, },
                TransactionMode = new[] { TransactionMode.Default, },
                AuditModes = new[] { Audit.Off },
                ConcurrencyLevels = new[] { ConcurrencyLevel.EnvCores }
            });
        }
    }
}