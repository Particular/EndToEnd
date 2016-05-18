namespace Tests.CompatibilityTests
{
    using System;
    using Common.Runner.AppDomains;
    using Common.Runner.Nuget;
    using Common.Tests;

    public class TestInfo
    {
        public enum TestType
        {
            Serialization,
            Deserialization
        }

        public Package RunPackage { get; set; }
        public string CheckVersion { get; set; }
        public Type TestCase { get; set; }
        public TestType Type { get; set; }
        public SerializationFormat Format { get; set; }
        public AppDomainRunner Runner { get; set; }

        public override string ToString()
        {
            if (Type == TestType.Serialization)
            {
                return $"Serialization_{TestCase.Name}_{Format}_{RunPackage.Version}";
            }
            return $"Deserialization_{TestCase.Name}_{Format}_{CheckVersion}->{RunPackage.Version}";
        }
    }
}