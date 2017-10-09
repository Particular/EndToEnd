namespace Categories
{
    using System.Collections.Generic;
    using NUnit.Framework;
    using Tests.Permutations;
    using Variables;

    [TestFixture(Description = "Transports", Category = "Performance"), Explicit]
    public class AzureServiceBusFixture : Base
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

        [TestCaseSource(nameof(CreatePermutations))]
        public override void PublishOneOnOneRunner(Permutation permutation)
        {
            base.PublishOneOnOneRunner(permutation);
        }

        [TestCaseSource(nameof(CreatePermutations))]
        public override void SendLocalOneOnOneRunner(Permutation permutation)
        {
            base.SendLocalOneOnOneRunner(permutation);
        }

        static IEnumerable<Permutation> CreatePermutations()
        {
            return PermutationGenerator.Generate(new Permutations
            {
                Versions = new[] { NServiceBusVersion.V7 },
                Transports = new[] { Transport.AzureServiceBus },
                MessageSizes = new[] { MessageSize.Tiny },
                Serializers = new[] { Serialization.Json },
                OutboxModes = new[] { Outbox.Off },
                ConcurrencyLevels = new[] { ConcurrencyLevel.EnvCores04x },
                TransactionMode = new[] { TransactionMode.Atomic, TransactionMode.Receive, TransactionMode.None }
            });
        }
    }
}