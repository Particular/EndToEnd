namespace ServiceControlCompatibilityTests
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Threading.Tasks;
    using NServiceBus;
    using NServiceBus.Transport.SQLServer;

    public class MultiInstanceSqlTransportDetails : SqlTransportDetails, IAcceptEndpointMapping
    {
        public MultiInstanceSqlTransportDetails(string defaultConnectionString) : base(defaultConnectionString) { }

        public void AcceptEndpointMapping(IDictionary<string, string> endpointMapping)
        {
            _endpointMapping = endpointMapping;
        }

        public MultiInstanceSqlTransportDetails WithServer(string tag, string serverConnectionString)
        {
            _connectionStringMapping[tag] = serverConnectionString;
            return this;
        }

        public override async Task Initialize()
        {
            Console.WriteLine("Creating databases");
            await Task.WhenAll(_connectionStringMapping.Values.Select(EnsureDatabaseExists)).ConfigureAwait(false);
            Console.WriteLine("Databases created");
        }

        static async Task EnsureDatabaseExists(string connectionString)
        {
            var builder = new SqlConnectionStringBuilder(connectionString);
            var databaseName = builder.InitialCatalog;
            builder.InitialCatalog = "master";

            Console.WriteLine($"Creating Database [{databaseName}]");

            SqlConnection connection = null;

            try
            {
                connection = new SqlConnection(builder.ToString());
                await connection.OpenAsync().ConfigureAwait(false);

                var cmd = new SqlCommand($"if not exists(select * from sys.databases where name = '{databaseName}') create database [{databaseName}]", connection);
                await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);

                Console.WriteLine($"[{databaseName}] Created");
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Exception creating database [{databaseName}]: {exception}");
            }
            finally
            {
                connection?.Close();
            }
        }

        public override void ApplyTo(Configuration configuration)
        {
            base.ApplyTo(configuration);
            configuration.AppSettings.Settings.Set(SettingsList.EnableDTC, true.ToString());
            foreach (var mapping in _endpointMapping)
            {
                configuration.ConnectionStrings.ConnectionStrings.Set($"NServiceBus/Transport/{mapping.Key}", GetConnectionString(mapping.Key));
            }
        }

        public override void ConfigureEndpoint(string endpointName, EndpointConfiguration endpointConfig)
        {
            endpointConfig.PurgeOnStartup(true);

            var transport = endpointConfig.UseTransport<SqlServerTransport>();

            transport.ConnectionString(GetConnectionString(endpointName));

#pragma warning disable 618
            transport.EnableLegacyMultiInstanceMode(async transportAddress =>
#pragma warning restore 618
            {
                var connection = new SqlConnection(GetConnectionString(transportAddress));
                await connection.OpenAsync().ConfigureAwait(false);
                return connection;
            });
        }

        string GetConnectionString(string endpointNameOrTransportAddress)
        {
            var endpointName = endpointNameOrTransportAddress.Replace("@[dbo]", "");

            var instanceConnectionString = connectionString;

            string instanceTag;
            if (!_endpointMapping.TryGetValue(endpointName, out instanceTag))
            {
                return instanceConnectionString;
            }

            if (_connectionStringMapping.TryGetValue(instanceTag, out instanceConnectionString))
            {
                return instanceConnectionString;
            }

            throw new Exception($"{endpointNameOrTransportAddress} gets converted to Endpoint {endpointName} which gets mapped to {instanceTag} but there is no corresponding connectionString for {instanceTag}");
        }

        IDictionary<string, string> _endpointMapping;
        IDictionary<string, string> _connectionStringMapping = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
    }
}