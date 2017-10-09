using NServiceBus.Persistence.Sql;

[assembly: SqlPersistenceSettings(
    MsSqlServerScripts = true,
    MySqlScripts = false,
    OracleScripts = false,
    ScriptPromotionPath = "$(SolutionDir)PromotedSqlScripts")]
