using Debaser.Attributes;

namespace Postgredebaser.Tests.Naming;

[TestFixture]
public class TestSnakeCaseRoundtrip : FixtureBase
{
    UpsertHelper<OrderLine> _upsertHelper;

    protected override void SetUp()
    {
        _upsertHelper = new UpsertHelper<OrderLine>(ConnectionString);

        _upsertHelper.DropSchema(dropTable: true);
        _upsertHelper.CreateSchema();
    }

    [Test]
    public async Task CreatesTableAndColumnsWithSnakeCaseNames()
    {
        await using var connection = await OpenNpgsqlConnection();

        await using var command = connection.CreateCommand();
        command.CommandText = """
                              SELECT column_name FROM information_schema.columns
                              WHERE table_schema = 'public' AND table_name = 'order_line'
                              ORDER BY ordinal_position
                              """;

        var columnNames = new List<string>();

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            columnNames.Add(reader.GetString(0));
        }

        Assert.That(columnNames, Is.EqualTo(["id", "order_number", "customer_id"]));
    }

    [Test]
    public async Task CanRoundtripRows()
    {
        await _upsertHelper.UpsertAsync([
            new OrderLine(1, "one", 10),
            new OrderLine(2, "two", 20),
            new OrderLine(3, "three", 30)
        ]);

        var rows = _upsertHelper.LoadAll().OrderBy(r => r.Id).ToList();

        Assert.That(rows.Select(r => r.OrderNumber), Is.EqualTo(["one", "two", "three"]));
        Assert.That(rows.Select(r => r.CustomerID), Is.EqualTo([10, 20, 30]));
    }

    [Test]
    public async Task CanUpdateExistingRows()
    {
        await _upsertHelper.UpsertAsync([new OrderLine(1, "one", 10)]);
        await _upsertHelper.UpsertAsync([new OrderLine(1, "ONE", 11)]);

        var row = _upsertHelper.LoadAll().Single();

        Assert.That(row.OrderNumber, Is.EqualTo("ONE"));
        Assert.That(row.CustomerID, Is.EqualTo(11));
    }

    [Test]
    public async Task CanQueryUsingSnakeCaseCriteria()
    {
        await _upsertHelper.UpsertAsync([
            new OrderLine(1, "one", 10),
            new OrderLine(2, "two", 20)
        ]);

        var results = await _upsertHelper.LoadWhereAsync("\"order_number\" = @n", new { n = "two" });

        Assert.That(results.Count, Is.EqualTo(1));
        Assert.That(results[0].Id, Is.EqualTo(2));
    }

    [Test]
    public async Task CanDeleteUsingSnakeCaseCriteria()
    {
        await _upsertHelper.UpsertAsync([
            new OrderLine(1, "one", 10),
            new OrderLine(2, "two", 20)
        ]);

        await _upsertHelper.DeleteWhereAsync("\"customer_id\" = @id", new { id = 10 });

        Assert.That(_upsertHelper.LoadAll().Select(r => r.Id), Is.EqualTo([2]));
    }

    class OrderLine(int id, string orderNumber, int customerID)
    {
        [DebaserKey]
        public int Id { get; } = id;

        public string OrderNumber { get; } = orderNumber;

        public int CustomerID { get; } = customerID;
    }
}
