using System;
using System.Linq;
using System.Threading.Tasks;
using Debaser.Attributes;
using NUnit.Framework;

namespace Debaser.Tests.Revisioning
{
    [TestFixture]
    public class TestRevisioning : FixtureBase
    {
        UpsertHelper<SomeRowWithIntegerRevision> _upsertHelper;
        UpsertHelper<SomeRowWithDateTimeRevision> _upsertHelper2;

        protected override void SetUp()
        {
            _upsertHelper = new UpsertHelper<SomeRowWithIntegerRevision>(ConnectionString);
            _upsertHelper.DropSchema(dropTable: true, dropProcedure: true, dropType: true);
            _upsertHelper.CreateSchema();

            _upsertHelper2 = new UpsertHelper<SomeRowWithDateTimeRevision>(ConnectionString);
            _upsertHelper2.DropSchema(dropTable: true, dropProcedure: true, dropType: true);
            _upsertHelper2.CreateSchema();
        }

        [Test]
        public async Task CanCarryOutOrdinaryUpdate()
        {
            await _upsertHelper.Upsert(new[]
            {
                new SomeRowWithIntegerRevision(1, "hej", 0),
                new SomeRowWithIntegerRevision(2, "med", 0),
                new SomeRowWithIntegerRevision(3, "dig", 0)
            });

            var rows = _upsertHelper.LoadAll().OrderBy(r => r.Id).ToList();

            Assert.That(rows.Count, Is.EqualTo(3));
            Assert.That(rows.Select(r => r.Id), Is.EqualTo(new[] { 1, 2, 3 }));
        }

        [Test]
        public async Task CanCarryOutConditionalUpdate()
        {
            await _upsertHelper.Upsert(new[]
            {
                new SomeRowWithIntegerRevision(1, "hej", 0),
                new SomeRowWithIntegerRevision(2, "med", 0),
                new SomeRowWithIntegerRevision(3, "dig", 0)
            });

            Assert.That(_upsertHelper.LoadAll().OrderBy(r => r.Id).Select(r => r.Data), Is.EqualTo(new[] { "hej", "med", "dig" }));

            await _upsertHelper.Upsert(new[]
            {
                new SomeRowWithIntegerRevision(1, "hej", 1),
                new SomeRowWithIntegerRevision(2, "med", 1),
                new SomeRowWithIntegerRevision(3, "mig", 1)
            });

            Assert.That(_upsertHelper.LoadAll().OrderBy(r => r.Id).Select(r => r.Data), Is.EqualTo(new[] { "hej", "med", "mig" }));

            await _upsertHelper.Upsert(new[]
            {
                new SomeRowWithIntegerRevision(1, "hej", 2),
                new SomeRowWithIntegerRevision(2, "med", 2),
                new SomeRowWithIntegerRevision(3, "Frank Frank", 1)
            });

            Assert.That(_upsertHelper.LoadAll().OrderBy(r => r.Id).Select(r => r.Data), Is.EqualTo(new[] { "hej", "med", "mig" }));
        }

        [DebaserUpdateCriteria("[S].[Rev] > [T].[Rev]")]
        class SomeRowWithIntegerRevision
        {
            public SomeRowWithIntegerRevision(int id, string data, int rev)
            {
                Id = id;
                Data = data;
                Rev = rev;
            }

            public int Id { get; }
            public string Data { get; }

            public int Rev { get; }
        }

        [Test]
        public async Task WorksWithDateTimeCriteriaToo()
        {
            var t0 = DateTime.Now;
            var t1 = t0.AddSeconds(1);
            var tminus1 = t0.AddSeconds(-1);

            await _upsertHelper2.Upsert(new[] { new SomeRowWithDateTimeRevision(1, "hej", t0) });

            Assert.That(_upsertHelper2.LoadAll().Single().Data, Is.EqualTo("hej"));

            await _upsertHelper2.Upsert(new[] { new SomeRowWithDateTimeRevision(1, "hej igen", t1) });

            Assert.That(_upsertHelper2.LoadAll().Single().Data, Is.EqualTo("hej igen"));

            await _upsertHelper2.Upsert(new[] { new SomeRowWithDateTimeRevision(1, "DEN HER SKAL IKKE IGENNEM", tminus1) });

            Assert.That(_upsertHelper2.LoadAll().Single().Data, Is.EqualTo("hej igen"));
        }

        //[DebaserRevision(nameof(LastUpdated))]
        [DebaserUpdateCriteria("[S].[LastUpdated] > [T].[LastUpdated]")]
        class SomeRowWithDateTimeRevision
        {
            public SomeRowWithDateTimeRevision(int id, string data, DateTime lastUpdated)
            {
                Id = id;
                Data = data;
                LastUpdated = lastUpdated;
            }

            public int Id { get; }
            public string Data { get; }

            public DateTime LastUpdated { get; }
        }
    }
}