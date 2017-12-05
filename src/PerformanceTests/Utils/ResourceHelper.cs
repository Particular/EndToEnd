using System;
using System.IO;
using System.Linq;
using System.Reflection;

public static class ResourceHelper
{
    public static string GetManifestResourceText(this Assembly assembly, string resourceName)
    {
        using (var s = assembly.GetManifestResourceStream(resourceName))
        {
            if (s == null) throw new ArgumentException($"Resource {resourceName} does not exist in assembly {assembly}.");
            using (var reader = new StreamReader(s))
            {
                return reader.ReadToEnd();
            }
        }
    }

    public static string GetManifestResourceTextThatEndsWith(string identifier)
    {
        var assembly = Assembly.GetCallingAssembly();
        var key = assembly.GetManifestResourceNames().First(x => x.EndsWith(identifier));
        return assembly.GetManifestResourceText(key);
    }
}