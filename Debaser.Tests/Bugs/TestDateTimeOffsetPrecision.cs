using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Debaser.Attributes;
using NUnit.Framework;

namespace Debaser.Tests.Bugs;

[TestFixture]
public class TestDateTimeOffsetPrecision : FixtureBase
{
    [Test]
    public async Task ItWorks()
    {
        var helper = new UpsertHelper<SomeClassWithDateTimeOffset>(ConnectionString);

        helper.DropSchema(dropType: true, dropTable: true, dropProcedure: true);
        helper.CreateSchema();

        var now = DateTimeOffset.Now;

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
        public string Id { get; set; }

        [DebaserSqlType(SqlDbType.DateTimeOffset, size: 0)]
        public DateTimeOffset Time { get; set; }
    }
}