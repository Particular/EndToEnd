namespace ServiceControlCompatibilityTests
{
    using NUnit.Framework;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    abstract class SqlScTest
    {
        Dictionary<Type, Func<ITransportDetails>> transportDetailActivations = new Dictionary<Type, Func<ITransportDetails>>
        {
            { typeof(SqlTransportDetails), () => new SqlTransportDetails("Data Source=.\\SQLEXPRESS;Initial Catalog=nservicebus;Integrated Security=True") },
            { typeof(MsmqTransportDetails), () => new MsmqTransportDetails() },
            { typeof(RabbitMQTransportDetails), () => new RabbitMQTransportDetails("host=localhost") },
            { typeof(ASBForwardingTopologyTransportDetails), () => new ASBForwardingTopologyTransportDetails(Environment.GetEnvironmentVariable("AzureServiceBus.ConnectionString")) },
            { typeof(ASQTransportDetails), () => new ASQTransportDetails(Environment.GetEnvironmentVariable("AzureStorageQueueTransport.ConnectionString")) },
            {
                typeof(MultiInstanceSqlTransportDetails), () => 
                    new MultiInstanceSqlTransportDetails("Data Source=.\\SQLEXPRESS;Initial Catalog=nservicebus;Integrated Security=True")
                        .WithServer(ServerA, $"Data Source=.\\SQLEXPRESS;Initial Catalog={ServerA};Integrated Security=True")
                        .WithServer(ServerB, $"Data Source=.\\SQLEXPRESS;Initial Catalog={ServerB};Integrated Security=True")
            }
        };

        protected IEndpointFactory StartUp(string testName, Type transportDetailsType, Action<IDictionary<string, string>> fillInEndpointMappings = null)
        {
            var testId = $"{testName}_{transportDetailsType.Name.Replace("TransportDetails", "")}";
            Console.WriteLine($"Starting test {testId}");
            var transportDetails = ActivateInstanceOfTransportDetail(transportDetailsType);

            var endpointMappings = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
            fillInEndpointMappings?.Invoke(endpointMappings);

            (transportDetails as IAcceptEndpointMapping)?.AcceptEndpointMapping(endpointMappings);

            transportDetails.Initialize();
            
            serviceControl = StartServiceControl(testId, transportDetails);

            var endpointFactory = new EndpointFactory(transportDetails);

            return endpointFactory;
        }

        [TearDown]
        public void ShutDown()
        {
            // TODO: Clean up using the transportDetails
            serviceControl?.Stop();
        }

        ITransportDetails ActivateInstanceOfTransportDetail(Type transportDetailType)
        {
            var activator = transportDetailActivations[transportDetailType];

            return activator?.Invoke();
        }

        ServiceControlInstance StartServiceControl(string testId, ITransportDetails transport)
        {
            var runningInTeamCity = Environment.GetEnvironmentVariable("TEAMCITY_VERSION") != null;
            
            Console.WriteLine(runningInTeamCity ? "Running in TeamCity" : "Running outside of TeamCity");
            
            // TODO: If Not Running in TeamCity get this from a different Env variable?
            var serviceControlPath = runningInTeamCity 
                ? Path.Combine(Environment.CurrentDirectory, "ServiceControl")
                : @"C:\Temp\ServiceControl";

            Console.WriteLine($"Creating SC Factory at {serviceControlPath}");
            var factory = new ServiceControlFactory(serviceControlPath);

            return factory.Start(transport, testId);
        }

        protected static Type[] AllTransports()
        {
            var assemblyToLoad = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll");
            foreach (var path in assemblyToLoad)
            {
                AppDomain.CurrentDomain.Load(AssemblyName.GetAssemblyName(path));
            }

            var transports = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.GetInterfaces().Any(i => i == (typeof(ITransportDetails))));

            return transports.ToArray();
        }

        ServiceControlInstance serviceControl;
        protected ServiceControlApi ServiceControl => serviceControl.Api;

        protected const string ServerA = "ServerA";
        protected const string ServerB = "ServerB";
    }
}