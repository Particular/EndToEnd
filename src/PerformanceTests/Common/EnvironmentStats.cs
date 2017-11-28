using System;
using System.Linq;
using System.Net;
using NServiceBus.Logging;

static class EnvironmentStats
{
    public static void Write()
    {
        var log = LogManager.GetLogger("GCSettings");
        log.InfoFormat("IsServerGC: {0}", System.Runtime.GCSettings.IsServerGC);
        log.InfoFormat("LatencyMode: {0}", System.Runtime.GCSettings.LatencyMode);

        log = LogManager.GetLogger("Environment");
        log.InfoFormat("ProcessorCount: {0}", Environment.ProcessorCount);
        log.InfoFormat("64bit: {0}", Environment.Is64BitProcess);
        log.InfoFormat("Version: {0}", Environment.Version);
        log.InfoFormat("OSVersion: {0}", Environment.OSVersion);
        log.InfoFormat("MachineName: {0}", Environment.MachineName);
        log.InfoFormat("WorkingSet: {0}", Environment.WorkingSet);
        log.InfoFormat("SystemPageSize: {0}", Environment.SystemPageSize);
        log.InfoFormat("HostName: {0}", Dns.GetHostName());
        foreach (var key in Environment.GetEnvironmentVariables().Keys.OfType<string>().Where(x => x.StartsWith("PROCESSOR_")))
        {
            log.InfoFormat("{0}: {1}", key, Environment.GetEnvironmentVariable(key));
        }

        log = LogManager.GetLogger("ServicePointManager");
        log.InfoFormat("DefaultConnectionLimit: {0}", ServicePointManager.DefaultConnectionLimit);
        log.InfoFormat("DefaultNonPersistentConnectionLimit: {0}", ServicePointManager.DefaultNonPersistentConnectionLimit);
        log.InfoFormat("EnableDnsRoundRobin: {0}", ServicePointManager.EnableDnsRoundRobin);
        log.InfoFormat("Expect100Continue: {0}", ServicePointManager.Expect100Continue);
        log.InfoFormat("UseNagleAlgorithm: {0}", ServicePointManager.UseNagleAlgorithm);

        log = LogManager.GetLogger("ENVVAR");
        foreach (var item in Environment.GetEnvironmentVariables().Keys.OfType<string>().OrderBy(x => x))
        {
            log.DebugFormat("- {0}", item);
        }
    }
}
