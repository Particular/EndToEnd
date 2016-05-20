using System;
using System.Collections.Generic;
using System.IO;
using Common;
using NUnit.Framework;

namespace PersistenceCompatibilityTests
{
    public abstract class TestRun
    {
        private static string BinDirectoryTemplate = "TestRun_{0}_{1}";

        protected AppDomainRunner<T> CreateTestFacade<T>(string versionName)
        {
            var packageInfo = new PackageInfo("NServiceBus.NHibernate.Tests", versionName);
            var package = packageResolver.GetLocalPackage(packageInfo);
            var appDomainDescriptor = domainCreator.CreateDomain(BinDirectoryTemplate, package, "NServiceBus.NHibernate");
            var runner = new AppDomainRunner<T>(appDomainDescriptor);

            appDomainDescriptors.Add(appDomainDescriptor);

            return runner;
        }

        [TearDown]
        public void CleanUp()
        {
            UnloadAppDomains();
            RemoveAppDomainCodeBaseDirs();
        }

        void RemoveAppDomainCodeBaseDirs()
        {
            foreach (var appDomainDescriptor in appDomainDescriptors)
            {
                var file = new FileInfo(appDomainDescriptor.ProjectAssemblyPath);
                file.Directory?.Delete(true);
            }
        }

        void UnloadAppDomains()
        {
            foreach (var appDomainDescriptor in appDomainDescriptors)
            {
                appDomainDescriptor.Dispose();
            }
        }

        [SetUp]
        public void Setup()
        {
            var path = AppDomain.CurrentDomain.BaseDirectory;
            appDomainDescriptors = new List<AppDomainDescriptor>();
            packageResolver = new LocalPackageResolver(path);
            domainCreator = new AppDomainCreator();
        }

        LocalPackageResolver packageResolver;
        AppDomainCreator domainCreator;
        IList<AppDomainDescriptor> appDomainDescriptors;

        //static string BinDirectorySearchPattern = string.Format(BinDirectoryTemplate, "*", "*");
    }
}