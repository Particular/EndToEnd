using System.Collections.Generic;

public class DataBusVerifier
{
    public static void AssertExpectations()
    {
        foreach (var endpointName in EndpointNames.All)
        {
            ResponseReceivedFromSites.VerifyContains(endpointName, $"{TestRunner.EndpointName} expected to receive a send from site {endpointName}");
            SendReceivedFromSites.VerifyContains(endpointName, $"{TestRunner.EndpointName} expected to receive a response from site {endpointName}");
        }
    }

    public static List<string> SendReceivedFromSites = new List<string>();
    public static List<string> ResponseReceivedFromSites = new List<string>();
}