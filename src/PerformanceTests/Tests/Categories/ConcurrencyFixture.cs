namespace Categories
{
    using System;
    using System.Collections.Generic;
    using NUnit.Framework;
    using Tests.Permutations;
    using Variables;

    [TestFixture(Description = "Concurrency", Category = "Performance")]
    public class ConcurrencyFixture : Base
    {
        [TestCaseSource(nameof(CreatePermutations))]
        public override void ReceiveRunner(Permutation permutation)
        {
            base.ReceiveRunner(permutation);
        }

        static IEnumerable<Permutation> CreatePermutations()
        {
            return PermutationGenerator.Generate(new Permutations
            {
                Transports = (Transport[])Enum.GetValues(typeof(Transport)),
                Serializers = new[] { Serialization.Json, },
                OutboxModes = new[] { Outbox.Off, },
                TransactionMode = new[] { TransactionMode.Receive, },
                ConcurrencyLevels = (ConcurrencyLevel[])Enum.GetValues(typeof(ConcurrencyLevel)),
            });
        }
    }
}