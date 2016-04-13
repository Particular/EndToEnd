namespace Common
{
    using System;
    using System.Configuration;
    using System.Diagnostics;

    public static class ProfileExtensionMethods
    {
        public static string GetConnectionString(this IProfile currentProfile, string connectionStringName)
        {
            var environmentVariableConnectionString = Environment.GetEnvironmentVariable(connectionStringName);
            if (!string.IsNullOrWhiteSpace((environmentVariableConnectionString)))
            {
                Trace.TraceInformation("Environment variable found {0}", environmentVariableConnectionString);
                return environmentVariableConnectionString;
            }

            var applicationConfigConnectionString = ConfigurationManager.ConnectionStrings[connectionStringName].ConnectionString;
            if (!string.IsNullOrWhiteSpace((applicationConfigConnectionString)))
            {
                Trace.TraceInformation("App.config connection string variable found {0}", environmentVariableConnectionString);
                return applicationConfigConnectionString;
            }

            throw new ConfigurationErrorsException(string.Format("Could not find an environment variable or connection string named {0}", connectionStringName));
        }
    }
}