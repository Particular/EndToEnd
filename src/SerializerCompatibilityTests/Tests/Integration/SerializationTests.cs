namespace Tests.Integration
{
    using System;
    using System.IO;
    using Common.Runner;
    using Common.Runner.AppDomains;
    using Common.Runner.Nuget;
    using Common.Tests;
    using Common.Tests.TestCases;
    using NUnit.Framework;

    [TestFixture]
    public class SerializationTests
    {
        AppDomainRunner CreateTaskRunner(PackageInfo packages)
        {
            var package = nuGetPackageResolver.DownloadPackageWithDependencies(packages).Result;
            var domain = domainCreator.CreateDomain(TestsGlobal.BinDirectoryTemplate, package);

            return new AppDomainRunner(domain);
        }

        [SetUp]
        public void SetUp()
        {
            var outputDirectory = new OutputDirectoryCreator().SetupOutputDirectory("TestCaseOutput");

            filePath = Path.Combine(outputDirectory, "testcase.dat");
            nuGetPackageResolver = new NuGetPackageResolver("../..");
            domainCreator = new AppDomainCreator();
        }

        [Test]
        public void Failing_test_case_throws_an_exception()
        {
            var runner = CreateTaskRunner("NServiceBus:5.2.5");

            runner.Run(t => t.Serialize(typeof(Failing), SerializationFormat.Xml, filePath));

            Assert.That(() => runner.Run(t => t.Verify(typeof(Failing), SerializationFormat.Xml, filePath)), Throws.TypeOf<Exception>());
        }

        [Test]
        public void Working_test_case_does_not_throw()
        {
            var runner = CreateTaskRunner("NServiceBus:5.2.5");

            runner.Run(t => t.Serialize(typeof(Passing), SerializationFormat.Json, filePath));

            Assert.That(() => runner.Run(t => t.Verify(typeof(Passing), SerializationFormat.Json, filePath)), Throws.Nothing);
        }

        NuGetPackageResolver nuGetPackageResolver;
        AppDomainCreator domainCreator;
        string filePath;
    }
}