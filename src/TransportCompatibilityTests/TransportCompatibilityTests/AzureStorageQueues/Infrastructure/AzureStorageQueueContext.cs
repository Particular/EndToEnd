namespace TransportCompatibilityTests.AzureStorageQueues
{
    using System;
    using NUnit.Framework;
    using Common.AzureStorageQueues;

    [TestFixture]
    public abstract class AzureStorageQueuesContext
    {
        [SetUp]
        public void CommonSetUp()
        {
            if (string.IsNullOrWhiteSpace(AzureStorageQueuesConnectionStringBuilder.Build()))
            {
                throw new Exception($"Environment variable `{AzureStorageQueuesConnectionStringBuilder.EnvironmentVariable}` is required for Azure Service Bus connection string.");
            }
        }
    }
}
