using NuGet;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Configuration;

namespace Common
{
    public class NugetHelper
    {
        IEnumerable<string> packageSources;

        public NugetHelper()
        {
            LoadNugetSourcesFromConfig();
        }

        private void LoadNugetSourcesFromConfig()
        {
            var machineSettings = Settings.LoadDefaultSettings(new PhysicalFileSystem("C:\\"), null, null);
            var packageSourceProvider = new PackageSourceProvider(machineSettings);
            packageSources = packageSourceProvider.GetEnabledPackageSources()
                .OrderBy(source => source.IsOfficial)
                .Select(source => source.Source).ToList();
        }

        public IEnumerable<string> GetPossibleVersionsFor(string packageName, string minimumVersion)
        {
            var repo = PackageRepositoryFactory.Default.CreateRepository(packageSources.First());
            var packages = repo.FindPackagesById(packageName)
                .Where(p => p.IsListed())
                .Where(p => p.Version.CompareTo(SemanticVersion.Parse(minimumVersion)) >= 0);

            return packages.Select(p => p.Version.ToString());
        }

        internal void DownloadPackageTo(string packageName, string version, string location)
        {
            var packageSourceEnumerator = packageSources.GetEnumerator();

            while (packageSourceEnumerator.MoveNext())
            {
                try
                {
                    InstallPackageFromSource(packageSourceEnumerator.Current, packageName, version, location);
                    return;
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Can't install {packageName}-{version} from {location}");
                    Console.WriteLine(e.Message);
                }
            }

            throw new ConfigurationErrorsException($"Can't install {packageName}--{version} from any of the provided package stores");
        }

        void InstallPackageFromSource(string source, string packageName, string version, string location)
        {
            var repo = PackageRepositoryFactory.Default.CreateRepository(source);
            var packageManager = new PackageManager(repo, location);

            packageManager.InstallPackage(packageName, SemanticVersion.Parse(version), false, true);
        }
    }
}
