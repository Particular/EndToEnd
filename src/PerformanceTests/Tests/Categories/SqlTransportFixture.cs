namespace Categories
{
    using System;
    using System.Collections.Generic;
    using NUnit.Framework;
    using Tests.Permutations;
    using Variables;

    [TestFixture(Description = "Transports", Category = "SqlTransports"), Explicit]
    public class SqlTransportsFixture : Base
    {
        [TestCaseSource(nameof(CreatePermutations))]
        public override void GatedSendLocalRunner(Permutation permutation)
        {
            base.GatedSendLocalRunner(permutation);
        }

        [TestCaseSource(nameof(CreatePermutations))]
        public override void GatedPublishRunner(Permutation permutation)
        {
            base.GatedPublishRunner(permutation);
        }

        [TestCaseSource(nameof(CreatePermutations))]
        public override void ReceiveRunner(Permutation permutation)
        {
            base.ReceiveRunner(permutation);
        }

        static IEnumerable<Permutation> CreatePermutations()
        {
            return PermutationGenerator.Generate(new Permutations
            {
                Transports = new[] { Transport.SQLServer, Transport.SQLServer_Azure, },
                MessageSizes = (MessageSize[])Enum.GetValues(typeof(MessageSize)),
                Serializers = new[] { Serialization.Json, },
                OutboxModes = new[] { Outbox.Off, },
                ConcurrencyLevels = (ConcurrencyLevel[])Enum.GetValues(typeof(ConcurrencyLevel)),
            });
        }
    }
}