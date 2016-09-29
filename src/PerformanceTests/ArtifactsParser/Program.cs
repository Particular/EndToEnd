using System;

namespace ArtifactsParser
{
    using System.IO;

    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Please specify artifact path as commandline argument\n\n\tArtifactParser.exe <path to artifacsts>");
                return;
            }
            var path = args[0];
            var csv = ScanLogs.ToCsvString(path);
            var dst = Path.Combine(path, "report.csv");
            File.WriteAllText(dst, csv);
            Console.WriteLine("Parsed '{0}' recursively and written to '{0}'", path, dst);
        }
    }
}
