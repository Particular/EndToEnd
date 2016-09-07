namespace Categories
{
    using System.Collections.Generic;
    using NUnit.Framework;
    using Tests.Permutations;
    using Variables;

    [TestFixture(Description = "Persisters", Category = "Performance")]
    public class PersistersPublishStrategiesFixture : Base
    {
        [TestCaseSource(nameof(CreatePermutations))]
        public override void PublishOneOnOneRunner(Permutation permutation)
        {
            base.PublishOneOnOneRunner(permutation);
        }
        static IEnumerable<Permutation> CreatePermutations()
        {
            return PermutationGenerator.Generate(new Permutations
            {
                Transports = new[] { Transport.MSMQ },
                Persisters = new [] { Persistence.Azure, Persistence.InMemory, Persistence.NHibernate, Persistence.NHibernate_Azure, Persistence.RavenDB, },
                Serializers = new[] { Serialization.Json, },
                OutboxModes = new[] { Outbox.Off, },
                TransactionMode = new[] { TransactionMode.Atomic, TransactionMode.Transactional, }
            });
        }
    }
}