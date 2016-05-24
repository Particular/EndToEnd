﻿using System;
using System.Collections.Generic;
using System.Linq;
using DataDefinitions;
using NUnit.Framework;

namespace PersistenceCompatibilityTests
{
    [TestFixture]
    public class NHibernatePersistenceTests
    {
        [OneTimeSetUp]
        public void Setup()
        {
            persisterProvider = new PersisterProvider();
            persisterProvider.Initialize(NHibernatePackageVersions);    
        }

        [OneTimeTearDown]
        public void CleanUp()
        {
            persisterProvider.Dispose();
        }

        [TestCaseSource(nameof(GenerateTestCases))]
        public void can_fetch_simple_saga_persisted_by_another_version(string sourceVersion, string destinationVersion)
        {
            var sourcePersister = persisterProvider.Get(sourceVersion);
            var destinationPersister = persisterProvider.Get(destinationVersion);

            var writeData = new TestSagaData
            {
                Id = Guid.NewGuid(),
                Originator = "test-originator"
            };

            sourcePersister.Save(writeData, nameof(writeData.Id), writeData.Id.ToString());

            var readData = destinationPersister.Get<TestSagaData>(writeData.Id);

            Assert.AreEqual(writeData.Id, readData.Id);
            Assert.AreEqual(writeData.Originator, readData.Originator);
        }

        [TestCaseSource(nameof(GenerateTestCases))]
        public void can_fetch_saga_with_list_persisted_by_another_version(string sourceVersion, string destinationVersion)
        {
            var sourcePersister = persisterProvider.Get(sourceVersion);
            var destinationPersister = persisterProvider.Get(destinationVersion);

            var writeData = new TestSagaDataWithList 
            {
                Id = Guid.NewGuid(),
                Ints = new List<int> { 1, 5, 7, 9, -3}
            };

            sourcePersister.Save(writeData, nameof(writeData.Id), writeData.Id.ToString());

            var readData = destinationPersister.Get<TestSagaDataWithList>(writeData.Id);

            Assert.AreEqual(writeData.Id, readData.Id);
            CollectionAssert.AreEqual(writeData.Ints, readData.Ints);
        }

        [TestCaseSource(nameof(GenerateTestCases))]
        public void can_fetch_composite_saga_persisted_by_another_version(string sourceVersion, string destinationVersion)
        {
            var sourcePersister = persisterProvider.Get(sourceVersion);
            var destinationPersister = persisterProvider.Get(destinationVersion);

            var writeData = new TestSagaDataWithComposite
            {
                Id = Guid.NewGuid(),
                Composite = new TestSagaDataWithComposite.SagaComposite { Value = "test-value" }
            };

            sourcePersister.Save(writeData, nameof(writeData.Id), writeData.Id.ToString());

            var readData = destinationPersister.Get<TestSagaDataWithComposite>(writeData.Id);

            Assert.AreEqual(writeData.Id, readData.Id);
            CollectionAssert.AreEqual(writeData.Composite.Value, readData.Composite.Value);
        }

        static string[] NHibernatePackageVersions => new[] { "4.5", "5.0", "6.2", "7.0" };

        static object[][] GenerateTestCases()
        {
            var cases = from va in NHibernatePackageVersions
                        from vb in NHibernatePackageVersions
                        select new object[] {va, vb};

            return cases.ToArray();
        }

        PersisterProvider persisterProvider;
    }
}