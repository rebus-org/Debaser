using System.Linq;
using System.Threading.Tasks;
using Debaser.Attributes;
using NUnit.Framework;

namespace Debaser.Tests.Query
{
    [TestFixture]
    public class TestDeleting : FixtureBase
    {
        UpsertHelper<RowWithData> _upsertHelper;

        protected override void SetUp()
        {
            _upsertHelper = new UpsertHelper<RowWithData>("db");

            _upsertHelper.DropSchema(dropTable: true, dropProcedure: true, dropType: true);
            _upsertHelper.CreateSchema();
        }

        [Test]
        public async Task CanDeleteRows()
        {
            var rows = new[]
            {
                new RowWithData("1", "hej"), 
                new RowWithData("2", "hej"), 
                new RowWithData("3", "bum!"), 
                new RowWithData("4", "bum!"), 
                new RowWithData("5", "bum!"), 
                new RowWithData("6", "bum!"), 
                new RowWithData("7", "farvel"), 
                new RowWithData("8", "farvel"), 
                new RowWithData("9", "farvel"), 
            };

            await _upsertHelper.Upsert(rows);

            var allRows = _upsertHelper.LoadAll().ToList();

            await _upsertHelper.DeleteWhere("[data] = @data", new {data = "hej"});

            var rowsAfterDeletingHej = _upsertHelper.LoadAll().ToList();

            await _upsertHelper.DeleteWhere("[data] = @data", new {data = "farvel"});

            var rowsAfterDeletingFarvel = _upsertHelper.LoadAll().ToList();

            Assert.That(allRows.Count, Is.EqualTo(9));

            Assert.That(rowsAfterDeletingHej.Count, Is.EqualTo(7));

            Assert.That(rowsAfterDeletingFarvel.Count, Is.EqualTo(4));
        }

        class RowWithData
        {
            public RowWithData(string id, string data)
            {
                Id = id;
                Data = data;
            }

            [DebaserKey]
            public string Id { get; }
            public string Data { get; }
        }
    }
}