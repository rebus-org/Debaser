using System;
using System.Data.SqlClient;

namespace Debaser.Internals.Sql
{
    class SqlConnectionFactory
    {
        readonly bool _useManagedIdentity;
        readonly string _connectionString;

        public SqlConnectionFactory(string connectionString)
        {
            var builder = new SqlConnectionStringBuilder(connectionString ?? throw new ArgumentNullException(nameof(connectionString)));

            _useManagedIdentity = builder.ContainsKey("Authentication") && string.Equals(builder["Authentication"] as string, "Active Directory Interactive");
            
            builder.Remove("Authentication");

            _connectionString = builder.ConnectionString;
        }


        public SqlConnection OpenSqlConnection()
        {
            var connection = new SqlConnection(_connectionString);
            connection.Open();
            return connection;
        }
    }
}