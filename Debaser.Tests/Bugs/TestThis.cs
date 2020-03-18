using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Debaser.Attributes;
using NUnit.Framework;
// ReSharper disable ArgumentsStyleLiteral

namespace Debaser.Tests.Bugs
{
    [TestFixture]
    public class TestThis : FixtureBase
    {
        [Test]
        public async Task CanRoundtripThisBadBoy()
        {
            var helper = new UpsertHelper<SomeKindOfLine>(ConnectionString);

            helper.DropSchema(dropProcedure: true, dropTable: true, dropType: true);
            helper.CreateSchema();

            await helper.UpsertAsync(new[] { new SomeKindOfLine(123, 789, 505, TimeZoneInfo.Local) });

            var roundtripped = helper.LoadAll().ToList();

            Assert.That(roundtripped.Count, Is.EqualTo(1));

            var line = roundtripped.First();

            Assert.That(line.SecondId, Is.EqualTo(789));
            Assert.That(line.FirstId, Is.EqualTo(123));
            Assert.That(line.DecimalNumber, Is.EqualTo(505));
            Assert.That(line.Timezone, Is.EqualTo(TimeZoneInfo.Local));
        }

        public class SomeKindOfLine
        {
            public SomeKindOfLine(int firstId, int secondId, decimal decimalNumber, TimeZoneInfo timezone)
            {
                FirstId = firstId;
                SecondId = secondId;
                DecimalNumber = decimalNumber;
                Timezone = timezone;
            }

            [DebaserKey]
            public int FirstId { get; }

            [DebaserKey]
            public int SecondId { get; }

            public decimal DecimalNumber { get; }

            [DebaserMapper(typeof(TimeZoneInfoDebaserMapper))]
            public TimeZoneInfo Timezone { get; }

            class TimeZoneInfoDebaserMapper : IDebaserMapper
            {
                public object ToDatabase(object arg) =>
                    arg is TimeZoneInfo tz
                        ? tz.Id
                        : throw new ArgumentException($"The argument {arg} is not a TimeZoneInfo");

                public object FromDatabase(object arg)
                {
                    try
                    {
                        return TimeZoneInfo.FindSystemTimeZoneById((string)arg);
                    }
                    catch (Exception exception)
                    {
                        throw new ArgumentException($"Could not generate TimeZoneInfo object from value '{arg}'", exception);
                    }
                }

                public SqlDbType SqlDbType => SqlDbType.NVarChar;
                public int? SizeOrNull => 70;
                public int? AdditionalSizeOrNull => null;
            }
        }

    }

}