using System.Collections.Generic;

namespace Common
{
    public class NHibernateConnectionInfo
    {
        private const string Dialect = "NHibernate.Dialect.MsSql2012Dialect";
        private const string ConnectionString = @"Data Source=localhost\SQLEXPRESS;Initial Catalog=persistencetests;Integrated Security=True";

        public static IDictionary<string, string> Settings = new Dictionary<string, string>
        {
            {"dialect", Dialect},
            {"connection.connection_string", ConnectionString}
        };
    }
}