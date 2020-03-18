using System.Linq;
using System.Threading.Tasks;
using Debaser.Attributes;
using NUnit.Framework;

namespace Debaser.Tests.Ignorance
{
    [TestFixture]
    public class TestIgnoreAttribute : FixtureBase
    {
        UpsertHelper<HasIgnoredProperty> _helper;

        protected override void SetUp()
        {
            _helper = new UpsertHelper<HasIgnoredProperty>(ConnectionString);

            _helper.DropSchema(dropTable: true, dropType: true, dropProcedure: true);
            _helper.CreateSchema();
        }

        [Test]
        public async Task TrulyIgnoresIgnoredProperty()
        {
            var rows = new[]
            {
                new HasIgnoredProperty{Id = "001", Data="this is data", IgnoredData = "this is ignored data"},
                new HasIgnoredProperty{Id = "002", Data="this is more data", IgnoredData = "this is ignored data"},
            };

            await _helper.UpsertAsync(rows);

            var roundtrippedRows = _helper.LoadAll().OrderBy(r => r.Id).ToList();

            Assert.That(roundtrippedRows.Select(r => r.Id), Is.EqualTo(new[] { "001", "002" }));
            Assert.That(roundtrippedRows.Select(r => r.Data), Is.EqualTo(new[] { "this is data", "this is more data" }));
            Assert.That(roundtrippedRows.Select(r => r.IgnoredData), Is.EqualTo(new string[] { null, null }));
        }

        class HasIgnoredProperty
        {
            public string Id { get; set; }
            public string Data { get; set; }

            [DebaserIgnore]
            public string IgnoredData { get; set; }
        }
    }
}