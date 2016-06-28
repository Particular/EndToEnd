﻿namespace ServiceControlCompatibilityTests
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Threading;

    class ServiceControlInstance
    {
        public ServiceControlInstance(string installFolder, string rootUri)
        {
            this.installFolder = installFolder;
            Api = new ServiceControlApi(rootUri);
        }

        public ServiceControlApi Api { get; }

        public void Start()
        {
            Console.WriteLine("Starting ServiceControl");

            RunServiceControlInstallers();

            var psi = new ProcessStartInfo
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = installFolder,
                FileName = Path.Combine(installFolder, "ServiceControl.exe"),
                Arguments = "-p"
            };

            process = Process.Start(psi);

            if (process == null)
            {
                throw new Exception("The process is null which means it hasn't actually started");
            }

            // TODO: This should probably by async and eventually give up
            var retryCount = 0;
            var maxRetries = 100;
            while (!Api.CheckIsAvailable() && retryCount++ < maxRetries)
            {
                Console.WriteLine("ServiceControl not available yet, waiting 200ms");
                Thread.Sleep(TimeSpan.FromMilliseconds(200));
            }

            if (retryCount >= maxRetries)
            {
                throw new ApplicationException($"Could not start Service Control after {maxRetries} attempts");
            }

            Console.WriteLine("Service Control successfully started");
        }

        private void RunServiceControlInstallers()
        {
            var psi = new ProcessStartInfo
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = installFolder,
                FileName = Path.Combine(installFolder, "ServiceControl.exe"),
                Arguments = "-s"
            };

            process = Process.Start(psi);
            process.WaitForExit();
        }

        public void Stop()
        {
            process?.Kill();
            process?.WaitForExit(5000);
        }

        string installFolder;
        Process process;
    }
}