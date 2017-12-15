# Running Performance Tests

All required Azure resources are located in the `PerfTests` resource group.

## Prerequisites

* Ensure the `perfteststorage` VM is started
  * Ensure it has it's DNS name configured to `perfteststorage2.eastus2.cloudapp.azure.com`


## Running on Ubuntu

* Start the `perftestubuntu17` VM.
* Connect to the machine via SSH (e.g. using Putty on Windows) using the `pertest` user. The password is stored in LastPass.
* Switch to the `~/src` folder an verify the connection strings in `.connectionsstrings.config` (note the file is hidden as it starts with a dot).
* Switch to the `EndToEnd` repository.
* clean the repository from build and test artifacts of previous tests by using `git clean -xdf`.
* use git to get the desired branch to test.
* switch to the performance tests subfolder located in `EndToEnd/src/PerformanceTests`.
* build the solution targeting .NET Core using `dotnet build -f netcoreapp2.0 -c Release` ** Ignore build errors related to projects that do not support .NET Core **
* switch to the tests project in the `Tests` subfolder.
* run the tests using `dotnet test -f netcoreapp2.0 -c Release --no-build`. You can optionally redirect the console output in a log file by appending `> logfile.txt` to the command.
* aggregate the test results by switching to the `PerformanceTests/ArtifactParser` directory and using the following command: `dotnet run -f netcoreapp2.0 -c Release ~/src/EndToEnd/src/PerformanceTests/bin/Release/@`.


## Running on Windows

* Start the `perftestwin2016` VM.
* Connect to the machine using RDP (e.g. via the Azure portal) using the `perftest` user. The password is stored in LastPass.
* Switch to the `src` folder an verify the connection strings in `.connectionsstrings.config` (note the file is hidden as it starts with a dot).
* Switch to the `EndToEnd` repository.
* clean the repository from build and test artifacts of previous tests by using `git clean -xdf`.
* use git to get the desired branch to test.
* switch to the performance tests subfolder located in `EndToEnd/src/PerformanceTests`.
* build the solution targeting .NET Core using `dotnet build -f netcoreapp2.0 -c Release` ** Ignore build errors related to projects that do not support .NET Core **
* switch to the tests project in the `Tests` subfolder.
* run the tests using `dotnet test -f netcoreapp2.0 -c Release --no-build`. You can optionally redirect the console output in a log file by appending `> logfile.txt` to the command.
* aggregate the test results by switching to the `PerformanceTests/ArtifactParser` directory and using the following command: `dotnet run -f netcoreapp2.0 -c Release "C:\Users\perftest\Documents\src\EndToEnd\src\PerformanceTests\bin\Release\@"`.

