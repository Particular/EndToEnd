namespace PersistenceCompatibilityTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using DataDefinitions;
    using NUnit.Framework;
    using Common;
    [TestFixture]
    public class UpdatingSagaTests
    {
        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            persisterProvider = new PersisterProvider();
            persisterProvider.Initialize("NServiceBus.NHibernate.Tests", "NServiceBus.NHibernate", NHibernatePackageVersions);    
        }

        [OneTimeTearDown]
        public void CleanUp()
        {
            persisterProvider.Dispose();
        }

        [SetUp]
        public void Setup()
        {
            Database.Cleanup();
        }

        [TestCaseSource(nameof(GenerateTestCases))]
        public void can_fetch_updated_saga_by_correlation_property(string sourceVersion, string destinationVersion)
        {
            var sourcePersister = persisterProvider.Get(sourceVersion);
            var destinationPersister = persisterProvider.Get(destinationVersion);

            var writeData = new TestSagaData
            {
                Id = Guid.NewGuid(),
                Originator = "test-originator",
                SomeValue = "test"
            };

            sourcePersister.Save(writeData, nameof(writeData.Originator), writeData.Originator);

            writeData.SomeValue = "updated";
            sourcePersister.Update(writeData);

            var readByCorrelationProperty = destinationPersister.GetByCorrelationId<TestSagaData>(nameof(writeData.Originator), writeData.Originator);

            Assert.AreEqual(writeData.Id, readByCorrelationProperty.Id);
            Assert.AreEqual(writeData.Originator, readByCorrelationProperty.Originator);
            Assert.AreEqual("updated", readByCorrelationProperty.SomeValue);
        }

        [TestCaseSource(nameof(GenerateTestCases))]
        public void can_fetch_simple_saga_updated_by_another_version(string sourceVersion, string destinationVersion)
        {
            var sourcePersister = persisterProvider.Get(sourceVersion);
            var destinationPersister = persisterProvider.Get(destinationVersion);

            var writeData = new TestSagaData
            {
                Id = Guid.NewGuid(),
                Originator = "test-originator",
                SomeValue = "test"
            };

            sourcePersister.Save(writeData, nameof(writeData.Originator), writeData.Originator);

            writeData.SomeValue = "updated";
            sourcePersister.Update(writeData);

            var readByGuid = destinationPersister.Get<TestSagaData>(writeData.Id);

            Assert.AreEqual(writeData.Id, readByGuid.Id);
            Assert.AreEqual(writeData.Originator, readByGuid.Originator);
            Assert.AreEqual("updated", readByGuid.SomeValue);
        }

        [TestCaseSource(nameof(GenerateTestCases))]
        public void can_fetch_saga_with_list_updated_by_another_version(string sourceVersion, string destinationVersion)
        {
            var sourcePersister = persisterProvider.Get(sourceVersion);
            var destinationPersister = persisterProvider.Get(destinationVersion);

            var writeData = new TestSagaDataWithList
            {
                Id = Guid.NewGuid(),
                Ints = new List<int> { 1, 5, 7, 9, -3 }
            };

            sourcePersister.Save(writeData, nameof(writeData.Id), writeData.Id.ToString());

            writeData.Ints.RemoveAt(1);
            writeData.Ints.RemoveAt(2);
            sourcePersister.Update(writeData);

            var readData = destinationPersister.Get<TestSagaDataWithList>(writeData.Id);

            Assert.AreEqual(writeData.Id, readData.Id);
            CollectionAssert.AreEqual(new List<int> { 1, 7, -3 }, readData.Ints);
        }

        [TestCaseSource(nameof(GenerateTestCases))]
        public void can_fetch_composite_saga_updated_by_another_version(string sourceVersion, string destinationVersion)
        {
            var sourcePersister = persisterProvider.Get(sourceVersion);
            var destinationPersister = persisterProvider.Get(destinationVersion);

            var writeData = new TestSagaDataWithComposite
            {
                Id = Guid.NewGuid(),
                Composite = new TestSagaDataWithComposite.SagaComposite { Value = "test-value" }
            };

            sourcePersister.Save(writeData, nameof(writeData.Id), writeData.Id.ToString());

            writeData.Composite.Value = "updated";
            sourcePersister.Update(writeData);

            var readData = destinationPersister.Get<TestSagaDataWithComposite>(writeData.Id);

            Assert.AreEqual(writeData.Id, readData.Id);
            Assert.AreEqual("updated", readData.Composite.Value);
        }

        static object[][] GenerateTestCases()
        {
            var nuget = new NugetHelper();
            NHibernatePackageVersions = nuget.GetPossibleVersionsFor("NServiceBus.NHibernate", "4.5.0");

            var cases = from va in NHibernatePackageVersions
                        from vb in NHibernatePackageVersions
                        select new object[] { va, vb };

            return cases.ToArray();
        }

        static IEnumerable<string> NHibernatePackageVersions;
        PersisterProvider persisterProvider;
    }
}
