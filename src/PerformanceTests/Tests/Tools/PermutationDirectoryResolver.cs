namespace Tests.Tools
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Tests.Permutations;
    using Variables;

    public class PermutationDirectoryResolver
    {
        readonly string rootDirectory;

        public PermutationDirectoryResolver(string rootDirectory)
        {
            this.rootDirectory = rootDirectory;
        }

        public PermutationResult Resolve(Permutation permutation)
        {
            var components = GetPermutationComponents(permutation);

            var root = new DirectoryInfo(rootDirectory);

            var dirs = root.GetDirectories()
                .Where(d => components.Any(c => d.Name.StartsWith(c)));

            if (permutation.Version == NServiceBusVersion.V7)
            {
                // v7 targeting projects are using the new csproj format
                dirs = dirs.Select(d => d.GetDirectories("net461").Single());
            }

            return new PermutationResult
            {
                RootProjectDirectory = GetRootDirectory(permutation.Version),
                Directories = dirs.ToArray()
            };
        }

        string GetRootDirectory(NServiceBusVersion version)
        {
            switch (version)
            {
                case NServiceBusVersion.V5:
                    return "NServiceBus5";
                case NServiceBusVersion.V6:
                    return "NServiceBus6";
                case NServiceBusVersion.V7:
                    return "NServiceBus7";
                default:
                    throw new ArgumentOutOfRangeException(nameof(version), version, null);
            }
        }

        string GetImplementation(object instance)
        {
            var value = instance.ToString();
            var index = value.IndexOf('_');

            if (index == -1) return value;

            return value.Substring(0, index);
        }

        IEnumerable<string> GetPermutationComponents(Permutation permutation)
        {
            var persister = GetImplementation(permutation.Persister);
            var transport = GetImplementation(permutation.Transport);

            yield return $"Persistence.{permutation.Version}.{persister}";
            yield return $"Transport.{permutation.Version}.{transport}";
            yield return $"Distribution.{permutation.Version}.{permutation.ScaleOut}";
        }

        public class PermutationResult
        {
            public DirectoryInfo[] Directories { get; set; }
            public string RootProjectDirectory { get; set; }
        }
    }
}
