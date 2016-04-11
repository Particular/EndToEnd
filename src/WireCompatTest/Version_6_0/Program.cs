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
        var endpointConfiguration = new EndpointConfiguration(endpointName);
        endpointConfiguration.Conventions().ApplyMessageConventions();
        endpointConfiguration.UseSerialization<JsonSerializer>();
        endpointConfiguration.UseTransport<MsmqTransport>();
        endpointConfiguration.UsePersistence<InMemoryPersistence>();
        endpointConfiguration.RijndaelEncryptionService();
        endpointConfiguration.UseDataBus<FileShareDataBus>().BasePath("..\\..\\..\\tempstorage");
        endpointConfiguration.ScaleOut().InstanceDiscriminator("1");
        endpointConfiguration.RegisterComponents(c => c.ConfigureComponent<EncryptionVerifier>(DependencyLifecycle.SingleInstance));
        endpointConfiguration.EnableInstallers();

        return Endpoint.Start(endpointConfiguration);
    }
}