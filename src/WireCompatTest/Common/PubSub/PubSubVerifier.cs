using System.Collections.Generic;

public class PubSubVerifier
{

    public static void AssertExpectations()
    {
        foreach (var endpointName in EndpointNames.All)
        {
            Asserter.IsTrue(EventReceivedFrom.Contains(endpointName), $"{TestRunner.EndpointName} expected a event to be Received From {endpointName}");
        }
    }

    public static List<string> EventReceivedFrom = new List<string>();

}