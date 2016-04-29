namespace Categories
{
    using System;
    using System.Collections.Generic;
    using NUnit.Framework;
    using Tests.Permutations;
    using Variables;

    [TestFixture(Description = "Resources utilization", Category = "Performance")]
    public class ResourceUtilizationFixture : Base
    {
        [TestCaseSource(nameof(CreatePermutations))]
        public override void SendLocalOneOnOneRunner(Permutation permutation)
        {
            base.SendLocalOneOnOneRunner(permutation);
        }

        static IEnumerable<Permutation> CreatePermutations()
        {
            return PermutationGenerator.Generate(new Permutations
            {
                Platforms = (Platform[])Enum.GetValues(typeof(Platform)),
                GarbageCollectors = (GarbageCollector[])Enum.GetValues(typeof(GarbageCollector)),
                Transports = new[] { Transport.MSMQ },
                Serializers = new[] { Serialization.Json, },
                OutboxModes = new[] { Outbox.Off },
                ConcurrencyLevels = new[] { ConcurrencyLevel.Sequential }
            });
        }
    }
}