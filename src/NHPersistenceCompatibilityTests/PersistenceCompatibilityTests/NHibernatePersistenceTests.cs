using System;
using System.Collections.Generic;
using System.Linq;
using DataDefinitions;
using NUnit.Framework;

namespace PersistenceCompatibilityTests
{
    [TestFixture]
    public class NHibernatePersistenceTests : TestRun
    {
        [Test]
        public void test()
        {
            var persister = CreatePersister("4.5");
            var writeData = new TestSagaData
            {
                Id = Guid.NewGuid(),
                Originator = "test-originator"
            };

            persister.Save<TestSagaData>(writeData);

            var readData = persister.Get<TestSagaData>(writeData.Id);

            Assert.AreEqual(writeData.Id, readData.Id);
            Assert.AreEqual(writeData.Originator, readData.Originator);
        }

        [TestCaseSource(nameof(GenerateTestCases))]
        public void can_fetch_saga_persisted_by_another_version(string sourceVersion, string destinationVersion)
        {
            var sourceRunner = CreateTestFacade<ITestPersistence>(sourceVersion);
            var destinationRunner = CreateTestFacade<ITestPersistence>(destinationVersion);

            var id = Guid.NewGuid();
            var originator = sourceVersion;

            sourceRunner.Run(t => t.Persist(id, originator));
            destinationRunner.Run(t => t.Verify(id, originator));
        }

        [TestCaseSource(nameof(GenerateTestCases))]
        public void can_fetch_saga_with_list_persisted_by_another_version(string sourceVersion, string destinationVersion)
        {
            var sourceRunner = CreateTestFacade<ITestPersistence>(sourceVersion);
            var destinationRunner = CreateTestFacade<ITestPersistence>(destinationVersion);

            var id = Guid.NewGuid();
            var originator = sourceVersion;

            sourceRunner.Run(t => t.Persist(id, new List<int> {1, 13, 19}, originator));
            destinationRunner.Run(t => t.Verify(id, new List<int> {1, 13, 19}, originator));
        }

        [TestCaseSource(nameof(GenerateTestCases))]
        public void can_fetch_composite_saga_persisted_by_another_version(string sourceVersion, string destinationVersion)
        {
            var sourceRunner = CreateTestFacade<ITestPersistence>(sourceVersion);
            var destinationRunner = CreateTestFacade<ITestPersistence>(destinationVersion);

            var id = Guid.NewGuid();
            var compositeText = "composite-value";
            var originator = sourceVersion;

            sourceRunner.Run(t => t.Persist(id, compositeText, originator));
            destinationRunner.Run(t => t.Verify(id, compositeText, originator));
        }

        static object[][] GenerateTestCases()
        {
            var versions = new [] {"4.5", "5.0", "6.2", "7.0"};

            var cases = from va in versions
                from vb in versions
                select new object[] {va, vb};

            return cases.ToArray();
        }
    }
}