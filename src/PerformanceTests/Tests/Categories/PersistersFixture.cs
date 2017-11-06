namespace Categories
{
    using System;
    using System.Collections.Generic;
    using NUnit.Framework;
    using Tests.Permutations;
    using Variables;
    using System.Linq;

    [TestFixture(Description = "Persisters", Category = "Performance")]
    public class PersistersFixture : Base
    {
        [TestCaseSource(nameof(CreatePermutations))]
        public override void GatedPublishRunner(Permutation permutation)
        {
            base.GatedPublishRunner(permutation);
        }

        static IEnumerable<Permutation> CreatePermutations()
        {
            return PermutationGenerator.Generate(new Permutations
            {
                Transports = new[] { Transport.MSMQ },
                TransactionMode = new[] { TransactionMode.None, },
                Persisters = (Persistence[])Enum.GetValues(typeof(Persistence)),
                Serializers = new[] { Serialization.Json, },
                OutboxModes = new[] { Outbox.Off, },
            },
            filter => filter.Version != NServiceBusVersion.V5 || !new[] { Persistence.Sql, Persistence.Sql_Azure, Persistence.Sql_RDS }.Contains(filter.Persister));
        }
    }
}
