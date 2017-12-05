using System;
using System.Collections.Generic;
using NUnit.Framework;
using Tests.Permutations;
using Variables;

namespace Categories
{

    [TestFixture(Description = "Audit forwarding On / Off", Category = "Performance"), Explicit]
    public class AuditOnVsOffFixture : Base
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
                Serializers = new[] { Serialization.Json, },
                MessageSizes = (MessageSize[])Enum.GetValues(typeof(MessageSize)),
                OutboxModes = (Outbox[])Enum.GetValues(typeof(Outbox)),
                AuditModes = (Audit[])Enum.GetValues(typeof(Audit)),
            });
        }
    }
}
