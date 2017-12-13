namespace Categories
{
    using System.Collections.Generic;
    using NUnit.Framework;
    using Tests.Permutations;
    using Variables;

    [TestFixture(Description = "Sender-side distribution", Category = "Performance"), Explicit]
    public class SenderSideFixture : Base
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
                Versions = new[] { NServiceBusVersion.V6 },
                Transports = new[] { Transport.MSMQ },
                Serializers = new[] { Serialization.Json },
                OutboxModes = new[] { Outbox.Off, },
                ScaleOuts = new[] { ScaleOut.SenderSide, }
            });
        }
    }
}