using System;
using System.Configuration;
using System.Globalization;

public static class Settings
{
    public static TimeSpan WarmupDuration = TimeSpan.Parse(ConfigurationManager.AppSettings["WarmupDuration"]);
    public static TimeSpan RunDuration = TimeSpan.Parse(ConfigurationManager.AppSettings["RunDuration"]);
    public static TimeSpan SeedDuration = TimeSpan.FromSeconds((RunDuration + WarmupDuration).TotalSeconds * Convert.ToDouble(ConfigurationManager.AppSettings["SeedDurationFactor"], CultureInfo.InvariantCulture));

    public const int DefaultConnectionLimit = 150;
}
