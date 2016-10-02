using System;

namespace ArtifactsParser
{
    using System.Diagnostics;
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
            var src = Path.GetFullPath(path);
            Console.WriteLine("{0} => {1}", path, src);

            var csv = ScanLogs.ToCsvString(path);
            var dst = Path.Combine(src, $"report-{DateTime.Now:yyyy-MM-dd_hhmmss}.csv");
            File.WriteAllText(dst, csv);
            Console.WriteLine("Parsed '{0}' recursively and written to '{1}'", src, dst);

            Process.Start(dst);
        }
    }
}
