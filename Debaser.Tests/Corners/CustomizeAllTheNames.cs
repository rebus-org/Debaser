using System;
using System.Linq;
using System.Threading.Tasks;
using Debaser.Tests.Extensions;
using NUnit.Framework;
// ReSharper disable RedundantArgumentDefaultValue
// ReSharper disable ArgumentsStyleStringLiteral

namespace Debaser.Tests.Corners;

[TestFixture]
public class CustomizeAllTheNames : FixtureBase
{
    [Test]
    public async Task CanUseGuidsAsNames()
    {
        var tableName = Guid.NewGuid().ToString("N");
        var typeName = Guid.NewGuid().ToString("N");
        var procName = Guid.NewGuid().ToString("N");

        var helper = new UpsertHelper<SomeModel>(ConnectionString, tableName: tableName, procName: procName, typeName: typeName);

        helper.DropSchema(dropType: true, dropTable: true, dropProcedure: true);
        helper.CreateSchema();

        await helper.UpsertAsync(new[] { new SomeModel { Id = 123, Text = "nummer 123" } });

        var rows = helper.LoadAll().ToList();

        Assert.That(rows.Count, Is.EqualTo(1));

        var row = rows.First();

        Assert.That(row.Id, Is.EqualTo(123));
        Assert.That(row.Text, Is.EqualTo("nummer 123"));

        await using var connection = await OpenSqlConnection();

        var schemas = connection.GetSchemas().ToList();

        Assert.That(schemas, Contains.Item("dbo"));

        Assert.That(connection.GetTableNames(schema: "dbo"), Contains.Item(tableName));
        Assert.That(connection.GetTableDataTypeNames(schema: "dbo"), Contains.Item(typeName));
        Assert.That(connection.GetSprocNames(schema: "dbo"), Contains.Item(procName));
    }

    class SomeModel
    {
        public int Id { get; set; }
        public string Text { get; set; }
    }
}