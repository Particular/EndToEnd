using System;
using System.Configuration;
using NLog;

public static class ConfigurationHelper
{
    static readonly ILogger Log = LogManager.GetLogger(nameof(ConfigurationHelper));

    public static string GetConnectionString(string connectionStringName)
    {
        var environmentVariableConnectionString = Environment.GetEnvironmentVariable(connectionStringName);
        if (!string.IsNullOrWhiteSpace(environmentVariableConnectionString))
        {
            Log.Info("Environment variable found.");
            Log.Debug("Connection string: {0}", environmentVariableConnectionString);
            return environmentVariableConnectionString;
        }

        var applicationConfigConnectionString = ConfigurationManager.ConnectionStrings[connectionStringName];
        if (applicationConfigConnectionString != null)
        {
            var cs = applicationConfigConnectionString.ConnectionString;
            Log.Info("App.config connection string variable found.");
            Log.Debug("Connection string: {0}", cs);
            return cs;
        }

        throw new NotSupportedException($"Connection string {connectionStringName} could not be resolved.");
    }

    public static string FetchSetting(string key)
    {
        string value;
        if (TryGetEnvVar(key, out value)) return value;
        if (TryGetAppSetting(key, out value)) return value;
        return null;
    }

    static bool TryGetAppSetting(string key, out string value)
    {
        value = ConfigurationManager.AppSettings[key];
        if (string.IsNullOrWhiteSpace(value)) return false;
        Log.Debug("Setting: {0} = {1} ({2})", key, value, "AppSetting");
        return true;
    }

    static bool TryGetEnvVar(string key, out string value)
    {
        value = Environment.GetEnvironmentVariable(key);
        if (string.IsNullOrWhiteSpace(value)) return false;
        Log.Debug("Setting: {0} = {1} ({2})", key, value, "Environment");
        return true;
    }
}
