using System.Collections.Generic;

public class SendReplyVerifier
{
    public static void AssertExpectations()
    {
        foreach (var endpointName in EndpointNames.All)
        {
            FirstMessageReceivedFrom.VerifyContains(endpointName, $"{TestRunner.EndpointName} expected a FirstMessage to be Received From {endpointName}");
            SecondMessageReceivedFrom.VerifyContains(endpointName, $"{TestRunner.EndpointName} expected a SecondMessage to be Received From {endpointName}");
        }
    }

    public static List<string> FirstMessageReceivedFrom = new List<string>();
    public static List<string> SecondMessageReceivedFrom = new List<string>();
}