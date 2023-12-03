using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Debaser.Attributes;
using NUnit.Framework;
// ReSharper disable ArgumentsStyleLiteral

namespace Debaser.Tests.Bugs;

[TestFixture]
public class TestDecimalPrecision : FixtureBase
{
    [Test]
    public async Task ItWorks()
    {
        var helper = new UpsertHelper<SomeClassWithDecimal>(ConnectionString);

        helper.DropSchema(dropType: true, dropTable: true, dropProcedure: true);
        helper.CreateSchema();

        await helper.UpsertAsync(new[]
        {
            new SomeClassWithDecimal {Decimal = 1.1m, Id = Guid.NewGuid().ToString()},
            new SomeClassWithDecimal {Decimal = 1.12m, Id = Guid.NewGuid().ToString()},
            new SomeClassWithDecimal {Decimal = 1.123m, Id = Guid.NewGuid().ToString()},
            new SomeClassWithDecimal {Decimal = 1.1234m, Id = Guid.NewGuid().ToString()},
            new SomeClassWithDecimal {Decimal = 1.12345m, Id = Guid.NewGuid().ToString()},
            new SomeClassWithDecimal {Decimal = 1.123456m, Id = Guid.NewGuid().ToString()},
            new SomeClassWithDecimal {Decimal = 1.1234567m, Id = Guid.NewGuid().ToString()},
            new SomeClassWithDecimal {Decimal = 1.12345678m, Id = Guid.NewGuid().ToString()},
            new SomeClassWithDecimal {Decimal = 1.123456789m, Id = Guid.NewGuid().ToString()},
            new SomeClassWithDecimal {Decimal = 1.1234567891m, Id = Guid.NewGuid().ToString()},
        });

        var all = helper.LoadAll().OrderBy(d => d.Decimal).ToList();

        Assert.That(all.Select(a => a.Decimal), Is.EqualTo(new[]
        {
            1.1m,
            1.12m,
            1.123m,
            1.1234m,
            1.12345m,
            1.123456m,
            1.1234567m,
            1.12345678m,
            1.123456789m,
            1.1234567891m,
        }));
    }

    class SomeClassWithDecimal
    {
        public string Id { get; set; }

        [DebaserSqlType(SqlDbType.Decimal, size: 20, altSize: 10)]
        public decimal Decimal { get; set; }
    }
}