using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Common
{
    [Serializable]
    public class AppDomainCreator
    {
        public AppDomainDescriptor CreateDomain(string startupDirTemplate, Package package)
        {
            var startupDir = CreateStartupDir(startupDirTemplate, package.Version, Guid.NewGuid());
            
            CopyAssembliesToStarupDir(startupDir, package.Files);

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
                PackageVersion = package.Version
            };
        }

        void SetupBindingRedirects(Package package, AppDomain appDomain)
        {
            var assemblyRedirector = new CustomAssemblyLoader(appDomain);
            var assemblies = package.Files.Where(f => f.Extension == ".dll").ToList();

            foreach (var assembly in assemblies)
            {
                var assemblyName = AssemblyName.GetAssemblyName(assembly.FullName);
                var token = assemblyName.GetPublicKeyToken();
                var shortName = assemblyName.Name;
                var version = assemblyName.Version;

                assemblyRedirector.AddAssemblyRedirect(shortName, version, token);
            }
        }

        void CopyAssembliesToStarupDir(DirectoryInfo destination, FileInfo[] baseFiles)
        {
            foreach (var file in baseFiles)
            {
                var newFilename = Path.Combine(destination.FullName, file.Name);

                File.Copy(file.FullName, newFilename);
            }
        }

        DirectoryInfo CreateStartupDir(string codeBaseDirTemplate, string version, Guid uniqueId)
        {
            var directoryName = string.Format(codeBaseDirTemplate, version, uniqueId);

            if (!Directory.Exists(directoryName))
            {
                return Directory.CreateDirectory(directoryName);
            }

            return new DirectoryInfo(directoryName);
        }
    }
}