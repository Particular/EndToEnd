using System.Collections.Generic;

namespace Tests.Categories
{
    using global::Categories;
    using NUnit.Framework;
    using Permutations;
    using Variables;

    [TestFixture(Description = "Persisters", Category = "Azure"), Explicit]
    public class AzureFixture : Base
    {
        [TestCaseSource(nameof(CreatePermutations))]
        public override void GatedPublishRunner(Permutation permutation)
        {
            base.GatedPublishRunner(permutation);
        }

        [TestCaseSource(nameof(CreatePermutations))]
        public override void SagaInitiateRunner(Permutation permutation)
        {
            base.SagaInitiateRunner(permutation);
        }

        static IEnumerable<Permutation> CreatePermutations()
        {
            return PermutationGenerator.Generate(new Permutations
            {
                Transports = new[] { Transport.AzureStorageQueues, Transport.AzureServiceBus,  },
                Persisters = new [] { Persistence.Azure, },
                Serializers = new[] { Serialization.Json, },
                OutboxModes = new[] { Outbox.Off, },
            });
        }
    }
}
