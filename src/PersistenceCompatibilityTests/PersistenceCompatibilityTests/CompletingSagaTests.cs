namespace PersistenceCompatibilityTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Common;
    using DataDefinitions;
    using NUnit.Framework;

    [TestFixture]
    public class CompletingSagaTests
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
        public void completed_saga_can_not_be_accessed(string sourceVersion, string destinationVersion)
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


            var readByCorrelationProperty = destinationPersister.GetByCorrelationId<TestSagaData>(nameof(writeData.Originator), writeData.Originator);

            Assert.AreEqual(writeData.Id, readByCorrelationProperty.Id);
            Assert.AreEqual("test", readByCorrelationProperty.SomeValue);

            sourcePersister.Complete(writeData);

            var completedSaga = destinationPersister.GetByCorrelationId<TestSagaData>(nameof(writeData.Originator), writeData.Originator);
            Assert.IsNull(completedSaga);
        }



        static object[][] GenerateTestCases()
        {
            var nuget = new NugetHelper();
            NHibernatePackageVersions = nuget.GetPossibleVersionsFor("NServiceBus.NHibernate", "5.0.0")
                .Where(v => !v.Contains("-")); // exclude beta and rc versions

            var cases = from va in NHibernatePackageVersions
                        from vb in NHibernatePackageVersions
                        select new object[] { va, vb };

            return cases.ToArray();
        }

        PersisterProvider persisterProvider;
        static IEnumerable<string> NHibernatePackageVersions;
    }
}
