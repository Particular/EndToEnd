namespace TransportCompatibilityTests.Common.SqlServer
{
    using System;

    [Serializable]
    public class SqlServerEndpointDefinition : EndpointDefinition
    {
        public override string TransportName => "SqlServer";
        public string Schema { get; set; }
        public MessageMapping[] Mappings { get; set; }
        public MessageMapping[] Publishers { get; set; }

        public SqlServerEndpointDefinition()
        {
            Mappings = new MessageMapping[0];
            Publishers = new MessageMapping[0];
        }
    }
}
