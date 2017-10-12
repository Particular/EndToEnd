namespace Categories
{
    using System.Collections.Generic;
    using NUnit.Framework;
    using Tests.Permutations;
    using Variables;

    [TestFixture(Category = "Performance"), Explicit]
    public class SqlPersistenceFixture : Base
    {
        [TestCaseSource(nameof(CreatePermutations))]
        public override void GatedPublishRunner(Permutation permutation)
        {
            base.GatedPublishRunner(permutation);
        }

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
                Versions = new[] { NServiceBusVersion.V7 },
                Transports = new[] { Transport.MSMQ },
                Persisters = new[] { Persistence.Sql, },
                MessageSizes = new[] { MessageSize.Tiny, },
                Serializers = new[] { Serialization.Json, },
                OutboxModes = new[] { Outbox.Off },
                TransactionMode = new[] { TransactionMode.Transactional, },
                ConcurrencyLevels = new[] { ConcurrencyLevel.EnvCores04x, }
            });
        }
    }
}