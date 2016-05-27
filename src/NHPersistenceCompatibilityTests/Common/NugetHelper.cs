using NuGet;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Common
{
    public class NugetHelper
    {
        string packageSource;
        string fallbackPackageSource;

        public NugetHelper(string packageSource = "https://packages.nuget.org/api/v2", string fallbackPackageSource = "https://www.myget.org/F/particular/")
        {
            // Todo: Load this package source from the machine config
            // Todo: Support multiple package sources (e.g. Build server with Myget and Nuget)
            this.packageSource = packageSource;
            this.fallbackPackageSource = fallbackPackageSource;
        }

        public IEnumerable<string> GetPossibleVersionsFor(string packageName, string minimumVersion)
        {
            var repo = PackageRepositoryFactory.Default.CreateRepository(packageSource);
            var packages = repo.FindPackagesById(packageName)
                .Where(p => p.IsListed())
                .Where(p => p.Version.CompareTo(SemanticVersion.Parse(minimumVersion)) >= 0);

            return packages.Select(p => p.Version.ToString());
        }

        internal void DownloadPackageTo(string packageName, string version, string location)
        {
            try
            {
                InstallPackageFromSource(packageSource, packageName, version, location);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Can't install {packageName}-{version} from {location}");
                Console.WriteLine(e.Message);

                InstallPackageFromSource(fallbackPackageSource, packageName, version, location);
            }
        }

        void InstallPackageFromSource(string source, string packageName, string version, string location)
        {
            var repo = PackageRepositoryFactory.Default.CreateRepository(source);
            var packageManager = new PackageManager(repo, location);

            packageManager.InstallPackage(packageName, SemanticVersion.Parse(version), false, true);
        }
    }
}
