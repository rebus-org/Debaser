// ReSharper disable UnusedMember.Local
// ReSharper disable ClassNeverInstantiated.Local

namespace Postgredebaser.Tests.Naming;

[TestFixture]
public class TestSnakeCaseSchema : FixtureBase
{
    [Test]
    public void DerivesSnakeCaseTableAndColumnNames()
    {
        var helper = new UpsertHelper<OrderLine>(ConnectionString);

        var script = helper.GetCreateSchemaScript();

        Assert.That(script, Does.Contain("\"public\".\"order_line\""));
        Assert.That(script, Does.Contain("\"order_number\""));
        Assert.That(script, Does.Contain("\"customer_id\""));
    }

    [Test]
    public void UsesExplicitTableNameVerbatim()
    {
        var helper = new UpsertHelper<OrderLine>(ConnectionString, "MyTable");

        var script = helper.GetCreateSchemaScript();

        Assert.That(script, Does.Contain("\"public\".\"MyTable\""));

        // ...the columns are still converted though
        Assert.That(script, Does.Contain("\"order_number\""));
    }

    [Test]
    public void DropScriptUsesTheConvertedTableNameToo()
    {
        var helper = new UpsertHelper<OrderLine>(ConnectionString);

        Assert.That(helper.GetDropSchemaScript(dropTable: true), Does.Contain("\"public\".\"order_line\""));
    }

    class OrderLine
    {
        public int Id { get; set; }

        public string OrderNumber { get; set; }

        public Guid CustomerID { get; set; }
    }
}
