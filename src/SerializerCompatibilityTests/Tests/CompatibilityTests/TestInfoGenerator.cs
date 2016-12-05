namespace Tests.CompatibilityTests
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Common.Runner.AppDomains;
    using Common.Runner.Nuget;
    using Common.Tests;

    public class TestInfoGenerator
    {
        public static IEnumerable<TestInfo> Generate()
        {
            TestsGlobal.CleanupAfterPreviousRuns();

            var packages = DownloadPackages(PackageInfos);

            var runners = CreateAppDomainRunners(packages);

            var allTestCases = new TestCaseFinder().FindAll();

            var serializationCases = GenerateSerializationCases(allTestCases, packages, runners);
            var deserializationCases = GenerateDeserializationCases(allTestCases, packages, runners);

            var testCases = new List<TestInfo>(serializationCases);
            testCases.AddRange(deserializationCases);
            return testCases;
        }

        static Package[] DownloadPackages(PackageInfo[] packageInfos)
        {
            var resolver = new NuGetPackageResolver(PackageStore);
            var packages = new ConcurrentBag<Package>();

            foreach (var packageInfo in packageInfos)
            {
                var package = resolver.DownloadPackageWithDependencies(packageInfo).GetAwaiter().GetResult();

                packages.Add(package);
            }

            var uniquePackages = RemoveVersionDuplicates(packages.ToList());

            return uniquePackages;
        }

        static Package[] RemoveVersionDuplicates(List<Package> availableNServiceBusVersions)
        {
            //HINT: We want to remove packageinfos which resolved to the same version number. E.g.
            //      (5.2,5.3) and (5.0,6.0) could both resolve to 5.2.8 if that's the newest version.
            return availableNServiceBusVersions.Distinct(new PackageVersionComparer()).ToArray();
        }

        static Dictionary<string, AppDomainRunner> CreateAppDomainRunners(Package[] packages)
        {
            var appDomainCreator = new AppDomainCreator();
            var appDomainRunners = new ConcurrentBag<AppDomainRunner>();
            var tasks = new List<Task>();

            foreach (var package in packages)
            {
                var packageToUse = package;

                var task = Task.Factory.StartNew(() =>
                {
                    var domain = appDomainCreator.CreateDomain(TestsGlobal.BinDirectoryTemplate, packageToUse);
                    var runner = new AppDomainRunner(domain);

                    appDomainRunners.Add(runner);
                });

                tasks.Add(task);
            }

            Task.WaitAll(tasks.ToArray());

            return appDomainRunners.ToDictionary(i => i.PackageVersion, i => i);
        }

        static IEnumerable<TestInfo> GenerateDeserializationCases(List<TestCase> testCases, Package[] packages, Dictionary<string, AppDomainRunner> runners)
        {
            var testInfos = from t in testCases
                from rp in packages
                from cp in packages
                from sf in new[]
                {
                    SerializationFormat.Xml,
                    SerializationFormat.Json
                }
                where t.SupportsVersion(rp.Version) && t.SupportsVersion(cp.Version) && t.SupportsFormat(sf)
                select new TestInfo
                {
                    RunPackage = rp,
                    CheckVersion = cp.Version,
                    TestCase = t.GetType(),
                    Format = sf,
                    Type = TestInfo.TestType.Deserialization,
                    Runner = runners[rp.Version]
                };

            return testInfos;
        }

        static IEnumerable<TestInfo> GenerateSerializationCases(List<TestCase> testCases, Package[] packages, Dictionary<string, AppDomainRunner> runners)
        {
            var testInfos = from t in testCases
                from p in packages
                from sf in new[]
                {
                    SerializationFormat.Xml,
                    SerializationFormat.Json
                }
                where t.SupportsVersion(p.Version) && t.SupportsFormat(sf)
                select new TestInfo
                {
                    RunPackage = p,
                    TestCase = t.GetType(),
                    Format = sf,
                    Type = TestInfo.TestType.Serialization,
                    Runner = runners[p.Version]
                };

            return testInfos;
        }

        static string PackageStore = "../..";

        static PackageInfo[] PackageInfos =
        {
            // latest v3 (3.3)
            "NServiceBus:(3.0,4.0)",

            // v4 (4.0 - 4.7)
            "NServiceBus:(4.0,4.1)",
            "NServiceBus:(4.1,4.2)",
            "NServiceBus:(4.2,4.3)",
            "NServiceBus:(4.3,4.4)",
            "NServiceBus:(4.4,4.5)",
            "NServiceBus:(4.5,4.6)",
            "NServiceBus:(4.6,4.7)",
            "NServiceBus:(4.0,5.0)",

            // v5 (5.0 - 5.2)
            "NServiceBus:(5.0,5.1)",
            "NServiceBus:(5.1,5.2)",
            "NServiceBus:(5.2,5.3)",
            "NServiceBus:(5.0,6.0)",

            // v6
            "NServiceBus:(6.0-beta,6.1-beta)" //due to an issue in Paket, it has to be specified like this
        };
    }
}