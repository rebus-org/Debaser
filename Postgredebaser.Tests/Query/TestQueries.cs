using Debaser.Attributes;

namespace Postgredebaser.Tests.Query;

[TestFixture]
public class TestQueries : FixtureBase
{
    UpsertHelper<RowWithData> _upsertHelper;

    protected override void SetUp()
    {
        _upsertHelper = new UpsertHelper<RowWithData>(ConnectionString);

        _upsertHelper.DropSchema(dropTable: true);
        _upsertHelper.CreateSchema();
    }

    [Test]
    public async Task CanQueryRows_Args()
    {
        var rows = new[]
        {
            new RowWithData(1, "number1"),
            new RowWithData(2, "number2"),
            new RowWithData(3, "number3"),
            new RowWithData(4, "number4"),
            new RowWithData(5, "number5"),
        };

        await _upsertHelper.UpsertAsync(rows);

        var results1 = await _upsertHelper.LoadWhereAsync("\"data\" = 'number4'");
        var results2 = await _upsertHelper.LoadWhereAsync("\"data\" = @data", new { data = "number4" });

        Assert.That(results1.Count, Is.EqualTo(1));
        Assert.That(results2.Count, Is.EqualTo(1));

        Assert.That(results1[0].Id, Is.EqualTo(4));
        Assert.That(results2[0].Id, Is.EqualTo(4));
    }

    [Test]
    public async Task CanQueryRows_Dictionary()
    {
        var rows = new[]
        {
            new RowWithData(1, "number1"),
            new RowWithData(2, "number2"),
            new RowWithData(3, "number3"),
            new RowWithData(4, "number4"),
            new RowWithData(5, "number5"),
        };

        await _upsertHelper.UpsertAsync(rows);

        var results1 = await _upsertHelper.LoadWhereAsync("\"data\" = 'number4'");
        var results2 = await _upsertHelper.LoadWhereAsync("\"data\" = @data", new Dictionary<string, object> { ["data"] = "number4" });

        Assert.That(results1.Count, Is.EqualTo(1));
        Assert.That(results2.Count, Is.EqualTo(1));

        Assert.That(results1[0].Id, Is.EqualTo(4));
        Assert.That(results2[0].Id, Is.EqualTo(4));
    }

    class RowWithData(int id, string data)
    {
        [DebaserKey]
        public int Id { get; } = id;

        public string Data { get; } = data;
    }
}