using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Debaser.Attributes;
using NUnit.Framework;
// ReSharper disable ArgumentsStyleLiteral

namespace Debaser.Tests.Bugs;

[TestFixture]
public class TestThisThingWithNull : FixtureBase
{
    UpsertHelper<CanContainNullValue> _helper;

    protected override void SetUp()
    {
        _helper = new UpsertHelper<CanContainNullValue>(ConnectionString);

        _helper.DropSchema(dropProcedure: true, dropTable: true, dropType: true);
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

    class CanContainNullValue
    {
        public string Id { get; }

        [DebaserSqlType(SqlDbType.Decimal, 15, 5)]
        public decimal? Value { get; }

        public CanContainNullValue(string id, decimal? value)
        {
            Id = id;
            Value = value;
        }
    }
}