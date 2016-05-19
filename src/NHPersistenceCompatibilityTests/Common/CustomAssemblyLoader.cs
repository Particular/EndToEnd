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

        public void AddAssemblyRedirect(string shortName, Version targetVersion, byte[] publicKeyToken)
        {
            var resolver = new BindingRedirectResolver(appDomain, shortName, targetVersion, publicKeyToken);

            appDomain.AssemblyResolve += resolver.Resolve;
        }

        AppDomain appDomain;

        [Serializable]
        class BindingRedirectResolver
        {
            public BindingRedirectResolver(AppDomain domain, string shortName, Version targetVersion, byte[] publicKeyToken)
            {
                this.domain = domain;
                this.shortName = shortName;
                this.targetVersion = targetVersion;
                this.publicKeyToken = publicKeyToken;
            }

            public Assembly Resolve(object sender, ResolveEventArgs args)
            {
                // Use latest strong name & version when trying to load SDK assemblies
                var requestedAssembly = new AssemblyName(args.Name);
                if (requestedAssembly.Name != shortName)
                    return null;

                Trace.WriteLine($"Redirecting assembly load of {args.Name},\tloaded by " + (args.RequestingAssembly?.FullName ?? "(unknown)"));

                requestedAssembly.Version = targetVersion;
                requestedAssembly.SetPublicKeyToken(publicKeyToken);
                requestedAssembly.CultureInfo = CultureInfo.InvariantCulture;

                domain.AssemblyResolve -= Resolve;

                return Assembly.Load(requestedAssembly);
            }

            readonly AppDomain domain;
            byte[] publicKeyToken;
            string shortName;
            Version targetVersion;
        }
    }
}