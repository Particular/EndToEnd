namespace Categories
{
    using System.Collections.Generic;
    using NUnit.Framework;
    using Tests.Permutations;
    using Variables;

    [TestFixture(Description = "Transports", Category = "Performance")]
    public class AmazonSQSFixture : Base
    {
        [TestCaseSource(nameof(CreatePermutations))]
        public override void GatedSendLocalRunner(Permutation permutation)
        {
            base.GatedSendLocalRunner(permutation);
        }

        [TestCaseSource(nameof(CreatePermutations))]
        public override void ReceiveRunner(Permutation permutation)
        {
            base.ReceiveRunner(permutation);
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
                Platforms = new[] { Platform.NetCore, Platform.NetFramework },
                Transports = new[] { Transport.AmazonSQS },
                Persisters = new[] { Persistence.InMemory },
                MessageSizes = new[] { MessageSize.Tiny },
                Serializers = new[] { Serialization.Json },
                OutboxModes = new[] { Outbox.Off },
                TransactionMode = new[] { TransactionMode.Receive },
                ConcurrencyLevels = new[] { ConcurrencyLevel.Sequential }
            });
        }
    }
}