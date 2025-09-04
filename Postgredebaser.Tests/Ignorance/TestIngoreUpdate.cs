using Debaser.Core.Attributes;

namespace Postgredebaser.Tests.Ignorance;

[TestFixture]
public class TestIngoreUpdate : FixtureBase
{
    UpsertHelper<SomeRowWithIntegerRevision> _upsertHelper;
    UpsertHelper<SomeRowWithDateTimeRevision> _upsertHelper2;

    protected override void SetUp()
    {
        _upsertHelper = new UpsertHelper<SomeRowWithIntegerRevision>(ConnectionString);
        _upsertHelper.DropSchema(dropTable: true);
        _upsertHelper.CreateSchema();

        _upsertHelper2 = new UpsertHelper<SomeRowWithDateTimeRevision>(ConnectionString);
        _upsertHelper2.DropSchema(dropTable: true);
        _upsertHelper2.CreateSchema();
    }

    [Test]
    public async Task CanCarryOutOrdinaryUpdate()
    {
        await _upsertHelper.UpsertAsync([
            new SomeRowWithIntegerRevision(1, "hej", 0),
            new SomeRowWithIntegerRevision(2, "med", 0),
            new SomeRowWithIntegerRevision(3, "dig", 0)
        ]);

        var rows = _upsertHelper.LoadAll().OrderBy(r => r.Id).ToList();

        Assert.That(rows.Count, Is.EqualTo(3));
        Assert.That(rows.Select(r => r.Id), Is.EqualTo(new[] { 1, 2, 3 }));
    }

    [Test]
    public async Task CanCarryOutConditionalUpdate()
    {
        await _upsertHelper.UpsertAsync([
            new SomeRowWithIntegerRevision(1, "hej", 0),
            new SomeRowWithIntegerRevision(2, "med", 0),
            new SomeRowWithIntegerRevision(3, "dig", 0)
        ]);

        Assert.That(_upsertHelper.LoadAll().OrderBy(r => r.Id).Select(r => r.Data), Is.EqualTo(new[] { "hej", "med", "dig" }));

        await _upsertHelper.UpsertAsync([
            new SomeRowWithIntegerRevision(1, "hej", 1),
            new SomeRowWithIntegerRevision(2, "med", 1),
            new SomeRowWithIntegerRevision(3, "mig", 1)
        ]);

        Assert.That(_upsertHelper.LoadAll().OrderBy(r => r.Id).Select(r => r.Data), Is.EqualTo(new[] { "hej", "med", "mig" }));

        await _upsertHelper.UpsertAsync([
            new SomeRowWithIntegerRevision(1, "hej", 2),
            new SomeRowWithIntegerRevision(2, "med", 2),
            new SomeRowWithIntegerRevision(3, "Frank Frank", 1)
        ]);

        Assert.That(_upsertHelper.LoadAll().OrderBy(r => r.Id).Select(r => r.Data), Is.EqualTo(new[] { "hej", "med", "mig" }));
    }

    // [DebaserUpdateCriteria("excluded.\"rev\" > \"public\".\"somerowtointegerrevision\".\"rev\"")]
    class SomeRowWithIntegerRevision(int id, string data, int rev)
    {
        [DebaserKey]
        public int Id { get; } = id;
        public string Data { get; } = data;

        public int Rev { get; } = rev;
    }

    [Test]
    public async Task WorksWithDateTimeCriteriaToo()
    {
        var t0 = DateTime.Now;
        var t1 = t0.AddSeconds(1);
        var tminus1 = t0.AddSeconds(-1);

        await _upsertHelper2.UpsertAsync([new SomeRowWithDateTimeRevision(1, "hej", t0)]);

        Assert.That(_upsertHelper2.LoadAll().Single().Data, Is.EqualTo("hej"));

        await _upsertHelper2.UpsertAsync([new SomeRowWithDateTimeRevision(1, "hej igen", t1)]);

        Assert.That(_upsertHelper2.LoadAll().Single().Data, Is.EqualTo("hej igen"));

        await _upsertHelper2.UpsertAsync([new SomeRowWithDateTimeRevision(1, "DEN HER SKAL IKKE IGENNEM", tminus1)]);

        Assert.That(_upsertHelper2.LoadAll().Single().Data, Is.EqualTo("hej igen"));
    }

    [DebaserUpdateCriteria("excluded.\"lastupdated\" > \"public\".\"somerowwithdatetimerevision\".\"lastupdated\"")]
    class SomeRowWithDateTimeRevision(int id, string data, DateTime lastUpdated)
    {
        [DebaserKey]
        public int Id { get; } = id;
        public string Data { get; } = data;

        public DateTime LastUpdated { get; } = lastUpdated;
    }
}