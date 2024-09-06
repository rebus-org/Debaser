using System.Linq;
using System.Threading.Tasks;
using Debaser.Attributes;
using NUnit.Framework;

namespace Debaser.Tests.Ignorance;

[TestFixture]
public class TestDebaserRevisionCriteriaAttribute : FixtureBase
{
    UpsertHelper<HasRevisionNumber> _helper;

    protected override void SetUp()
    {
        _helper = new UpsertHelper<HasRevisionNumber>(ConnectionString);
        _helper.DropSchema(dropTable: true, dropType: true, dropProcedure: true);
        _helper.CreateSchema();
    }

    [Test]
    public async Task IgnoresUpdate_LowerRevision()
    {
        const string id = "known-id";
        await _helper.UpsertAsync([new HasRevisionNumber(id, "data1", revision: 2)]);

        await _helper.UpsertAsync([new HasRevisionNumber(id, "data999", revision: 1)]);

        var rows = _helper.LoadAll().ToList();
        Assert.That(rows.Count, Is.EqualTo(1));
        var row = rows.First();
        Assert.That(row.Data, Is.EqualTo("data1"));
    }

    [Test]
    public async Task IgnoresUpdate_EqualRevision()
    {
        const string id = "known-id";
        await _helper.UpsertAsync([new HasRevisionNumber(id, "data1", revision: 1)]);

        await _helper.UpsertAsync([new HasRevisionNumber(id, "data999", revision: 1)]);

        var rows = _helper.LoadAll().ToList();
        Assert.That(rows.Count, Is.EqualTo(1));
        var row = rows.First();
        Assert.That(row.Data, Is.EqualTo("data1"));
    }

    [Test]
    public async Task PerformsUpdate_GreaterRevision()
    {
        const string id = "known-id";
        await _helper.UpsertAsync([new HasRevisionNumber(id, "data1", revision: 1)]);

        await _helper.UpsertAsync([new HasRevisionNumber(id, "data999", revision: 2)]);

        var rows = _helper.LoadAll().ToList();
        Assert.That(rows.Count, Is.EqualTo(1));
        var row = rows.First();
        Assert.That(row.Data, Is.EqualTo("data999"));
    }


    [DebaserRevisionCriteria(nameof(Revision))]
    class HasRevisionNumber(string id, string data, int revision)
    {
        public string Id { get; } = id;
        public string Data { get; } = data;
        public int Revision { get; } = revision;
    }
}