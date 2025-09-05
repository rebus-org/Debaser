using Microsoft.Data.SqlClient;

namespace Debaser.Tests;

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

    protected static async Task<SqlConnection> OpenSqlConnection()
    {
        var connection = new SqlConnection(ConnectionString);

        await connection.OpenAsync();

        return connection;
    }
}