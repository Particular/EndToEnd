using System.Collections.Generic;

public class SendReturnVerifier
{

    public static void AssertExpectations()
    {
        foreach (var endpointName in EndpointNames.All)
        {
            ReplyReceivedFrom.VerifyContains(endpointName, $"{TestRunner.EndpointName} expected a reply to be Received From {endpointName}");
        }
    }

    public static List<string> ReplyReceivedFrom = new List<string>();
}