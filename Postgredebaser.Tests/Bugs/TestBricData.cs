using Debaser.Attributes;

namespace Postgredebaser.Tests.Bugs;

[TestFixture]
public class TestBricData : FixtureBase
{
    UpsertHelper<BricData> _upserter;

    protected override void SetUp()
    {
        _upserter = new UpsertHelper<BricData>(ConnectionString);
        _upserter.DropSchema(dropTable: true);
        _upserter.CreateSchema();
    }

    [Test]
    public async Task CanWriteDoublesAndFloats()
    {
        await _upserter.UpsertAsync([
            new BricData {CellId = "hg03jg93", GnsHstIndk2010 = 24, GnsPersIndkHigh2010 = 3435}
        ]);
    }

    public class BricData
    {
        [DebaserKey]
        public string CellId { get; set; }
        public double GnsHstIndk2010 { get; set; }
        public float GnsPersIndkHigh2010 { get; set; }
    }

}