namespace Categories
{
    using System;
    using System.Collections.Generic;
    using NUnit.Framework;
    using Tests.Permutations;
    using Variables;

    [TestFixture(Description = "Platform net452/dotnet-core, gc client/server", Category = "Performance")]
    public class PlatformFixture : Base
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
                Transports = new[] { Transport.RabbitMQ },
                TransactionMode = new[] { TransactionMode.Receive },
                Serializers = new[] { Serialization.Json, },
                OutboxModes = new[] { Outbox.Off, },
            });
        }
    }
}