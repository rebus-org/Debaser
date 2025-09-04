using Npgsql;

namespace Postgredebaser.Tests.Schema;

[TestFixture]
public class TestSchemaCreator : FixtureBase
{
    [Test]
    public void CanCreateSchema()
    {
        using var connection = new NpgsqlConnection(ConnectionString);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = @"
CREATE SCHEMA IF NOT EXISTS bimse;
";
        command.ExecuteNonQuery();

        var helper = new UpsertHelper<SomeClass>(ConnectionString, "testtable", "bimse");

        helper.DropSchema(dropTable: true);
        helper.CreateSchema();
    }

    class SomeClass
    {
        public int Id { get; set; }
        public string Text { get; set; }
    }
}