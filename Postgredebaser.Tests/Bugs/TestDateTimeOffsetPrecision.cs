using NpgsqlTypes;
using Debaser.Core.Attributes;
using Postgredebaser.Attributes;

namespace Postgredebaser.Tests.Bugs;

[TestFixture]
public class TestDateTimeOffsetPrecision : FixtureBase
{
    [Test]
    public async Task ItWorks()
    {
        var helper = new UpsertHelper<SomeClassWithDateTimeOffset>(ConnectionString);

        helper.DropSchema(dropTable: true);
        helper.CreateSchema();

        var now = DateTimeOffset.UtcNow;

        await helper.UpsertAsync([
            new SomeClassWithDateTimeOffset {Id = "1", Time = now},
            new SomeClassWithDateTimeOffset {Id = "2", Time = now},
            new SomeClassWithDateTimeOffset {Id = "3", Time = now}
        ]);

        var all = helper.LoadAll();

        Assert.That(all.Count(), Is.EqualTo(3));
    }

    class SomeClassWithDateTimeOffset
    {
        [DebaserKey]
        public string Id { get; set; }

        public DateTimeOffset Time { get; set; }
    }
}