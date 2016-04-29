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
        static string[] assembliesToSkip = { "NServiceBus.Core.dll" };

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

            var files = dirs.SelectMany(d => d.GetFiles("*.dll").Where(dll => !assembliesToSkip.Any(dllToSkip => string.Compare(dllToSkip, dll.Name, true) == 0))).ToArray();

            return new PermutationResult
            {
                RootProjectDirectory = GetRootDirectory(permutation.Version),
                Files = files.ToArray()
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
                default:
                    throw new ArgumentOutOfRangeException(nameof(version), version, null);
            }
        }

        IEnumerable<string> GetPermutationComponents(Permutation permutation)
        {
            yield return $"Persistence.{permutation.Version}.{permutation.Persister}";
            yield return $"Transport.{permutation.Version}.{permutation.Transport}";
            yield return $"Distribution.{permutation.Version}.{permutation.ScaleOut}";
        }

        public class PermutationResult
        {
            public FileInfo[] Files { get; set; }
            public string RootProjectDirectory { get; set; }
        }
    }
}