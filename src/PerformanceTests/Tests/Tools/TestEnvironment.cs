namespace Tests.Tools
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
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
            var components = resolver.Resolve(permutation);
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

            try
            {
                var componentsHostDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", components.HostDirectory);
                var sourceAssemblyFiles = Directory.GetFiles(componentsHostDirectory, "*", SearchOption.AllDirectories);
                CopyAssembliesToStarupDir(startupDir, sourceAssemblyFiles, components.Directories);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            

            var projectAssemblyPath = Path.Combine(startupDir.FullName, components.HostAssemblyName);

            var descriptor = new TestDescriptor
            {
                Permutation = permutation,
                ProjectAssemblyPath = projectAssemblyPath,
                ProjectAssemblyDirectory = startupDir.FullName,
                Category = permutation.Category,
                Description = permutation.Description,
            };

            GenerateBat(permutation, descriptor);
            UpdateAppConfig(descriptor);

            return descriptor;
        }

        void GenerateBat(Permutation permutation, TestDescriptor value)
        {
            var args = PermutationParser.ToArgs(value.Permutation);
            var sessionIdArgument = string.Format(" --sessionId={0}", sessionId);
            var executable = new FileInfo(value.ProjectAssemblyPath);
            var batFile = Path.Combine(executable.DirectoryName, "start.bat");

            if (permutation.Platform == Platform.NetFramework)
            {
                if (!File.Exists(batFile)) File.WriteAllText(batFile, executable.Name + " " + args + " " + sessionIdArgument);
            }
            else if (permutation.Platform == Platform.NetCore)
            {
                if (!File.Exists(batFile)) File.WriteAllText(batFile, $"dotnet {executable.Name} {args} {sessionIdArgument}");
            }
        }

        void CopyAssembliesToStarupDir(DirectoryInfo destination, IEnumerable<string> baseFiles, IEnumerable<DirectoryInfo> dirs)
        {
            var maxRetryErrors = 100;
            foreach (var file in baseFiles)
            {
                var dst = Path.Combine(destination.FullName, Path.GetFileName(file));
                do
                {
                    try
                    {
                        Clone(file, dst);
                        break;
                    }
                    catch
                    {
                        if (--maxRetryErrors < 0) throw;
                        Thread.Sleep(100);
                    }
                } while (true);
            }

            foreach (var dir in dirs)
            {
                var files = dir.GetFiles("*", SearchOption.AllDirectories);
                foreach (var src in files)
                {
                    var relative = src.FullName.Substring(dir.FullName.Length + 1);
                    var dst = Path.Combine(destination.FullName, relative);
                    do
                    {
                        new FileInfo(dst).Directory.Create();
                        try
                        {
                            Clone(src.FullName, dst);
                            break;
                        }
                        catch
                        {
                            if (--maxRetryErrors < 0) throw;
                            Thread.Sleep(100);
                        }
                    } while (true);
                }
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
            if (File.Exists(dst))
            {
                var equalTimestamps = File.GetLastAccessTimeUtc(dst) == File.GetLastWriteTimeUtc(src);
                if (equalTimestamps) return;
                File.Delete(dst);
            }
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
