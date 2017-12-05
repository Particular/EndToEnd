using System.Collections.Generic;
using NUnit.Framework;
using Tests.Permutations;
using Variables;

namespace Categories
{

    [TestFixture(Description = "Serializer differences", Category = "Performance"), Explicit]
    public class SerializerFixture : Base
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
                Transports = new[] { Transport.MSMQ },
                Serializers = new[] { Serialization.Json, Serialization.Xml  },
                MessageSizes = new [] {MessageSize.Tiny },
                OutboxModes = new[] { Outbox.Off, },
            });
        }
    }
}
