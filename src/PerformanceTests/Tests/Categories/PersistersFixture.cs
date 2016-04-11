namespace Categories
{
    using System.Collections.Generic;
    using NUnit.Framework;
    using Tests.Permutations;
    using Variables;

    [TestFixture(Description = "Persisters", Category = "Performance")]
    public class PersistersFixture : Base
    {
        [TestCaseSource(nameof(CreatePermutations))]
        public override void GatedPublishRunner(Permutation permutation)
        {
            base.GatedPublishRunner(permutation);
        }

        static IEnumerable<Permutation> CreatePermutations()
        {
            return PermutationGenerator.Generate(new Permutations
            {
                Versions = new[] { NServiceBusVersion.V5, NServiceBusVersion.V6, },
                IOPS = new[] { IOPS.Default },
                Platforms = new[] { Platform.x64, },
                GarbageCollectors = new[] { GarbageCollector.Client, },
                Transports = new[] { Transport.MSMQ },
                Persisters = new[] { Persistence.NHibernate, Persistence.RavenDB },//Persistence.InMemory,
                Serializers = new[] { Serialization.Json, },
                MessageSizes = new[] { MessageSize.Tiny, },
                OutboxModes = new[] { Outbox.On, },
                DTCModes = new[] { DTC.On, },
                TransactionMode = new[] { TransactionMode.Default, },
                AuditModes = new[] { Audit.Off },
                ConcurrencyLevels = new[] { ConcurrencyLevel.EnvCores4x }
            });
        }
    }
}