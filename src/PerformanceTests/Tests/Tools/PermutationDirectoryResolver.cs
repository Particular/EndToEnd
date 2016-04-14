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
        static string[] assembliesToSkip = { "NServiceBus.Core.dll" };
        readonly string rootDirectory;

        public PermutationDirectoryResolver(string rootDirectory)
        {
            this.rootDirectory = rootDirectory;
        }

        public PermutationResult Resolve(Permutation permutation)
        {
            var components = GetPermutationComponents(permutation);
        
            var files = components
                .SelectMany(s => Directory.GetDirectories(rootDirectory, s + "*").Select(path => new DirectoryInfo(path)))
                .Where(di => di.Exists)
                .SelectMany(di =>
                {
                    return di.GetFiles("*.dll").Where(info => !Array.Exists(assembliesToSkip, f => f == info.Name)).ToArray();
                });

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
        }

        public class PermutationResult
        {
            public FileInfo[] Files { get; set; }
            public string RootProjectDirectory { get; set; }
        }
    }
}