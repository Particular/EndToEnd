namespace Categories
{
    using System.Collections.Generic;
    using NUnit.Framework;
    using Tests.Permutations;
    using Variables;
    using System.Linq;

    [TestFixture(Description = "Sagas", Category = "Performance")]
    public class SagasFixture : Base
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
                Transports = new[] { Transport.RabbitMQ },
                Persisters = new[] { Persistence.Azure, Persistence.NHibernate, Persistence.RavenDB, Persistence.RavenDB_Embedded, Persistence.Sql, Persistence.InMemory, },
                Serializers = new[] { Serialization.Json, },
                OutboxModes = new[] { Outbox.Off, },
                ConcurrencyLevels = new[] { ConcurrencyLevel.Sequential, ConcurrencyLevel.EnvCores, ConcurrencyLevel.EnvCores04x },
                TransactionMode = new[] { TransactionMode.Receive }
            },
            p => (p.Persister == Persistence.Azure && p.TransactionMode != TransactionMode.Transactional || p.Persister != Persistence.Azure)
                 && (p.Version != NServiceBusVersion.V5 || !new[] { Persistence.Sql, Persistence.Sql_Azure, Persistence.Sql_RDS }.Contains(p.Persister))
            );
        }
    }
}
