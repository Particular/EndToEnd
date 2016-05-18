namespace Common.Runner.AppDomains
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    public class AssemblyLoader : MarshalByRefObject
    {
        // ReSharper disable once EmptyConstructor
        public AssemblyLoader()
        {
#if NCRUNCH
    // This code isn't needed for other test runners or NCrunch with the
    // 'Copy Referenced Assemblies To Workspace' setting enabled, because they
    // will have the ReferencedProject.dll sitting in the current directory.
    // see https://www.ncrunch.net/documentation/troubleshooting_tests-that-build-their-own-appdomains

            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
#endif
        }

        public void LoadAssembly(string assemblyFile)
        {
            try
            {
                Assembly.LoadFrom(assemblyFile);
            }
            catch (FileNotFoundException)
            {
                // continue loading no matter what
            }
        }

        public IEnumerable<AssemblyName> GetLoadedAssemblies()
        {
            return AppDomain.CurrentDomain.GetAssemblies().Select(assembly => assembly.GetName()).ToList();
        }

#if NCRUNCH
        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            // Search through the known assembly locations returned from the NCrunchEnvironment.GetAllAssemblyLocations method,
            // and load any assembly with a name matching the one we're looking for

            var shortAssemblyName = getShortAssemblyName(args.Name);

            foreach (var knownAssemblyLocation in NCrunchEnvironment.GetAllAssemblyLocations())
                if (string.Compare(Path.GetFileNameWithoutExtension(knownAssemblyLocation), shortAssemblyName, true) == 0)
                    return Assembly.LoadFrom(knownAssemblyLocation);

            return null;
        }

        private static string getShortAssemblyName(string assemblyName)
        {
            // The CLR can attempt to resolve assemblies using long names - so we truncate the name to make matching easier.

            if (assemblyName.Contains(","))
                return assemblyName.Substring(0, assemblyName.IndexOf(','));

            return assemblyName;
        }
#endif
    }
}