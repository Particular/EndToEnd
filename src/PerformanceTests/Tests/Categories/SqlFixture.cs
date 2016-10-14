namespace Categories
{
    using System.Collections.Generic;
    using NUnit.Framework;
    using Tests.Permutations;
    using Variables;

    [TestFixture(Description = "Transports", Category = "Performance"), Explicit]
    public class SqlFixture : Base
    {
        [TestCaseSource(nameof(CreatePermutations))]
        public void SagaUpdateRunner(Permutation permutation)
        {
            Tasks(permutation);
        }

        [TestCaseSource(nameof(CreatePermutations))]
        public override void PublishOneOnOneRunner(Permutation permutation)
        {
            Tasks(permutation);
        }

        static IEnumerable<Permutation> CreatePermutations()
        {
            return PermutationGenerator.Generate(new Permutations
            {
                Transports = new[] { Transport.SQLServer, Transport.SQLServer_Azure, Transport.SQLServer_RDS, },
                Persisters = new[] { Persistence.NHibernate, Persistence.NHibernate_Azure, Persistence.NHibernate_RDS, },
                MessageSizes = new[] { MessageSize.Tiny, },
                Serializers = new[] { Serialization.Json, },
                OutboxModes = new[] { Outbox.Off, Outbox.On, },
                TransactionMode = new[] { TransactionMode.Atomic, TransactionMode.Receive, TransactionMode.Transactional, },
                ConcurrencyLevels = new[] { ConcurrencyLevel.Sequential, }

            }, p => (p.Transport == Transport.SQLServer && p.Persister == Persistence.NHibernate
                    || p.Transport == Transport.SQLServer_Azure && p.Persister == Persistence.NHibernate_Azure
                    || p.Transport == Transport.SQLServer_RDS && p.Persister == Persistence.NHibernate_RDS)
                    &&
                    (p.OutboxMode == Outbox.On && p.TransactionMode == TransactionMode.Transactional
                    || p.OutboxMode == Outbox.Off && p.TransactionMode == TransactionMode.Atomic
                    )
                    );
        }
    }
}