using System.Collections.Generic;
using Microsoft.Data.SqlClient;

namespace Debaser.Tests.Extensions
{
    static class ConnectionExtensions
    {
        public static IEnumerable<string> GetSchemas(this SqlConnection connection) => QueryFor(connection, "name", "select [name] from sys.schemas");

        public static IEnumerable<string> GetTableNames(this SqlConnection connection, string schema = "dbo")
        {
            return QueryFor(connection, "name", $@"

select t.name from sys.tables t
	join sys.schemas s on s.schema_id = t.schema_id
	where s.name = '{schema}'

");
        }

        public static IEnumerable<string> GetTableDataTypeNames(this SqlConnection connection, string schema = "dbo")
        {
            return QueryFor(connection, "name", $@"

select t.name from sys.table_types t
	join sys.schemas s on s.schema_id = t.schema_id
	where s.name = '{schema}'

");
        }

        public static IEnumerable<string> GetSprocNames(this SqlConnection connection, string schema = "dbo")
        {
            return QueryFor(connection, "name", $@"

select t.name from sys.procedures t
	join sys.schemas s on s.schema_id = t.schema_id
	where s.name = '{schema}'

");
        }

        static IEnumerable<string> QueryFor(SqlConnection connection, string columnName, string sql)
        {
            using var commmand = connection.CreateCommand();

            commmand.CommandText = sql;

            using var reader = commmand.ExecuteReader();

            while (reader.Read())
            {
                yield return (string)reader[columnName];
            }
        }
    }
}