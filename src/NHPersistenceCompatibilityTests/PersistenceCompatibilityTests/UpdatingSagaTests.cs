namespace PersistenceCompatibilityTests
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Threading.Tasks;
    using Common;
    using DataDefinitions;
    using NUnit.Framework;

    [TestFixture]
    public class UpdatingSagaTests : TestRun
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
        public void can_fetch_updated_saga_by_correlation_property(string sourceVersion, string destinationVersion)
        {
            var sourcePersister = CreatePersister(sourceVersion);
            var destinationPersister = CreatePersister(destinationVersion);

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
            var sourcePersister = CreatePersister(sourceVersion);
            var destinationPersister = CreatePersister(destinationVersion);

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
            var sourcePersister = CreatePersister(sourceVersion);
            var destinationPersister = CreatePersister(destinationVersion);

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
            var sourcePersister = CreatePersister(sourceVersion);
            var destinationPersister = CreatePersister(destinationVersion);

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
            CollectionAssert.AreEqual("updated", readData.Composite.Value);
        }

        static object[][] GenerateTestCases()
        {
            var versions = new[] { "4.5", "5.0", "6.2", "7.0" };

            var cases = from va in versions
                        from vb in versions
                        select new object[] { va, vb };

            return cases.ToArray();
        }
    }
}
