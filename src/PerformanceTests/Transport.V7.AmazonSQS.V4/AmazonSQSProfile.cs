using System;
using System.Data.Common;
using Amazon;
using Amazon.SQS;
using NServiceBus;
using Tests.Permutations;
using Variables;
using Amazon.Runtime;
using Amazon.S3;

class AmazonSQSProfile : IProfile, INeedPermutation
{
    public Permutation Permutation { private get; set; }

    public void Configure(EndpointConfiguration endpointConfiguration)
    {
        var cs = new DbConnectionStringBuilder
        {
            ConnectionString = ConfigurationHelper.GetConnectionString("AmazonSQS")
        };

        // https://docs.particular.net/transports/sqs/configuration-options
        var transport = endpointConfiguration.UseTransport<SqsTransport>();

        var credentials = new EnvironmentVariablesAWSCredentials();
        var region = RegionEndpoint.GetBySystemName(cs["Region"].ToString());
        transport.QueueNamePrefix(cs["QueueNamePrefix"].ToString());
        transport.MaxTimeToLive(TimeSpan.FromDays(Convert.ToDouble(cs["MaxTTLDays"].ToString())));
        transport.ClientFactory(() => new AmazonSQSClient(
            credentials,
            new AmazonSQSConfig
            {
                RegionEndpoint = region,
#if !NET452
                // dotnet core httpclient ignores DefaultConnectionLimit and uses int.MaxValue
                MaxConnectionsPerServer = Settings.DefaultConnectionLimit,
#endif
            }));
        var s3Configuration = transport.S3(cs["S3BucketForLargeMessages"].ToString(), cs["S3KeyPrefix"].ToString());
        s3Configuration.ClientFactory(() => new AmazonS3Client(
            credentials,
            new AmazonS3Config
            {
                RegionEndpoint = region,
#if !NET452
                // dotnet core httpclient ignores DefaultConnectionLimit and uses int.MaxValue
                MaxConnectionsPerServer = Settings.DefaultConnectionLimit
#endif
            }));

        // https://docs.particular.net/transports/sqs/transaction-support
        if (Permutation.TransactionMode != TransactionMode.Default
            && Permutation.TransactionMode != TransactionMode.None
            && Permutation.TransactionMode != TransactionMode.Receive
        ) throw new NotSupportedException("TransactionMode: " + Permutation.TransactionMode);

        if (Permutation.TransactionMode != TransactionMode.Default) transport.Transactions(Permutation.GetTransactionMode());
    }
}
