using System.Reflection;
using NServiceBus;

class Program
{
    static string endpointName = $"WireCompat{Assembly.GetExecutingAssembly().GetName().Name}";

    static void Main()
    {
        var bus = CreateBus();
        TestRunner.EndpointName = endpointName;
        TestRunner.RunTests(bus);
    }

    static IBus CreateBus()
    {
        var busConfiguration = new BusConfiguration();
        busConfiguration.EndpointName(endpointName);
        busConfiguration.Conventions().ApplyMessageConventions();
        busConfiguration.UseSerialization<JsonSerializer>();
        busConfiguration.UseTransport<MsmqTransport>();
        busConfiguration.UsePersistence<InMemoryPersistence>();
        busConfiguration.RijndaelEncryptionService();
#if (Version6)
        busConfiguration.UseDataBus<FileShareDataBus>().BasePath("..\\..\\..\\tempstorage");
#else
#pragma warning disable 618
        busConfiguration.FileShareDataBus("..\\..\\..\\tempstorage");
#pragma warning restore 618
#endif
        busConfiguration.RegisterComponents(
            components =>
            {
                components.ConfigureComponent<EncryptionVerifier>(DependencyLifecycle.SingleInstance);
            });
        busConfiguration.EnableInstallers();
        var startableBus = Bus.Create(busConfiguration);
        return startableBus.Start();
    }

}