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
                var platformDirectory = GetTargetFrameworkName(permutation);
                dirs = dirs.Select(d => d.GetDirectories(platformDirectory).Single());
            }

            return new PermutationResult
            {
                HostDirectory = GetHostDirectory(permutation),
                HostAssemblyName = GetHostName(permutation),
                Directories = dirs.ToArray()
            };
        }

        static string GetTargetFrameworkName(Permutation permutation)
        {
            switch (permutation.Platform)
            {
                case Platform.NetFramework:
                    return "net452";
                case Platform.NetCore:
                    return "netcoreapp2.0";
                default:
                    throw new NotSupportedException();
            }
        }

        string GetHostName(Permutation permutation)
        {
            switch (permutation.Version)
            {
                case NServiceBusVersion.V5:
                    return "NServiceBus5";
                case NServiceBusVersion.V6:
                    return "NServiceBus6";
                case NServiceBusVersion.V7:
                    return "NServiceBus7";
                default:
                    throw new NotSupportedException(permutation.Version.ToString("G"));
            }
        }

        string GetHostDirectory(Permutation permutation)
        {
            switch (permutation.Version)
            {
                case NServiceBusVersion.V5:
                case NServiceBusVersion.V6:
                    return GetHostName(permutation);
                case NServiceBusVersion.V7:
                    return Path.Combine(GetHostName(permutation), GetTargetFrameworkName(permutation));
                default:
                    throw new NotSupportedException(permutation.Version.ToString("G"));
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
            public string HostDirectory { get; set; }
            public string HostAssemblyName { get; set; }
        }
    }
}
