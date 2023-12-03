using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Debaser.Tests.Schema;

[TestFixture]
public class TestSchemaOutputter : FixtureBase
{
    [Test]
    public async Task CanGetCreationSchema()
    {
        var helper = new UpsertHelper<Something>(ConnectionString);

        Console.WriteLine(helper.GetCreateSchemaScript());

        Console.WriteLine(helper.GetDropSchemaScript(dropProcedure: true, dropTable: true, dropType: true));
    }

    class Something
    {
        public string Id { get; set; }
    }
}