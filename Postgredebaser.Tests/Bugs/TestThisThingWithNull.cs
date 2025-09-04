using NpgsqlTypes;
using Postgredebaser.Attributes;

// ReSharper disable ArgumentsStyleLiteral

namespace Postgredebaser.Tests.Bugs;

[TestFixture]
public class TestThisThingWithNull : FixtureBase
{
    UpsertHelper<CanContainNullValue> _helper;

    protected override void SetUp()
    {
        _helper = new UpsertHelper<CanContainNullValue>(ConnectionString);

        _helper.DropSchema(dropTable: true);
        _helper.CreateSchema();
    }

    [Test]
    public async Task CanRoundtripObjectWithNullValueInIt()
    {
        var row1 = new CanContainNullValue("id1", 123);
        var row2 = new CanContainNullValue("id2", null);

        await _helper.UpsertAsync([row1, row2]);

        var rows = _helper.LoadAll().OrderBy(r => r.Id).ToList();

        Assert.That(rows[0].Value, Is.EqualTo(123));
        Assert.That(rows[1].Value, Is.EqualTo(default(decimal?)));
    }

    class CanContainNullValue(string id, decimal? value)
    {
        public string Id { get; } = id;

        [DebaserNpgsqlType(NpgsqlDbType.Numeric, 15, 5)]
        public decimal? Value { get; } = value;
    }
}