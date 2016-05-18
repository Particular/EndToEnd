namespace Tests.Integration
{
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Common.Runner.Nuget;
    using NUnit.Framework;
    using Paket;

    [TestFixture]
    public class NuGetPackageResolverTests
    {
        [OneTimeSetUp]
        public void FixtureSetUp()
        {
            testPackagesDirectory = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "../.."));
        }

        [SetUp]
        public void Setup()
        {
            resolver = new NuGetPackageResolver(testPackagesDirectory);
        }

        [Test]
        public async Task Resolves_package_by_version()
        {
            var package = await resolver.DownloadPackageWithDependencies(new PackageInfo("NServiceBus", "[5.2.5]"));
            Assert.That(package.Files.Any(info => info.Name == "NServiceBus.Core.dll"));
        }

        [Test]
        public async Task Resolves_additional_assemblies()
        {
            var package = await resolver.DownloadPackageWithDependencies(new PackageInfo("NServiceBus", "(4.0,4.1)"));
            Assert.That(package.Files.Any(info => info.Name == "NServiceBus.dll"));
        }

        [Test]
        public async Task Gets_latest_package_version_for_major_minor()
        {
            var package = await resolver.DownloadPackageWithDependencies("NServiceBus:(5.2,5.3)");
            var actual = SemVer.Parse(package.Version);
            var expected = SemVer.Parse("5.2.5");
            Assert.That(actual, Is.GreaterThanOrEqualTo(expected));
        }

        string testPackagesDirectory;
        NuGetPackageResolver resolver;
    }
}