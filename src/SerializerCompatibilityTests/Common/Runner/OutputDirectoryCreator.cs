namespace Common.Runner
{
    using System;
    using System.IO;

    public static class OutputDirectoryCreator
    {
        public static string SetupOutputDirectory(string name)
        {
            var outputDirectory = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, name);

            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            //if (Directory.Exists(outputDirectory))
            //{
            //    Directory.Delete(outputDirectory, true);
            //}

            //Directory.CreateDirectory(outputDirectory);

            return outputDirectory;
        }
    }
}