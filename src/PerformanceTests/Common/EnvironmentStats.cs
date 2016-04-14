﻿using System;
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

        log = LogManager.GetLogger("ServicePointManager");
        log.InfoFormat("DefaultConnectionLimit: {0}", ServicePointManager.DefaultConnectionLimit);
        log.InfoFormat("DefaultNonPersistentConnectionLimit: {0}", ServicePointManager.DefaultNonPersistentConnectionLimit);
        log.InfoFormat("EnableDnsRoundRobin: {0}", ServicePointManager.EnableDnsRoundRobin);
        log.InfoFormat("Expect100Continue: {0}", ServicePointManager.Expect100Continue);
        log.InfoFormat("UseNagleAlgorithm: {0}", ServicePointManager.UseNagleAlgorithm);
    }
}