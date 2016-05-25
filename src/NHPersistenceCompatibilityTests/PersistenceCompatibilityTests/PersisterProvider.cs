using System;
using System.Collections.Generic;
using System.IO;
using Common;
using System.Linq;

namespace PersistenceCompatibilityTests
{
    public class PersisterProvider
    {
        // Todo: This should probably not be hardcoded to NHibernate. This class should be reusable
        // when testing different persisters.
        public void Initialize(IEnumerable<string> nHibernatePackageVersions)
        {
            appDomainDescriptors = new List<AppDomainDescriptor>();
            cachedPersisterFacades = new Dictionary<string, PersisterFacade>();

            foreach (var version in nHibernatePackageVersions.Where(nhVersion => !cachedPersisterFacades.ContainsKey(nhVersion)))
            {
                var appDomain = CreateAppDomain(version);

                appDomainDescriptors.Add(appDomain);

                var runner = new AppDomainRunner<IRawPersister>(appDomain);
                var facade = new PersisterFacade(runner);

                cachedPersisterFacades.Add(version, facade);
            }
        }

        AppDomainDescriptor CreateAppDomain(string versionName)
        {
            var path = AppDomain.CurrentDomain.BaseDirectory;
            var packageResolver = new LocalPackageResolver(path);
            var domainCreator = new AppDomainCreator();

            var packageInfo = new PackageInfo("NServiceBus.NHibernate.Tests", versionName);
            var package = packageResolver.GetLocalPackage(packageInfo);
            var appDomainDescriptor = domainCreator.CreateDomain(package, "NServiceBus.NHibernate");
           
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