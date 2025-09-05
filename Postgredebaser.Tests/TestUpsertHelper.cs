namespace Postgredebaser.Tests;

[TestFixture]
public class TestUpsertHelper : FixtureBase
{
    UpsertHelper<SimpleClassRow> _upsertHelper;
    UpsertHelper<SimpleRecordRow> _upsertHelper2;

    protected override void SetUp()
    {
        _upsertHelper = new UpsertHelper<SimpleClassRow>(ConnectionString);
        _upsertHelper.DropSchema(dropTable: true);
        _upsertHelper.CreateSchema();

        _upsertHelper2 = new UpsertHelper<SimpleRecordRow>(ConnectionString);
        _upsertHelper2.DropSchema(dropTable: true);
        _upsertHelper2.CreateSchema();
    }

    [Test]
    public async Task CanRoundtripSimpleRow()
    {
        await _upsertHelper.UpsertAsync([
            new SimpleClassRow {Id = 1, Text = "this is the first row"},
            new SimpleClassRow {Id = 2, Text = "this is the second row"}
        ]);

        var rows = _upsertHelper.LoadAll().OrderBy(r => r.Id).ToList();

        Assert.That(rows.Count, Is.EqualTo(2));
        Assert.That(rows.Select(r => r.Id), Is.EqualTo([1, 2]));
        Assert.That(rows.Select(r => r.Text), Is.EqualTo(["this is the first row", "this is the second row"]));
    }

    [Test]
    public async Task CanLoadStuffWithAsyncEnumerable()
    {
        await _upsertHelper.UpsertAsync([
            new SimpleClassRow {Id = 1, Text = "this is the first row"},
            new SimpleClassRow {Id = 2, Text = "this is the second row"}
        ]);

        var count = 0;

        await foreach (var _ in _upsertHelper.LoadAllAsync())
        {
            count++;
        }

        Assert.That(count, Is.EqualTo(2));
    }

    [Test]
    public async Task DoesItEvenWorkWithRecords()
    {
        await _upsertHelper2.UpsertAsync([new(1, "hej"), new(2, "hej igen")]);

        var records = _upsertHelper2.LoadAll().ToList();

        Assert.That(records.Count, Is.EqualTo(2));

        var rec1 = records.First(r => r.Id == 1);
        var rec2 = records.First(r => r.Id == 2);

        Assert.That(rec1.Text, Is.EqualTo("hej"));
        Assert.That(rec2.Text, Is.EqualTo("hej igen"));
    }

    record SimpleRecordRow(int Id, string Text);

    class SimpleClassRow
    {
        public int Id { get; set; }

        public string Text { get; set; }
    }
}