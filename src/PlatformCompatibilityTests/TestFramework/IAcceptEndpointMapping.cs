namespace ServiceControlCompatibilityTests
{
    using System.Collections.Generic;

    public interface IAcceptEndpointMapping
    {
        void AcceptEndpointMapping(IDictionary<string, string> endpointMapping);
    }
}
