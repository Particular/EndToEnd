# EndToEnd

This GitHub repository contains several Visual Studio solutions for end-to-end testing of NServiceBus.

## Goal
Contain automated and manual tests that are run to verify correctness, performance and/or stability of Particular Platform.
The tests are only subset of all verification steps that are done on various steps of platform development process.

## Performance tests
With the performance tests it's possible to measure the performance of features within NServiceBus.

More detailed information on performance tests can be found in the [documentation](src/PerformanceTests/readme.md).

## Transport compatibility tests
The [Visual Studio solution](EndToEnd/src/TransportCompatibilityTests/TransportCompatibilityTests.sln) provides a framework and tests to verify sending messages over different transport versions. Up until now it's only possible to send messages over the same transport type, so MSMQ to MSMQ.

### Required configuration

#### Amazon SQS

The SQS tests are hard-coded to run in `us-east-1` region. In order to run them, environment variables containing security credentials need to be configured: `AWS_ACCESS_KEY_ID` and `AWS_SECRET_ACCESS_KEY`.

## Persistence compatibility tests

The [Persistence Compatibility Tests](https://github.com/Particular/EndToEnd/tree/master/src/PersistenceCompatibilityTests) verify that information can be saved and read by various versions of the available [NServiceBus persistences](http://docs.particular.net/nservicebus/persistence/). Those properties are verified on public API interfaces.

The tests automatically retrieve all minor versions of Persistence packages within a given range from NuGet and MyGet feeds. The same test cases are run against all the versions ensuring data from one version is compatible with the other. This ensures both forward and backward compatibility.

### Tested versions:
- NHibernate - versions 4.5 - 7.x

### Test cases:
- Saga persistence - testing `ISagaPersistance` interface.
  - Saga data persisted in version X, can be read correctly in version Y. Complex saga data structures like nested properties and collections are also tested.
  - Saga data persisted and updated in version X, can be read correctly in version Y. Complex saga data structures like nested properties and collections are also tested.
  - The data of the saga completed in version X, cannot be accessed in version Y.
  
### Pre-requisites for running tests locally
- NHibernate tests - a local SQLExpress instance, with database `PersistenceTests`
- Being able to resolve and download packages from official Nuget feed and MyGet.
