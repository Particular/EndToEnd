# How to add a scenario
This document describes how to add a new scenario. Installation of environments like SQL Server, RabbitMQ or a new transport or pesister are of course out of scope.

## Writing your own scenario
Currently there are two base classes which support easily adding a new scenario. These are the `BaseRunner` and `LoopRunner` classes in the Common project. These are described in more detail in the [permutation execution](EndToEnd/docs/permutation-execution.md) document. More information can be found there or in the implementing classes to get an idea on how they work. The `BaseRunner` as top class is needed as the tests are found and run based on it.

After selecting a base class, a new scenario folder and class should be provided. The ReceiveRunner has an accompanying `Receive` folder and corresponding `ReceiveRunner` class, implementing the `BaseRunner` class. In this example, it also required seeding data, for which is implements the `ICreateSeedData` interface. Read more on that in [permutation execution](EndToEnd/docs/permutation-execution.md).

If needed, override the `Start()` and/or `Stop()` methods. For the `LoopRunner` the method `SendMessage()` is required to be implemented, and likely also a nested Handler class needs to be implemented, inheriting the `Handler<T>` class.

### Adding new transports, persisters or other specific needs
Transports, persisters and other components with specific needs, also require a separate project where

- Required NuGet packages should be referenced, for example NServiceBus and/or other components needed for the test. This will make sure the components get copied into the final permutation folder.
- A reference to either NServiceBus5 or NSerivceBus6 solution project
- A class should implement the `IProfile` interface to provide configuration builder specific settings to the component
 

### Sending messages
When sending messages, during execution an implementation of the `ISession` interface is provided. This can be used to send or publish messages.

## Added the test to a (new) permutation
To be able to execute the scenario, it has to be added to a testfixture. All the fixtures can be found in the project `Tests` and based on the context, the scenario should be added to an existing fixture or a new fixture has to be created. If tests are provided for a specific transport, the scenario obviously needs to go into the `TransportsFixture` class. At the bottom of each of these classes the `CreatePermutations` method can be found, which describes which permutations the scenario is executed against. New scenarios should obviously also call the base class `Base` and the scenario should be added there as well. 
