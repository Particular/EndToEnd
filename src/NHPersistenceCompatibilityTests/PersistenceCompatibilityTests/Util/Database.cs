namespace PersistenceCompatibilityTests
{
    using System.Data.SqlClient;
    using Common;

    public class Database
    {
        public static void Cleanup()
        {
            var connectionString = NHibernateConnectionInfo.ConnectionString;
            var builder = new SqlConnectionStringBuilder(connectionString);
            var initialCatalog = builder.InitialCatalog;

            using (var connection = new SqlConnection(builder.ConnectionString))
            using (var command = connection.CreateCommand())
            {
                connection.Open();

                var query = @"IF  NOT EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE name = N'{0}')
                                CREATE DATABASE [{0}]
                                     
                              USE {0}

                              exec sp_MSforeachtable ""declare @name nvarchar(max); set @name = parsename('?', 1); exec sp_MSdropconstraints @name"";
                              exec sp_MSforeachtable ""drop table ?""; ";


                command.CommandText = string.Format(query, initialCatalog);
                command.ExecuteNonQuery();
            }
        }
    }
}