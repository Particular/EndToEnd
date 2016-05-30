namespace Integration
{
    using System.Linq;
    using NUnit.Framework;
    using Tests.Permutations;
    using Tests.Tools;
    using Variables;

    [TestFixture]
    public class PermutationDirectoryResolverTests
    {
        PermutationDirectoryResolver resolver;

        [SetUp]
        public void Setup()
        {
            resolver = new PermutationDirectoryResolver(TestContext.CurrentContext.TestDirectory);
        }

        [Test]
        public void Should_resolve_files_based_on_permutation()
        {
            var permutation = new Permutation
            {
                Persister = Persistence.NHibernate,
                Version = NServiceBusVersion.V6
            };


        var result = resolver.Resolve(permutation);
            Assert.That(result.RootProjectDirectory, Is.EqualTo("NServiceBus6"));
            Assert.That(result.Files, Is.Not.Empty);
        }

        [TestCase(NServiceBusVersion.V5, Persistence.InMemory, "Persistence.V5.InMemory.dll")]
        [TestCase(NServiceBusVersion.V5, Persistence.NHibernate, "Persistence.V5.NHibernate_v6.dll")]
        [TestCase(NServiceBusVersion.V5, Persistence.RavenDB, "Persistence.V5.RavenDB_v3.dll")]
        [TestCase(NServiceBusVersion.V6, Persistence.InMemory, "Persistence.V6.InMemory.dll")]
        [TestCase(NServiceBusVersion.V6, Persistence.NHibernate, "Persistence.V6.NHibernate_v7.dll")]
        [TestCase(NServiceBusVersion.V6, Persistence.RavenDB, "Persistence.V6.RavenDB_v4.dll")]
        public void Should_resolve_persisters(NServiceBusVersion version, Persistence persister, string fileName)
        {
            var permutation = new Permutation
            {
                Persister = persister,
                Version = version
            };

            var result = resolver.Resolve(permutation);
            var format = $"The name {fileName} was not found in {string.Join(", ", result.Files.Select(f => f.Name))}";
            Assert.IsTrue(result.Files.Any(info => info.Name == fileName), format);
        }

        [TestCase(NServiceBusVersion.V5, Transport.MSMQ, "Transport.V5.MSMQ.dll")]
        [TestCase(NServiceBusVersion.V5, Transport.AzureServiceBus, "Transport.V5.AzureServiceBus_v6.dll")]
        [TestCase(NServiceBusVersion.V5, Transport.AzureStorageQueues, "Transport.V5.AzureStorageQueues_v6.dll")]
        [TestCase(NServiceBusVersion.V5, Transport.RabbitMQ, "Transport.V5.RabbitMQ_v3.dll")]
        [TestCase(NServiceBusVersion.V5, Transport.SQLServer, "Transport.V5.SQLServer_v2.dll")]
        [TestCase(NServiceBusVersion.V6, Transport.MSMQ, "Transport.V6.MSMQ.dll")]
        [TestCase(NServiceBusVersion.V6, Transport.AzureServiceBus, "Transport.V6.AzureServiceBus_v7.dll")]
        [TestCase(NServiceBusVersion.V6, Transport.AzureStorageQueues, "Transport.V6.AzureStorageQueues_v7.dll")]
        [TestCase(NServiceBusVersion.V6, Transport.RabbitMQ, "Transport.V6.RabbitMQ_v4.dll")]
        [TestCase(NServiceBusVersion.V6, Transport.SQLServer, "Transport.V6.SQLServer_v3.dll")]
        public void Should_resolve_transports(NServiceBusVersion version, Transport transport, string fileName)
        {
            var permutation = new Permutation
            {
                Transport= transport,
                Version = version
            };

            var result = resolver.Resolve(permutation);
            var format = $"The name {fileName} was not found in {string.Join(", ", result.Files.Select(f => f.Name))}";
            Assert.IsTrue(result.Files.Any(info => info.Name == fileName), format);
        }

        [Test]
        public void Should_resolve_unversioned_dependencies()
        {
            var permutation = new Permutation
            {
                Persister = Persistence.InMemory,
                Transport = Transport.MSMQ,
                Version = NServiceBusVersion.V5
            };

            var result = resolver.Resolve(permutation);
            Assert.That(result.Files.Any(info => info.Name == "Transport.V5.MSMQ.dll"));
            Assert.That(result.Files.Any(info => info.Name == "Persistence.V5.InMemory.dll"));
        }

        [Test]
        public void Should_not_contain_NServiceBus_Core_assembly()
        {
            var permutation = new Permutation
            {
                Persister = Persistence.NHibernate,
                Version = NServiceBusVersion.V6
            };

            var result = resolver.Resolve(permutation);
            Assert.That(result.Files.All(info => info.Name != "NServiceBus.Core.dll"));
        }
    }
}