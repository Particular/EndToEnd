# Permutation execution #

Every test fixture can execute one or more scenarios with different permutations. Several classes were build to support:

- Create new scenarios easily
- Single execution flow for scenarios
- As clean scenarios as possible
- Share as much code among different NServiceBus versions
- Share different strategies like seeding, endpoint configuration, etc.

With the created classes, the focus is on the scenario itself, rather then on getting a scenario to work. At the time of writing we've identified two different scenario types, for which we've written base classes.

- Receiving messages during a test run
- Sending messages during a test run

## Base classes ##

In the picture below you can see the classes and interfaces created for running scenarios. A shared `Program` class was created for both NServiceBus 5 and NServiceBus 6, which will execute the `BaseRunner` class. Multiple scenarios inherit from this class. For more specialized scenarios the `LoopRunner` class was created, which also has various implementations. For adding additional features, the `ICreateSeedData` and `IConfigureUnicastBus` interfaces were created. All the classes and interfaces are explained, including their usage and how they work internally.

![UML Base Classes](https://raw.githubusercontent.com/Particular/EndToEnd/doco/docs/images/baseclasses.png)

----------

### BaseRunner

The base class `BaseRunner` implements the template method pattern, which means it provides a skeleton and defers certain steps to sub classes. The subclasses are our actual scenarios. The main reason this class exists is to handle simpler scenarios and additionally provide additional features through the use of interfaces.

Although the template method pattern officially makes use of abstract methods that have to be implemented, the `Start()` and `Stop()` methods were not used in every single scenario. It was decided to not mark them abstract but `protected virtual` instead.

The simplest scenario that uses the BaseRunner is as follows, although this example does not provide any functionality, without mentioned use of any interface. 

```
class ExampleRunner : BaseRunner
{
    protected override void Start(ISession session)
    {
        base.Start(session);
    }

    protected override void Stop()
    {
        base.Stop();
    }	
}
```
#### Type inclusion & exclusion
Messages are seeded before or during the test. In some circumstances there might be messages in queues that are not supposed to be there. To make sure these messages aren't processed and influence the actual performance tests, we ignore every message handler that is not part of the scenario. This way messages out of the scope of the scenario will still be processed and consume cpu & memory, but will be send off to the error queue. This way it is logged that incorrect messages were being processed.

The resulting code is for NServiceBus 6 in the method `GetTypesToExclude()` and for NServiceBus 5 in the method `GetTypesToInclude` which also uses the `GetTypesToExclude()` method.

### LoopRunner

A second base class `LoopRunner` also implements its own template method pattern to provide a continuous loop feature. During the entire test the loop will be executed and call `SendMessage(ISession session)` method asynchronously. 

```
class ExampleRunner : LoopRunner
{
    protected override async Task SendMessage(ISession session)
    {
        await session.Send(new ExampleMessage());
    }
}
```

#### Generic message handler
The `LoopRunner` class enforces scenarios to implement the `SendMessage()` method, as mentioned earlier. It expects this method to send or publish a message. When sending the messages during the test, they also need to be dispatched to and be processed by a handler. This was handled in the `LoopRunner` itself by a generic handler. This way the handler does not need to be implemented by each scenario itself. The handler needs to be implemented in the scenario in a nested class, as can be seen in [GatedPublishRunner](EndToEnd/src/PerformanceTests/Common/Scenarios/GatedPublish/GatedPublishRunner.cs) and [GatedSendLocalRunner](EndToEnd/src/PerformanceTests/Common/Scenarios/GatedSendLocal/GatedSendLocalRunner.cs).  

## Interfaces ##

The following interfaces are provided

- [ICreateSeedData](EndToEnd/src/PerformanceTests/Common/Scenarios/ICreateSeedData.cs) to prepare & seed data
- [IConfigureUnicastBus](EndToEnd/src/PerformanceTests/Common/IConfigureUnicastBus.cs) to configure NServiceBus mappings


### ICreateSeedData
For scenarios that deal with just receiving data this interface will provide the functionality to prepare seeding data. For a specific amount of time the framework will start seeding data and call the `SendMessage()` abstract method, which the scenario developer will have to implement and send or publish the correct message.

##### Internal working
The `CreateSeedData()` method in `BaseRunner` executes the actual seeding. This method verifies if the `ICreateSeedData` interface is implemented and then starts a simple parallel loop. A `CancellationTokenSource` will automatically cancel the loop when done, with help from the `IterateUntilFalse` method. The seeding time can be site indirectly in application configuration with the `SeedDurationFactor` setting. It's a factor of the total time the test will run, excluding the warmup duration. So if `RunDuration` in configuration is 60 minuntes and the factor is `0.5` the seeding time will take half a minute.

An example of seeding can be found in [ReceiveRunner](EndToEnd/src/PerformanceTests/Common/Scenarios/Receive/ReceiveRunner.cs).

### IConfigureUnicastBus   
With this interface it is possible to provide settings that are usually configured in application configuration ([link](http://docs.particular.net/nservicebus/hosting/custom-configuration-providers)) with the `IConfigurationSource` interface.

An example can be found in [GatedPublishRunner](EndToEnd/src/PerformanceTests/Common/Scenarios/GatedPublish/GatedPublishRunner.cs).

##### Internal working
The BaseRunner itself implements the `IConfigurationSource` interface, but in the method `GetConfiguration<T>()` we now verify if the scenario implements the `IConfigureUnicastBus` interface. If it does we call the `GenerateMappings()` method on the scenario to provide the configuration. 

## NServiceBus abstraction ISession ##
Throughout the code various calls are made to the NServiceBus API which differs from version to version. For that we use prepocessor- or compiler directives ([link](https://msdn.microsoft.com/en-us/library/ed8yd1ha.aspx?f=255&MSPPError=-2147217396)) where needed, to distinguish code that is NServiceBus v5 and v6 related. For the scenarios we use partial classes to separate the code, but especially in `BaseRunner` there are portions of code between the compiler directives.

Sending messages (send, sendlocal & publish) is abstracted away behind the `ISession` interface. The console applications NServiceBus5 and NServiceBus6 have their own implementation of this interface. To provide developers with a way to send message, this interface is passed as parameter with abstract methods, so that the **intent** is clear the `ISession` interface is needed upon implementing these abstract methods.  

