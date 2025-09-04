using Npgsql;

namespace Postgredebaser.Internals.Sql;

public class NpgsqlConnectionFactory(string connectionString)
{
    readonly string _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));

    public NpgsqlConnection OpenNpgsqlConnection()
    {
        var connection = new NpgsqlConnection(_connectionString);
        connection.Open();
        return connection;
    }
}