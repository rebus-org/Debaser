using System;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace Debaser.Tests
{
    public abstract class FixtureBase
    {
        const int DatabaseAlreadyExists = 1801;

        protected static string ConnectionString => Environment.GetEnvironmentVariable("testdb") ?? "server=.; database=debaser_test; trusted_connection=true";

        [SetUp]
        public void InnerSetUp()
        {
            SetUp();
        }

        protected virtual void SetUp()
        {
        }

        protected static async Task<SqlConnection> OpenSqlConnection()
        {
            var connection = new SqlConnection(ConnectionString);

            await connection.OpenAsync();

            return connection;
        }
    }
}