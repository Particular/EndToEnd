# EndToEnd

This GitHub repository contains several Visual Studio solutions for end-to-end testing of NServiceBus.

## Performance tests
With the performance tests it's possible to measure the performance of features within NServiceBus.

More detailed information on performance tests can be found in the [documentation](EndToEnd/docs/performance-index.md).

## Transport compatibility tests
The [Visual Studio solution](EndToEnd/src/TransportCompatibilityTests/TransportCompatibilityTests.sln) provides a framework and tests to verify sending messages over different transport versions. Up until now it's only possible to send messages over the same transport type, so MSMQ to MSMQ.

## Serializer compatibility tests
More info soon...

## Wire compatibility tests
More info soon...
