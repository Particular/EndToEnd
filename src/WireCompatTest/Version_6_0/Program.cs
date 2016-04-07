using System.Reflection;
using System.Threading.Tasks;
using NServiceBus;

class Program
{
    static string endpointName = "WireCompat" + Assembly.GetExecutingAssembly().GetName().Name;

    static void Main()
    {
        AsyncMain().GetAwaiter().GetResult();
    }

    static async Task AsyncMain()
    {
        var bus = await CreateBus();
        TestRunner.EndpointName = endpointName;
        await TestRunner.RunTests(bus);
    }

    static Task<IEndpointInstance> CreateBus()
    {
        var busConfiguration = new EndpointConfiguration(endpointName);
        busConfiguration.Conventions().ApplyMessageConventions();
        busConfiguration.UseSerialization<JsonSerializer>();
        busConfiguration.UseTransport<MsmqTransport>();
        busConfiguration.UsePersistence<InMemoryPersistence>();
        busConfiguration.RijndaelEncryptionService();
        busConfiguration.UseDataBus<FileShareDataBus>().BasePath("..\\..\\..\\tempstorage");
        busConfiguration.ScaleOut().InstanceDiscriminator("1");
        busConfiguration.RegisterComponents(c => c.ConfigureComponent<EncryptionVerifier>(DependencyLifecycle.SingleInstance));
        busConfiguration.EnableInstallers();

        return Endpoint.Start(busConfiguration);
    }
}