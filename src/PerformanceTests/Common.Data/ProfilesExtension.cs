#if Version6
using Configuration = NServiceBus.EndpointConfiguration;
#else
using Configuration = NServiceBus.BusConfiguration;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using Tests.Permutations;
using Common;

static class ProfilesExtension
{
    public static void ApplyProfiles(this Configuration configuration, Permutation permutation)
    {
        foreach (var profile in GetProfiles())
        {
            var injectPermutation = profile as INeedPermutation;
            if (injectPermutation != null)
            {
                injectPermutation.Permutation = permutation;
            }
            profile.Configure(configuration);
        }
    }

    static IEnumerable<IProfile> GetProfiles()
    {
        var assemblies = AssemblyScanner.GetAssemblies();

        var type = typeof(IProfile);
        var types = assemblies
            .SelectMany(s => s.GetTypes())
            .Where(p => type.IsAssignableFrom(p) && !p.IsAbstract && !p.IsInterface);


        return types.Select(t => (IProfile)Activator.CreateInstance(t));
    }
}
