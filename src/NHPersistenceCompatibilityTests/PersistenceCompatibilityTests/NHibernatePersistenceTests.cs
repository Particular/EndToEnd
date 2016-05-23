using System;
using System.Collections.Generic;
using System.Linq;
using DataDefinitions;
using NUnit.Framework;

namespace PersistenceCompatibilityTests
{
    using System.Data.SqlClient;
    using System.Threading.Tasks;
    using Common;

    [TestFixture]
    public class NHibernatePersistenceTests : TestRun
    {
        [SetUp]
        public async Task Setup()
        {
            var dropAllTables = @"IF  NOT EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE name = N'persistencetests')
                                  CREATE DATABASE [persistencetests]
                                    
                                  exec sp_MSforeachtable ""declare @name nvarchar(max); set @name = parsename('?', 1); exec sp_MSdropconstraints @name"";
                                  exec sp_MSforeachtable ""drop table ?""; ";

            using (var connection = new SqlConnection(NHibernateConnectionInfo.ConnectionString))
            {
                await connection.OpenAsync().ConfigureAwait(false);

                using (var dropAllTablesCommand = new SqlCommand(dropAllTables, connection))
                {
                    await dropAllTablesCommand.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            }    
        }

        [TestCaseSource(nameof(GenerateTestCases))]
        public void can_fetch_by_correlation_property(string sourceVersion, string destinationVersion)
        {
            var sourcePersister = CreatePersister(sourceVersion);
            var destinationPersister = CreatePersister(destinationVersion);

            var writeData = new TestSagaData
            {
                Id = Guid.NewGuid(),
                Originator = "test-originator"
            };

            sourcePersister.Save(writeData, nameof(writeData.Originator), writeData.Originator);

            var readByCorrelationProperty = destinationPersister.GetByCorrelationId<TestSagaData>(nameof(writeData.Originator), writeData.Originator);

            Assert.AreEqual(writeData.Id, readByCorrelationProperty.Id);
            Assert.AreEqual(writeData.Originator, readByCorrelationProperty.Originator);
        }

        [TestCaseSource(nameof(GenerateTestCases))]
        public void can_fetch_simple_saga_persisted_by_another_version(string sourceVersion, string destinationVersion)
        {
            var sourcePersister = CreatePersister(sourceVersion);
            var destinationPersister = CreatePersister(destinationVersion);

            var writeData = new TestSagaData
            {
                Id = Guid.NewGuid(),
                Originator = "test-originator"
            };

            sourcePersister.Save(writeData, nameof(writeData.Originator), writeData.Originator);

            var readByGuid = destinationPersister.Get<TestSagaData>(writeData.Id);

            Assert.AreEqual(writeData.Id, readByGuid.Id);
            Assert.AreEqual(writeData.Originator, readByGuid.Originator);
        }

        [TestCaseSource(nameof(GenerateTestCases))]
        public void can_fetch_saga_with_list_persisted_by_another_version(string sourceVersion, string destinationVersion)
        {
            var sourcePersister = CreatePersister(sourceVersion);
            var destinationPersister = CreatePersister(destinationVersion);

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
            var sourcePersister = CreatePersister(sourceVersion);
            var destinationPersister = CreatePersister(destinationVersion);

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