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
        public override void SagaInitiateRunner(Permutation permutation)
        {
            base.SagaInitiateRunner(permutation);
        }

        static IEnumerable<Permutation> CreatePermutations()
        {
            return PermutationGenerator.Generate(new Permutations
            {
                Versions = new[] { NServiceBusVersion.V7 },
                Transports = new[] { Transport.MSMQ },
                Persisters = new[] { Persistence.Azure },
                Serializers = new[] { Serialization.Json },
                OutboxModes = new[] { Outbox.Off },
                TransactionMode = new [] { TransactionMode.Receive },
                ConcurrencyLevels = new[] { ConcurrencyLevel.EnvCores04x }
            });
        }
    }
}
