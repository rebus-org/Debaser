using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Debaser.Tests.Corners;

[TestFixture]
public class InsertEmptySequence : FixtureBase
{
    UpsertHelper<MinimalRow> _upserter;

    protected override void SetUp()
    {
        _upserter = new UpsertHelper<MinimalRow>(ConnectionString);
        _upserter.DropSchema(dropTable: true, dropProcedure: true, dropType: true);
        _upserter.CreateSchema();
    }

    [Test]
    public async Task DoesNotDieWhenUpsertingEmptySequence()
    {
        await _upserter.UpsertAsync(Enumerable.Empty<MinimalRow>());
    }

    [Test]
    public async Task DoesNotDieWhenUpsertingMinimalRows()
    {
        await _upserter.UpsertAsync(new[]
        {
            new MinimalRow(1),
            new MinimalRow(2),
            new MinimalRow(3),
            new MinimalRow(4),
        });

        var allRows = _upserter.LoadAll().OrderBy(r => r.Id).ToList();

        Assert.That(allRows.Select(r => r.Id), Is.EqualTo(new[] { 1, 2, 3, 4 }));
    }

    class MinimalRow
    {
        public MinimalRow(int id)
        {
            Id = id;
        }

        public int Id { get; }
    }
}