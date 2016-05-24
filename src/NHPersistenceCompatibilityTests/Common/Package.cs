using System;
using System.IO;

namespace Common
{
    public class Package
    {
        public PackageInfo Info { get; set; }

        public string Version { get; set; }

        public FileInfo[] Files { get; set; }
    }

    [Serializable]
    public class PackageInfo
    {
        public PackageInfo(string assemblyName, string version)
        {
            AssemblyName = assemblyName;
            Version = version;
        }

        public string Version { get; }
        public string AssemblyName { get; }

        public static implicit operator PackageInfo(string package)
        {
            var part = package.Split('_');
            if (part.Length == 2)
                return new PackageInfo(part[0], part[1]);

            throw new ArgumentException($"The value '{package}' is not in a correct format. Use 'AssemblyName_Version'.");
        }

        public string GetProjectTemplateVersion()
        {
            if (Version.StartsWith("4"))
            {
                return "4.5";
            }
            else if (Version.StartsWith("5"))
            {
                return "5.0";
            }
            else if (Version.StartsWith("6"))
            {
                return "6.2";
            }
            else if (Version.StartsWith("7"))
            {
                return "7.0";
            }

            return Version;
        }

        public override string ToString()
        {
            return $"{AssemblyName}_{Version}";
        }
    }
}