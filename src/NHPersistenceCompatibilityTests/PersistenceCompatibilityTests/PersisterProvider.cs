using System;
using System.Collections.Generic;
using System.IO;
using Common;
using System.Linq;

namespace PersistenceCompatibilityTests
{
    public class PersisterProvider
    {
        public void Initialize(string testAssemblyName, string packageName, IEnumerable<string> packageVersions)
        {
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

            appDomainDescriptors = new List<AppDomainDescriptor>();
            cachedPersisterFacades = new Dictionary<string, PersisterFacade>();

            foreach (var version in packageVersions.Where(nhVersion => !cachedPersisterFacades.ContainsKey(nhVersion)))
            {
                var appDomain = CreateAppDomain(testAssemblyName, packageName, version);

                appDomainDescriptors.Add(appDomain);

                var runner = new AppDomainRunner<IRawPersister>(appDomain);
                var facade = new PersisterFacade(runner);

                cachedPersisterFacades.Add(version, facade);
            }
        }

        AppDomainDescriptor CreateAppDomain(string assemblyName, string packageName, string packageVersionNumber)
        {
            var path = AppDomain.CurrentDomain.BaseDirectory;
            var packageResolver = new LocalPackageResolver(path);
            var domainCreator = new AppDomainCreator();

            var packageInfo = new PackageInfo(assemblyName, packageVersionNumber);
            var package = packageResolver.GetLocalPackage(packageInfo);
            var appDomainDescriptor = domainCreator.CreateDomain(package, packageName);
           
            return appDomainDescriptor;
        }

        public void Dispose()
        {
            var nugetFolders = appDomainDescriptors.Select(descriptor => descriptor.NugetDownloadPath).Distinct();
            foreach (var nugetFolder in nugetFolders)
            {
                new DirectoryInfo(nugetFolder)?.Delete(true);
            }

            foreach (var appDomainDescriptor in appDomainDescriptors)
            {
                appDomainDescriptor.Dispose();

                new FileInfo(appDomainDescriptor.ProjectAssemblyPath)
                    .Directory
                    ?.Delete(true);
            }
        }

        public PersisterFacade Get(string version)
        {
            return cachedPersisterFacades[version];
        }

        Dictionary<string, PersisterFacade> cachedPersisterFacades;
        IList<AppDomainDescriptor> appDomainDescriptors;
    }
}