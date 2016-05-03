# Permutation profiles

To provide every permutation with the correct configuration, an assembly was created for everything related to NServiceBus that requires a unique NuGet package.

An example for this is the SQL Server transport where we have an assembly called `Transport.V5.SQLServer_v2` and an assembly called `Transport.V6.SQLServer_v3`. Where V5 and V6 in the name represents the NServiceBus version and v2 and v3 at the end represent the package version. The only exception to this is MSMQ, which is the only transport that is embedded in NServiceBus itself.

## IProfile interface

When running a scenario with the `BaseRunner` class, everything related to this test has been prepared by the [permutation generator](EndToEnd/docs/permutations.md). This means the permutation profile assemblies and their corresponding NuGet packages are in the folder where the test is run from. Using assembly and type scanning we find all classes implementing the `IProfile` interface.

This interface marks classes that there is configuration related to the permutation. This is especially useful for NServiceBus transports and persistence related packages. But it is also used for other permutation related settings, like concurrency and encryption, which are included in the `NServiceBus5` and `NServiceBus6` projects. When the profile is implemented, it will be called during creation of an endpoint.

#### INeedPermutation
When the `INeedPermutation` interface is included, the permutation will be provided, so that proper decisions can be made.

## Example
A good example is the MsmqProfile, which will be called when the permutation generator included this for the test. It will make sure journaling and the deadletter queue options are turned off in the Msmq connectionstring.

```
class MsmqProfile : IProfile
{
    public void Configure(BusConfiguration busConfiguration)
    {
        busConfiguration.UseTransport<MsmqTransport>()
            .ConnectionString("deadLetter=false;journal=false");
    }
}
```

## Exceptions
There are a few exceptions, which don't specify any configuration, but do verify assumptions on the configuration. These are

- CheckGCServer
- CheckPlatform

As the gcServer setting is set in application configuration by the permutation generator, it is verified in this profile that it is set correctly. The platform is set by the permutation generator using the `corflags.exe` tool provided by the WindowsSDK. It is used in `NServiceBus5` and `NServiceBus6` post build events. The file is copied with adding x86 to the name (instead of x64) and corflags is run. Using the permutation, the incorrect file is deleted. The profile verifies if the application is 32bit or 64bit and throws if this is incorrect.
