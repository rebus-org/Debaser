using Npgsql;

namespace Postgredebaser.Tests;

public abstract class FixtureBase
{
    protected static string ConnectionString => ContainerManager.ConnectionString;

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