using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;

namespace Common
{
    [Serializable]
    public class CustomAssemblyLoader
    {
        public CustomAssemblyLoader(AppDomain appDomain)
        {
            this.appDomain = appDomain;
        }

        public void AddAssemblyRedirect(string shortName)
        {
            var resolver = new BindingRedirectResolver(appDomain, shortName);

            appDomain.AssemblyResolve += resolver.Resolve;
        }

        AppDomain appDomain;

        [Serializable]
        class BindingRedirectResolver
        {
            public BindingRedirectResolver(AppDomain domain, string shortName)
            {
                this.domain = domain;
                this.shortName = shortName;
            }

            public Assembly Resolve(object sender, ResolveEventArgs args)
            {
                // Use latest strong name & version when trying to load SDK assemblies
                var requestedAssembly = new AssemblyName(args.Name);
                if (requestedAssembly.Name != shortName)
                    return null;

                domain.AssemblyResolve -= Resolve;

                // load the assembly without a specific version - this is safe since these directories
                // are created at test time and we want to load the version that has been downloaded
                // from Nuget
                return Assembly.Load(requestedAssembly.Name);
            }

            AppDomain domain;
            string shortName;
        }
    }
}