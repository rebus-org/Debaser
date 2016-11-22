using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Debaser.Tests
{
    [TestFixture]
    public class TestUpsertHelper : FixtureBase
    {
        UpsertHelper<SimpleRow> _upsertHelper;

        protected override void SetUp()
        {
            _upsertHelper = new UpsertHelper<SimpleRow>("db");

            _upsertHelper.DropSchema();
            _upsertHelper.CreateSchema();
        }

        [Test]
        public async Task CanRoundtripSingleRow()
        {
            await _upsertHelper.Upsert(new[] { new SimpleRow() });

            var rows = _upsertHelper.Load().ToList();

            Assert.That(rows.Count, Is.EqualTo(1));
        }

        class SimpleRow
        {
            public int Id { get; set; }

            public string Text { get; set; }
        }
    }
}
