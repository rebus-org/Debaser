using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
// ReSharper disable ArgumentsStyleLiteral

namespace Debaser.Tests.Corners
{
    [TestFixture]
    public class UseSomeNullableTypes : FixtureBase
    {
        UpsertHelper<SomeClassWithNullables> _upsertHelper;

        protected override void SetUp()
        {
            _upsertHelper = new UpsertHelper<SomeClassWithNullables>(ConnectionString);
            _upsertHelper.DropSchema(dropProcedure: true, dropTable: true, dropType: true);
            _upsertHelper.CreateSchema();
        }

        [Test]
        public async Task CanDoIt()
        {
            await _upsertHelper.Upsert(new[]
            {
                new SomeClassWithNullables(1),
                new SomeClassWithNullables(2, true, 8, 10, 200, 300, 0.5f , 0.6, 0.7m),
                new SomeClassWithNullables(3, false, 9, 11, 201, 301, 0.6f , 0.7, 0.8m),
            });

            var rows = _upsertHelper.LoadAll().OrderBy(r => r.Id).ToList();

            Assert.That(rows.Select(r => r.Id), Is.EqualTo(new[] { 1, 2, 3 }));
            Assert.That(rows.Select(r => r.BoolOrNull), Is.EqualTo(new bool?[] { null, true, false }));
            Assert.That(rows.Select(r => r.ByteOrNull), Is.EqualTo(new byte?[] { null, 8, 9 }));
            Assert.That(rows.Select(r => r.ShortOrNull), Is.EqualTo(new short?[] { null, 10, 11 }));
            Assert.That(rows.Select(r => r.IntOrNull), Is.EqualTo(new int?[] { null, 200, 201 }));
            Assert.That(rows.Select(r => r.LongOrNull), Is.EqualTo(new long?[] { null, 300, 301 }));
            Assert.That(rows.Select(r => r.FloatOrNull), Is.EqualTo(new float?[] { null, 0.5f, 0.6f }));
            Assert.That(rows.Select(r => r.DoubleOrNull), Is.EqualTo(new double?[] { null, 0.6, 0.7 }));
            Assert.That(rows.Select(r => r.DecimalOrNull), Is.EqualTo(new decimal?[] { null, 0.7m, 0.8m }));
        }

        class SomeClassWithNullables
        {
            public SomeClassWithNullables(int id,
                bool? boolOrNull = null,
                byte? byteOrNull = null,
                short? shortOrNull = null,
                int? intOrNull = null,
                long? longOrNull = null,
                float? floatOrNull = null,
                double? doubleOrNull = null,
                decimal? decimalOrNull = null)
            {
                Id = id;
                BoolOrNull = boolOrNull;
                ByteOrNull = byteOrNull;
                ShortOrNull = shortOrNull;
                IntOrNull = intOrNull;
                LongOrNull = longOrNull;
                FloatOrNull = floatOrNull;
                DoubleOrNull = doubleOrNull;
                DecimalOrNull = decimalOrNull;
            }

            public int Id { get; }

            public bool? BoolOrNull { get; }
            public byte? ByteOrNull { get; }

            public short? ShortOrNull { get; }
            public int? IntOrNull { get; }
            public long? LongOrNull { get; }

            public float? FloatOrNull { get; }
            public double? DoubleOrNull { get; }
            public decimal? DecimalOrNull { get; }
        }
    }
}