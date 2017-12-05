namespace Categories
{
    using System.Collections.Generic;
    using NUnit.Framework;
    using Tests.Permutations;
    using Variables;

    [TestFixture(Description = "Persisters", Category = "Performance"), Explicit]
    public class AzureFixture : Base
    {
        [TestCaseSource(nameof(CreatePermutations))]
        public void SagaUpdateRunner(Permutation permutation)
        {
            Tasks(permutation);
        }

        [TestCaseSource(nameof(CreatePermutations))]
        public override void SagaInitiateRunner(Permutation permutation)
        {
            base.SagaInitiateRunner(permutation);
        }

        [TestCaseSource(nameof(CreatePermutations))]
        public void SagaCongestionRunner(Permutation permutation)
        {
            Tasks(permutation);
        }

        static IEnumerable<Permutation> CreatePermutations()
        {
            return PermutationGenerator.Generate(new Permutations
            {
                Versions = new[] { NServiceBusVersion.V7, NServiceBusVersion.V6, NServiceBusVersion.V5 },
                Platforms = new[] { Platform.NetCore, Platform.NetFramework },
                Transports = new[] { Transport.RabbitMQ },
                Persisters = new[] { Persistence.Azure },
                Serializers = new[] { Serialization.Json },
                OutboxModes = new[] { Outbox.Off },
                TransactionMode = new [] { TransactionMode.Receive },
                ConcurrencyLevels = new[] { ConcurrencyLevel.Sequential }
            });
        }
    }
}
