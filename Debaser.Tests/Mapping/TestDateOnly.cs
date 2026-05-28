namespace Debaser.Tests.Mapping;

[TestFixture]
public class TestDateOnly : FixtureBase
{
    UpsertHelper<SimpleRecordRow> _upsertHelper;

    protected override void SetUp()
    {
        base.SetUp();

        _upsertHelper = new UpsertHelper<SimpleRecordRow>(ConnectionString);
        _upsertHelper.DropSchema(dropTable: true, dropProcedure: true, dropType: true);
        _upsertHelper.CreateSchema();
    }

    record SimpleRecordRow(int Id, DateOnly Date);

    [Test]
    public async Task CanRoundtripTheseBadBoys()
    {
        await _upsertHelper.UpsertAsync([
            new(1, new(2026, 5, 1)),
            new(2, new(2026, 5, 2)),
            new(3, new(2026, 5, 3)),
        ]);
    }
}