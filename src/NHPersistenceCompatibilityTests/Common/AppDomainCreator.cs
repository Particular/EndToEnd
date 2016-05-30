using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Common
{
    [Serializable]
    public class AppDomainCreator
    {
        public AppDomainDescriptor CreateDomain(Package package, string nugetPackageToInstall)
        {
            var startupDir = CreateStartupDir(package.Version, Guid.NewGuid());
            
            CopyAssembliesToStartupDir(startupDir, package.Files);
            InstallCorrectNugetVersion(startupDir, nugetPackageToInstall, package.Version);

            var appDomain = AppDomain.CreateDomain(
                $"{package.Info.AssemblyName} {package.Version}",
                null,
                new AppDomainSetup
                {
                    ApplicationBase = startupDir.FullName
                });

            SetupBindingRedirects(package, appDomain);

            appDomain.SetData("FriendlyName", appDomain.FriendlyName);

            return new AppDomainDescriptor
            {
                AppDomain = appDomain,
                ProjectAssemblyPath = Path.Combine(startupDir.FullName, package.Info.AssemblyName + ".dll"),
                NugetDownloadPath = Path.Combine(startupDir.FullName, "..", nugetPackageToInstall + package.Version),
                PackageVersion = package.Version
            };
        }

        void InstallCorrectNugetVersion(DirectoryInfo startupDir, string packageName, string version)
        {
            var downloadNugetLocation = Path.Combine(startupDir.FullName, "..", packageName + version);

            var helper = new NugetHelper();
            helper.DownloadPackageTo(packageName, version, downloadNugetLocation);

            var files = new DirectoryInfo(downloadNugetLocation).GetFiles("*.dll", SearchOption.AllDirectories)
                // Don't overwrite the core NServiceBus libraries, we're testing package compatability
                // so we don't want to change the core versions, just the downstream paackages.
                .Where(f => f.Name != "NServiceBus.Core.dll")
                .Where(f => f.Name != "NServiceBus.dll").ToArray();

            CopyAssembliesToStartupDir(startupDir, files);
        }

        void SetupBindingRedirects(Package package, AppDomain appDomain)
        {
            var assemblyRedirector = new CustomAssemblyLoader(appDomain);
            var assemblies = package.Files.Where(f => f.Extension == ".dll").ToList();

            foreach (var assembly in assemblies)
            {
                var assemblyName = AssemblyName.GetAssemblyName(assembly.FullName);
                var shortName = assemblyName.Name;

                assemblyRedirector.AddAssemblyRedirect(shortName);
            }
        }

        void CopyAssembliesToStartupDir(DirectoryInfo destination, FileInfo[] baseFiles)
        {
            foreach (var file in baseFiles)
            {
                var newFilename = Path.Combine(destination.FullName, file.Name);

                File.Copy(file.FullName, newFilename, true);
            }
        }

        DirectoryInfo CreateStartupDir(string version, Guid uniqueId)
        {
            var directoryName = $"TestRun_{version}_{uniqueId}";

            if (!Directory.Exists(directoryName))
            {
                return Directory.CreateDirectory(directoryName);
            }

            return new DirectoryInfo(directoryName);
        }
    }
}