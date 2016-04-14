using System;
using System.Configuration;

static class Settings
{
    public static TimeSpan WarmupDuration = TimeSpan.Parse(ConfigurationManager.AppSettings["WarmupDuration"]);
    public static TimeSpan RunDuration = TimeSpan.Parse(ConfigurationManager.AppSettings["RunDuration"]);
}
