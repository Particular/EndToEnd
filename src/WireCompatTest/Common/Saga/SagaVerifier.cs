using System.Collections.Generic;

public class SagaVerifier
{


    public static void AssertExpectations()
    {
        foreach (var endpointName in EndpointNames.All)
        {
            RequestingSagaGotTheResponse.VerifyContains(endpointName, $"{TestRunner.EndpointName} expected Requesting Saga Got The Response From {endpointName}");
        }
    }


    public static List<string> RequestingSagaGotTheResponse = new List<string>();

}