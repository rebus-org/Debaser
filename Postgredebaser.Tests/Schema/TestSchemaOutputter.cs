// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable UnusedMember.Local

namespace Postgredebaser.Tests.Schema;

[TestFixture]
public class TestSchemaOutputter : FixtureBase
{
    [Test]
    public void CanGetCreationSchema()
    {
        var helper = new UpsertHelper<Something>(ConnectionString);

        Console.WriteLine(helper.GetCreateSchemaScript());

        Console.WriteLine(helper.GetDropSchemaScript());
    }

    class Something
    {
        public string Id { get; set; }
    }
}