namespace Tests.Tools
{
    using System.IO;
    using System.Xml.Linq;
    using System.Xml.XPath;
    using Tests.Permutations;
    using Variables;

    public class TestEnvironment
    {
        PermutationDirectoryResolver resolver;
        string sessionId;

        static TestEnvironment()
        {
            System.Environment.CurrentDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
        }

        public TestEnvironment(string sessionId)
        {
            resolver = new PermutationDirectoryResolver(".");
            this.sessionId = sessionId;
        }

        public TestDescriptor CreateTestEnvironments(Permutation permutation)
        {
            var result = resolver.Resolve(permutation);
            var startupDir = GetStartupDir(permutation);

            if (startupDir.Exists)
            {
                try
                {
                    startupDir.Delete(true);
                }
                catch
                {
                    foreach (var f in startupDir.GetFiles()) f.Delete();
                }
            }

            startupDir.Create();

            var sourceAssemblyFiles = Directory.GetFiles(result.RootProjectDirectory, "*");
            CopyAssembliesToStarupDir(startupDir, sourceAssemblyFiles, result.Files);

            var projectAssemblyPath = Path.Combine(startupDir.FullName, result.RootProjectDirectory + "." + permutation.Platform + ".exe");

            var descriptor = new TestDescriptor
            {
                Permutation = permutation,
                ProjectAssemblyPath = projectAssemblyPath,
                Category = permutation.Category,
                Description = permutation.Description,
            };

            permutation.Exe = projectAssemblyPath;

            GenerateBat(descriptor);
            UpdateAppConfig(descriptor);

            return descriptor;
        }

        void GenerateBat(TestDescriptor value)
        {
            var args = PermutationParser.ToArgs(value.Permutation);
            var sessionIdArgument = string.Format(" --sessionId={0}", sessionId);
            var exe = new FileInfo(value.ProjectAssemblyPath);

            var batFile = Path.Combine(exe.DirectoryName, "start.bat");

            if (!File.Exists(batFile)) File.WriteAllText(batFile, exe.Name + " " + args + " " + sessionIdArgument);
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

        void UpdateAppConfig(TestDescriptor value)
        {
            var x = value.Permutation.GarbageCollector == GarbageCollector.Client ? "false" : "true";

            var config = value.ProjectAssemblyPath + ".config";
            var doc = XDocument.Load(config);
            var enabled = doc
                .XPathSelectElement("/configuration/runtime/gcServer")
                .Attribute("enabled");

            if (enabled.Value == x) return;

            enabled.Value = x;

            File.Delete(config); // This makes sure that we do not update the symlink source!
            doc.Save(config, SaveOptions.None);
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
                permutation.Fixture,
                string.Join("_", permutation.Tests),
                permutation.Code.Replace(" ", "-")
                );

            return new DirectoryInfo(path);
        }
    }
}
