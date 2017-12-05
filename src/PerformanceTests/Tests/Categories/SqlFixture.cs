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
                Platforms = new[] { Platform.NetCore, Platform.NetFramework },
                Transports = new[] { Transport.SQLServer },
                Persisters = new[] { Persistence.Sql, },
                MessageSizes = new[] { MessageSize.Tiny, },
                Serializers = new[] { Serialization.Json, },
                OutboxModes = new[] { Outbox.Off },
                TransactionMode = new[] { TransactionMode.Atomic }, // TransactionScope currently not supported.
                ConcurrencyLevels = new[] { ConcurrencyLevel.EnvCores02x, ConcurrencyLevel.Sequential }
            });
        }
    }
}
