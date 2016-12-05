namespace Common.Runner.AppDomains
{
    using System;
    using System.IO;
    using System.Reflection;
    using Nuget;
    using NUnit.Framework;
    using Paket;

    [Serializable]
    public class AppDomainCreator
    {
        public AppDomainDescriptor CreateDomain(string startupDirTemplate, Package package)
        {
            var startupDir = CreateStartupDir(startupDirTemplate, package.Version, Guid.NewGuid());

            var sourceAssemblyDir = Path.Combine(TestContext.CurrentContext.TestDirectory, package.Info.PackageName + SemVer.Parse(package.Version).Major);
            var sourceAssemblyFiles = Directory.GetFiles(sourceAssemblyDir, "*");

            CopyAssembliesToStarupDir(startupDir, sourceAssemblyFiles, package.Files);

            var appDomain = AppDomain.CreateDomain(
                $"{package.Info.PackageName} {package.Version}",
                null,
                new AppDomainSetup
                {
                    ApplicationBase = startupDir.FullName
                });

            SetupBindingRedirects(package, appDomain);

            appDomain.SetData("FriendlyName", appDomain.FriendlyName);

            var projectAssemblyPath = Path.Combine(startupDir.FullName, Path.GetFileNameWithoutExtension(sourceAssemblyDir) + ".dll");

            return new AppDomainDescriptor
            {
                AppDomain = appDomain,
                ProjectAssemblyPath = projectAssemblyPath,
                PackageVersion = package.Version
            };
        }

        static void SetupBindingRedirects(Package package, AppDomain appDomain)
        {
            var assemblyRedirector = new CustomAssemblyLoader(appDomain);

            foreach (var assembly in package.Files)
            {
                var assemblyName = AssemblyName.GetAssemblyName(assembly.FullName);
                var token = assemblyName.GetPublicKeyToken();
                var shortName = assemblyName.Name;
                var version = assemblyName.Version;

                assemblyRedirector.AddAssemblyRedirect(shortName, version, token);
            }
        }

        void CopyAssembliesToStarupDir(DirectoryInfo destination, string[] baseFiles, FileInfo[] overrides)
        {
            foreach (var file in baseFiles)
            {
                var newFilename = Path.Combine(destination.FullName, Path.GetFileName(file));

                File.Copy(file, newFilename);
            }

            foreach (var @override in overrides)
            {
                @override.CopyTo(Path.Combine(destination.FullName, @override.Name), true);
            }
        }

        static DirectoryInfo CreateStartupDir(string codeBaseDirTemplate, string version, Guid uniqueId)
        {
            var directoryName = string.Format(codeBaseDirTemplate, version, uniqueId);
            var directoryPath = Path.Combine(TestContext.CurrentContext.TestDirectory, directoryName);

            if (Directory.Exists(directoryPath))
            {
                Directory.Delete(directoryPath, true);
            }

            Directory.CreateDirectory(directoryPath);

            return new DirectoryInfo(directoryPath);
        }
    }

    public class AppDomainDescriptor
    {
        public AppDomain AppDomain { get; set; }
        public string ProjectAssemblyPath { get; set; }
        public string PackageVersion { get; set; }
    }
}