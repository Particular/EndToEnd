using System;

namespace Common
{
    public class AppDomainDescriptor : IDisposable
    {
        public AppDomain AppDomain { get; set; }
        public string ProjectAssemblyPath { get; set; }
        public string NugetDownloadPath { get; set; }
        public string PackageVersion { get; set; }

        public void Dispose()
        {
            try
            {
                AppDomain.Unload(AppDomain);
            }
            catch (Exception ex)
            {
                throw new Exception($"Could not unload test appdomain {ProjectAssemblyPath}", ex);
            }
        }
    }
}