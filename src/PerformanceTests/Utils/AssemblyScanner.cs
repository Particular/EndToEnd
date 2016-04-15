using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using NLog;

public static class AssemblyScanner
{
    public static IEnumerable<Assembly> GetAssemblies()
    {
        var l = LogManager.GetLogger(nameof(AssemblyScanner));

        foreach (var path in Directory.EnumerateFiles(Environment.CurrentDirectory, "*.dll"))
        {
            Assembly.LoadFile(path);
        }

        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        foreach (var a in assemblies.OrderBy(a => a.ToString()))
        {
            string version,name;

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

            l.Info("Loaded: {0} ({1})", name, version);
        }

        return assemblies;


    }
}
