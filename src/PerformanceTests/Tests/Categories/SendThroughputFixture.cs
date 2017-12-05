using System;
using System.Collections.Generic;
using NUnit.Framework;
using Tests.Permutations;
using Variables;

namespace Categories
{ 
    [TestFixture(Description = "Send throughput", Category = "Performance"), Explicit]
    public class SendThroughputFixture : Base
    {
        [TestCaseSource(nameof(CreatePermutations))]
        public override void ForRunner(Permutation permutation)
        {
            base.ForRunner(permutation);
        }

        [TestCaseSource(nameof(CreatePermutations))]
        public override void ParallelForRunner(Permutation permutation)
        {
            base.ParallelForRunner(permutation);
        }

        [TestCaseSource(nameof(CreatePermutations))]
        public override void TaskArrayRunner(Permutation permutation)
        {
            base.TaskArrayRunner(permutation);
        }

        static IEnumerable<Permutation> CreatePermutations()
        {
            return PermutationGenerator.Generate(new Permutations
            {
                Versions = new[] { NServiceBusVersion.V6 },
                OutboxModes = new [] { Outbox.Off },
                MessageSizes = (MessageSize[])Enum.GetValues(typeof(MessageSize)),
            });
        }
    }
}
