using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using log4net;

public static class AssemblyScanner
{
    public static IEnumerable<Assembly> GetAssemblies()
    {
        var l = LogManager.GetLogger(typeof(AssemblyScanner));

        foreach (var path in Directory.EnumerateFiles(Environment.CurrentDirectory, "*.dll"))
        {
            var assemblyName = Path.GetFileNameWithoutExtension(path);
            try
            {
                Assembly.Load(assemblyName);
            }
            catch (Exception e)
            {
                l.Debug($"Unable to load assembly {assemblyName}", e);
            }
        }

        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        foreach (var a in assemblies.OrderBy(a => a.ToString()))
        {
            string version, name;

            try
            {
                name = a.GetName().Name;
            }
            catch (Exception)
            {
                name = a.FullName;
            }

            try
            {
                version = FileVersionInfo
                    .GetVersionInfo(a.Location)
                    .ProductVersion;
            }
            catch (Exception ex)
            {
                version = ex.Message;
            }

            l.InfoFormat("Loaded: {0} ({1})", name, version);
        }

        return assemblies;
    }

    public static IEnumerable<Type> GetAllTypes<T>()
    {
        var assemblies = GetAssemblies();

        var type = typeof(T);
        var types = assemblies
            .SelectMany(GetTypes)
            .Where(p => type.IsAssignableFrom(p) && !p.IsAbstract && !p.IsInterface);

        return types;
    }

    public static IEnumerable<T> GetAll<T>()
    {
        return GetAllTypes<T>().Select(t => (T)Activator.CreateInstance(t));
    }

    static Type[] GetTypes(Assembly a)
    {
        try
        {
            return a.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types;
        }
    }

}
