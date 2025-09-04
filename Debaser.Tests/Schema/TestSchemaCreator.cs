using Debaser.Internals.Schema;
using Debaser.Internals.Sql;
using Debaser.Mapping;
using Microsoft.Data.SqlClient;

namespace Debaser.Tests.Schema;

[TestFixture]
public class TestSchemaCreator : FixtureBase
{
    [Test]
    public void CanCreateSchema()
    {
        using var connection = new SqlConnection(ConnectionString);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = @"
if not exists (select top 1 * from [sys].[schemas] where [name] = 'bimse')
	exec('create schema bimse');
";
        command.ExecuteNonQuery();

        var mapper = new AutoMapper();
        var map = mapper.GetMap(typeof(SomeClass));
        var properties = map.Properties;
        var keyProperties = properties.Where(p => p.IsKey);

        var creator = new SchemaManager(new SqlConnectionFactory(ConnectionString), "testtable", "testdata", "testproc", keyProperties, properties, schema: "bimse");

        creator.DropSchema(true, true, true);
        creator.CreateSchema(true, true, true);
    }

    class SomeClass
    {
        public int Id { get; set; }
        public string Text { get; set; }
    }
}