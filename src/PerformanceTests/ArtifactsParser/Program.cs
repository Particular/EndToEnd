using System;

namespace ArtifactsParser
{
    using System.IO;

    class Program
    {
        static void Main(string[] args)
        {
            var path = args[0];
            var csv = ScanLogs.ToCsvString(path);
            var dst = Path.Combine(path, "report.csv");
            File.WriteAllText(dst, csv);
            Console.WriteLine("Parsed '{0}' recursively and written to '{0}'", path, dst);
        }
    }
}
