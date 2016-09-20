namespace Categories
{
    using System;
    using System.Collections.Generic;
    using NUnit.Framework;
    using Tests.Permutations;
    using Variables;

    [TestFixture(Description = "Outbox vs DTC", Category = "OutboxVsDtc")]
    public class OutboxVsDtcFixture : Base
    {
        [TestCaseSource(nameof(CreatePermutations))]
        public override void GatedSendLocalRunner(Permutation permutation)
        {
            base.GatedSendLocalRunner(permutation);
        }

        static IEnumerable<Permutation> CreatePermutations()
        {
            return PermutationGenerator.Generate(new Permutations
            {
                Transports = new[] { Transport.MSMQ },
                Persisters = new[] { Persistence.NHibernate, Persistence.RavenDB, },
                Serializers = new[] { Serialization.Json, },
                MessageSizes = (MessageSize[])Enum.GetValues(typeof(MessageSize)),
                OutboxModes = (Outbox[])Enum.GetValues(typeof(Outbox)),
                TransactionMode = new[] { TransactionMode.Transactional, TransactionMode.Atomic, }
            }, p => p.OutboxMode == Outbox.Off && p.TransactionMode == TransactionMode.Transactional || p.OutboxMode == Outbox.On && p.TransactionMode != TransactionMode.Transactional);
        }
    }
}
