namespace ServiceControlCompatibilityTests
{
    using System;

    // TODO: This was mostly lifted from SC Management Engine. It's probably not needed and duplicating it could lead to issues later
    public static class SettingsList
    {
        public static SettingInfo Port = new SettingInfo { Name = "ServiceControl/Port" };
        public static SettingInfo HostName = new SettingInfo { Name = "ServiceControl/HostName" };
        public static SettingInfo LogPath = new SettingInfo { Name = "ServiceControl/LogPath" };
        public static SettingInfo DBPath = new SettingInfo { Name = "ServiceControl/DBPath" };
        public static SettingInfo ForwardAuditMessages = new SettingInfo { Name = "ServiceControl/ForwardAuditMessages" };
        public static SettingInfo ForwardErrorMessages = new SettingInfo { Name = "ServiceControl/ForwardErrorMessages", SupportedFrom = new Version(1, 11, 2) };
        public static SettingInfo TransportType = new SettingInfo { Name = "ServiceControl/TransportType" };
        public static SettingInfo AuditQueue = new SettingInfo { Name = "ServiceBus/AuditQueue" };
        public static SettingInfo ErrorQueue = new SettingInfo { Name = "ServiceBus/ErrorQueue" };
        public static SettingInfo ErrorLogQueue = new SettingInfo { Name = "ServiceBus/ErrorLogQueue" };
        public static SettingInfo AuditLogQueue = new SettingInfo { Name = "ServiceBus/AuditLogQueue" };
        public static SettingInfo AuditRetentionPeriod = new SettingInfo { Name = "ServiceControl/AuditRetentionPeriod", SupportedFrom = new Version(1, 12, 1) };
        public static SettingInfo ErrorRetentionPeriod = new SettingInfo { Name = "ServiceControl/ErrorRetentionPeriod", SupportedFrom = new Version(1, 12, 1) };
        public static SettingInfo EnableDTC = new SettingInfo { Name = "ServiceControl/EnableDTC", SupportedFrom = new Version(1, 19, 0) };
    }
}