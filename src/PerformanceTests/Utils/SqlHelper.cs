using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;

public static class SqlHelper
{
    public static void ExecuteScript(string connectionString, string sql)
    {
        var commands = Regex
            .Split(sql, @"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase)
            .Where(commandString => !string.IsNullOrWhiteSpace(commandString));

        using (var con = new SqlConnection(connectionString))
        {
            con.Open();
            foreach (var cmdText in commands)
            {
                using (var command = new SqlCommand(cmdText, con))
                {
                    command.ExecuteNonQuery();
                }
            }
            con.Close();
        }
    }

    public static void CreateDatabase(string connectionString)
    {
        try
        {
            var builder = new DbConnectionStringBuilder
            {
                ConnectionString = connectionString
            };
            var catalog = builder["database"];
            builder.Remove("database");
            var master = builder.ToString();
            ExecuteScript(master, $"IF db_id('{catalog}') IS NULL CREATE DATABASE {catalog}");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Could not create database.", ex);
        }
    }
}