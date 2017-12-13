using System;
using System.Collections.Generic;
using NUnit.Framework;
using Tests.Permutations;
using Variables;

namespace Categories
{
    [TestFixture(Description = "Transaction Modes", Category = "Performance"), Explicit]
    public class TransactionModesFixture : Base
    {
        [TestCaseSource(nameof(CreatePermutations))]
        public override void ReceiveRunner(Permutation permutation)
        {
            base.ReceiveRunner(permutation);
        }

        [TestCaseSource(nameof(CreatePermutations))]
        public override void GatedSendLocalRunner(Permutation permutation)
        {
            base.GatedSendLocalRunner(permutation);
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
                Transports = (Transport[])Enum.GetValues(typeof(Transport)),
                Persisters = new[] { Persistence.InMemory, },
                OutboxModes = new[] { Outbox.Off, },
                TransactionMode = new[] { TransactionMode.Receive, TransactionMode.Atomic, TransactionMode.Transactional, TransactionMode.None }
            });
        }
    }
}