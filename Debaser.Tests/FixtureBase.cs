using System.Configuration;
using System.Data.SqlClient;
using NUnit.Framework;

namespace Debaser.Tests
{
    public abstract class FixtureBase
    {
        const int DatabaseAlreadyExists = 1801;
        protected static string ConnectionString => ConfigurationManager.ConnectionStrings["db"].ConnectionString;

        static FixtureBase()
        {
            EnsureDatabaseExists("debaser_test");
        }

        [SetUp]
        public void InnerSetUp()
        {
            SetUp();
        }

        protected virtual void SetUp()
        {

        }

        static void EnsureDatabaseExists(string databaseName)
        {
            const string masterConnectionString = "server=.; database=master; trusted_connection=true";

            using (var connection = new SqlConnection(masterConnectionString))
            {
                connection.Open();

                try
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = $"CREATE DATABASE [{databaseName}]";
                        command.ExecuteNonQuery();
                    }
                }
                catch (SqlException sqlException) when (sqlException.Number == DatabaseAlreadyExists)
                {
                }
            }
        }
    }
}