Performance Tests
===============

# Prerequisites for running tests

- Making sure that connection string to MS SQL DB is correct
- Creating empty database manually before first run


# Dependencies

In order to run all permutations you need:

- RavenDB
- SQL Server
- RabbitMQ
- Azure Service Bus
- Azure Storage Queue


All of these need a correct connection string. 


# Corflags.exe

In order to run the host as a 32 or 64 bit process the executable it patches using `corflags.exe` and we expect it here:

`%programFiles(x86)%\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.6.1 Tools\corflags.exe`


# How it all works

* Each persistence and transport has its own project. These projects target a specific major version for their two master dependencies (NServiceBus and the downstream package).
* Each NServiceBus major version has a host.
* The unit test project defines category fixture. Such a fixture defines a set of permutations.
* The permutations are generated as a NUnit data source.
* Then a test is run the permutation is generated to a folder.
* The generated permutation is invoked.
* The test waits until the run is finished or crashed but kills it if it takes too long.


