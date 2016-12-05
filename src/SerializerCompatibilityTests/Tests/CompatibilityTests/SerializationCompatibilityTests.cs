namespace Tests.CompatibilityTests
{
    using System.IO;
    using Common.Runner;
    using Common.Tests;
    using NUnit.Framework;

    [TestFixture]
    [Category("All")]
    public class SerializationCompatibilityTests
    {
        [OneTimeSetUp]
        public void Setup()
        {
            outputDirectory = OutputDirectoryCreator.SetupOutputDirectory("SerializationCompatibilityFiles");
        }

        string CreateOutputFilePath(TestInfo testCaseInfo)
        {
            var nsbVersion = testCaseInfo.RunPackage.Version;
            var outputFileName = new TestCaseFileName(nsbVersion, testCaseInfo.TestCase.Name, testCaseInfo.Format);
            return Path.Combine(outputDirectory, outputFileName.ToString());
        }

        string CreateInputFilePath(TestInfo testCaseInfo)
        {
            var nsbVersion = testCaseInfo.CheckVersion;
            var inputFileName = new TestCaseFileName(nsbVersion, testCaseInfo.TestCase.Name, testCaseInfo.Format);
            return Path.Combine(outputDirectory, inputFileName.ToString());
        }

        [Test, TestCaseSource(typeof(TestInfoGenerator), "Generate", Category = "SerializerCompatibility")]
        public void Test(TestInfo testInfo)
        {
            if (testInfo.Type == TestInfo.TestType.Serialization)
            {
                var outputFilePath = CreateOutputFilePath(testInfo);

                testInfo.Runner.Run(tester => tester.Serialize(testInfo.TestCase, testInfo.Format, outputFilePath));
            }

            if (testInfo.Type == TestInfo.TestType.Deserialization)
            {
                var inputFilePath = CreateInputFilePath(testInfo);

                testInfo.Runner.Run(tester => tester.Verify(testInfo.TestCase, testInfo.Format, inputFilePath));
            }
        }

        string outputDirectory;
    }
}