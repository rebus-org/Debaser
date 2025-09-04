using Debaser.Core.Attributes;

namespace Postgredebaser.Tests.Query;

[TestFixture]
public class TestDeleting : FixtureBase
{
    UpsertHelper<RowWithData> _upsertHelper;

    protected override void SetUp()
    {
        _upsertHelper = new UpsertHelper<RowWithData>(ConnectionString);

        _upsertHelper.DropSchema(dropTable: true);
        _upsertHelper.CreateSchema();
    }

    [Test]
    public async Task CanDeleteRows_Args()
    {
        var rows = new[]
        {
            new RowWithData("1", "hej"),
            new RowWithData("2", "hej"),
            new RowWithData("3", "bum!"),
            new RowWithData("4", "bum!"),
            new RowWithData("5", "bum!"),
            new RowWithData("6", "bum!"),
            new RowWithData("7", "farvel"),
            new RowWithData("8", "farvel"),
            new RowWithData("9", "farvel"),
        };

        await _upsertHelper.UpsertAsync(rows);

        var allRows = _upsertHelper.LoadAll().ToList();

        await _upsertHelper.DeleteWhereAsync("\"data\" = @data", new { data = "hej" });

        var rowsAfterDeletingHej = _upsertHelper.LoadAll().ToList();

        await _upsertHelper.DeleteWhereAsync("\"data\" = @data", new { data = "farvel" });

        var rowsAfterDeletingFarvel = _upsertHelper.LoadAll().ToList();

        Assert.That(allRows.Count, Is.EqualTo(9));

        Assert.That(rowsAfterDeletingHej.Count, Is.EqualTo(7));

        Assert.That(rowsAfterDeletingFarvel.Count, Is.EqualTo(4));
    }

    [Test]
    public async Task CanDeleteRows_Dictionary()
    {
        var rows = new[]
        {
            new RowWithData("1", "hej"),
            new RowWithData("2", "hej"),
            new RowWithData("3", "bum!"),
            new RowWithData("4", "bum!"),
            new RowWithData("5", "bum!"),
            new RowWithData("6", "bum!"),
            new RowWithData("7", "farvel"),
            new RowWithData("8", "farvel"),
            new RowWithData("9", "farvel"),
        };

        await _upsertHelper.UpsertAsync(rows);

        var allRows = _upsertHelper.LoadAll().ToList();

        await _upsertHelper.DeleteWhereAsync("\"data\" = @data", new Dictionary<string, object> { ["data"] = "hej" });

        var rowsAfterDeletingHej = _upsertHelper.LoadAll().ToList();

        await _upsertHelper.DeleteWhereAsync("\"data\" = @data", new Dictionary<string, object> { ["data"] = "farvel" });

        var rowsAfterDeletingFarvel = _upsertHelper.LoadAll().ToList();

        Assert.That(allRows.Count, Is.EqualTo(9));

        Assert.That(rowsAfterDeletingHej.Count, Is.EqualTo(7));

        Assert.That(rowsAfterDeletingFarvel.Count, Is.EqualTo(4));
    }

    class RowWithData(string id, string data)
    {
        [DebaserKey]
        public string Id { get; } = id;

        public string Data { get; } = data;
    }
}