using Microsoft.Data.SqlClient;
// ReSharper disable ArgumentsStyleNamedExpression

namespace Debaser.Internals.Sql;

class SqlConnectionFactory(string connectionString)
{
    public SqlConnection OpenSqlConnection()
    {
        var connection = new SqlConnection(connectionString);
        connection.Open();
        return connection;
    }
}