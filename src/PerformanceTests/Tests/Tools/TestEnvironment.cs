namespace Tests.Tools
{
    using System;
    using System.IO;
    using Tests.Permutations;
    using Variables;

    public class TestEnvironment
    {
        PermutationDirectoryResolver resolver;

        public TestEnvironment()
        {
            resolver = new PermutationDirectoryResolver(".");
        }

        public TestDescriptor CreateTestEnvironments(Permutation permutation)
        {
            var result = resolver.Resolve(permutation);
            var startupDir = GetStartupDir(permutation);

            if (startupDir.Exists)
            {
                startupDir.Delete(true);
            }

            startupDir.Create();

            var sourceAssemblyFiles = Directory.GetFiles(result.RootProjectDirectory, "*");
            CopyAssembliesToStarupDir(startupDir, sourceAssemblyFiles, result.Files);

            var projectAssemblyPath = Path.Combine(startupDir.FullName, result.RootProjectDirectory + "." + permutation.Platform + ".exe");

            var descriptor = new TestDescriptor
            {
                Permutation = permutation,
                ProjectAssemblyPath = projectAssemblyPath,
            };

            permutation.Exe = projectAssemblyPath;

            GenerateBat(descriptor);

            return descriptor;
        }

        void GenerateBat(TestDescriptor value)
        {
            var args = PermutationParser.ToArgs(value.Permutation);
            var exe = new FileInfo(value.ProjectAssemblyPath);

            var batFile = Path.Combine(exe.DirectoryName, "start.bat");

            if (!File.Exists(batFile)) File.WriteAllText(batFile, exe.Name + " " + args);
        }

        void CopyAssembliesToStarupDir(DirectoryInfo destination, string[] baseFiles, FileInfo[] overrides)
        {
            foreach (var file in baseFiles)
            {
                var dst = Path.Combine(destination.FullName, Path.GetFileName(file));
                Clone(file, dst);
            }

            foreach (var @override in overrides)
            {
                var dst = Path.Combine(destination.FullName, @override.Name);
                Clone(@override.FullName, dst);
            }
        }

        static void Clone(string src, string dst)
        {
            src = Path.GetFullPath(src);
            if (File.Exists(dst)) return;
            if (!SymbolicLink.Create(src, dst)) File.Copy(src, dst);
            File.SetLastWriteTimeUtc(dst, File.GetLastWriteTimeUtc(src));
        }

        static DirectoryInfo GetStartupDir(Permutation permutation)
        {
            var path = Path.Combine(
                "@",
                permutation.Category,
                string.Join("_", permutation.Tests),
                permutation.Id
                );

            return new DirectoryInfo(path);
        }
    }
}
