using Npgsql;

namespace Postgredebaser.Tests;

public abstract class FixtureBase
{
    protected static string ConnectionString => "host=localhost; database=postgredebaser_test; user id=user; password=password;";

    [SetUp]
    public void InnerSetUp()
    {
        SetUp();
    }

    protected virtual void SetUp()
    {
    }

    protected static async Task<NpgsqlConnection> OpenNpgsqlConnection()
    {
        var connection = new NpgsqlConnection(ConnectionString);

        await connection.OpenAsync();

        return connection;
    }
}