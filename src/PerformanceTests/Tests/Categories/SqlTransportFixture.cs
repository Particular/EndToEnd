namespace Categories
{
    using System.Collections.Generic;
    using NUnit.Framework;
    using Tests.Permutations;
    using Variables;

    [TestFixture(Description = "Transports", Category = "Performance"), Explicit]
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
                Versions = new[] { NServiceBusVersion.V7 },
                Platforms = new[] { Platform.NetCore, Platform.NetFramework },
                Transports = new[] { Transport.SQLServer, },
                MessageSizes = new[] { MessageSize.Tiny },
                Serializers = new[] { Serialization.Json, },
                OutboxModes = new[] { Outbox.Off, },
                ConcurrencyLevels = new [] { ConcurrencyLevel.EnvCores08x },
            });
        }
    }
}