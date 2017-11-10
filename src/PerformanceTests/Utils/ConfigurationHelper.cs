using System;
using System.Configuration;
using log4net;

public static class ConfigurationHelper
{
    static readonly ILog Log = LogManager.GetLogger(nameof(Configuration));

    public static string GetConnectionString(string connectionStringName)
    {
        var environmentVariableConnectionString = Environment.GetEnvironmentVariable(connectionStringName);
        if (!string.IsNullOrWhiteSpace(environmentVariableConnectionString))
        {
            Log.Info("Environment variable found.");
            return environmentVariableConnectionString;
        }

        var applicationConfigConnectionString = ConfigurationManager.ConnectionStrings[connectionStringName];
        if (applicationConfigConnectionString != null)
        {
            Log.Info("App.config connection string variable found.");
            return applicationConfigConnectionString.ConnectionString;
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
        return true;
    }

    static bool TryGetEnvVar(string key, out string value)
    {
        value = Environment.GetEnvironmentVariable(key);
        if (string.IsNullOrWhiteSpace(value)) return false;
        return true;
    }
}
