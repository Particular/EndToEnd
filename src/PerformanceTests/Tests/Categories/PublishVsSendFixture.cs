namespace Categories
{
    using System.Collections.Generic;
    using NUnit.Framework;
    using Tests.Permutations;
    using Variables;

    [TestFixture(Description = "Publish vs Send", Category = "Performance")]
    public class PublishVsSendFixture : Base
    {
        [TestCaseSource(nameof(CreatePermutations))]
        public override void GatedSendLocalRunner(Permutation permutation)
        {
            base.GatedSendLocalRunner(permutation);
        }

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
                Platforms = new[] { Platform.x86, },
                GarbageCollectors = new[] { GarbageCollector.Client, },
                Transports = new[] { Transport.MSMQ, Transport.AzureServiceBus, Transport.AzureStorageQueues, Transport.RabbitMQ, Transport.SQLServer, },
                Persisters = new[] { Persistence.InMemory },
                Serializers = new[] { Serialization.Json, },
                MessageSizes = new[] { MessageSize.Tiny, },
                OutboxModes = new[] { Outbox.Off, },
                DTCModes = new[] { DTC.On, },
                TransactionMode = new[] { TransactionMode.Default, },
                AuditModes = new[] { Audit.Off },
                ConcurrencyLevels = new[] { ConcurrencyLevel.EnvCores4x }
            });
        }
    }
}