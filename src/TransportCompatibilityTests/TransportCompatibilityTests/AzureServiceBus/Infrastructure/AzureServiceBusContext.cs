namespace TransportCompatibilityTests.AzureServiceBus
{
    using System;
    using NUnit.Framework;
    using Common.AzureServiceBus;

    [TestFixture]
    public abstract class AzureServiceBusContext
    {
        [SetUp]
        public void CommonSetup()
        {
            if (string.IsNullOrWhiteSpace(AzureServiceBusConnectionStringBuilder.Build()))
            {
                throw new Exception($"Environment variable `{AzureServiceBusConnectionStringBuilder.EnvironmentVariable}` is required for Azure Service Bus connection string.");
            }
        }
    }
}
