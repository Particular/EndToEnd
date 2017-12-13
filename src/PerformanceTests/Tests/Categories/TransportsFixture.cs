namespace Categories
{
    using System;
    using System.Collections.Generic;
    using NUnit.Framework;
    using Tests.Permutations;
    using Variables;

    [TestFixture(Description = "Transports", Category = "Performance")]
    public class TransportsFixture : Base
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

        [TestCaseSource(nameof(CreatePermutations))]
        public override void ReceiveRunner(Permutation permutation)
        {
            base.ReceiveRunner(permutation);
        }

        static IEnumerable<Permutation> CreatePermutations()
        {
            return PermutationGenerator.Generate(new Permutations
            {
                Transports = new []{Transport.AmazonSQS, Transport.AzureStorageQueues, Transport.RabbitMQ, Transport.SQLServer},
                Serializers = new[] { Serialization.Json, },
                OutboxModes = new[] { Outbox.Off, },
                TransactionMode = new[] { TransactionMode.Receive, },
                ConcurrencyLevels = new []{ ConcurrencyLevel.EnvCores, ConcurrencyLevel.EnvCores04x, ConcurrencyLevel.EnvCores16x}
            });
        }
    }
}