
// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable UnusedMember.Local

namespace Debaser.Tests.Schema;

[TestFixture]
public class TestSchemaOutputter : FixtureBase
{
    [Test]
    public void CanGetCreationSchema()
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