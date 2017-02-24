using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Debaser.Attributes;
using NUnit.Framework;
// ReSharper disable ArgumentsStyleLiteral

namespace Debaser.Tests.Naming
{
    [TestFixture]
    public class TestNameCustomization : FixtureBase
    {
        UpsertHelper<Customized> _upsertHelper;

        protected override void SetUp()
        {
            _upsertHelper = new UpsertHelper<Customized>(ConnectionString);
            _upsertHelper.DropSchema(dropProcedure: true, dropTable: true, dropType: true);
            _upsertHelper.CreateSchema();
        }

        [Test]
        public async Task ItWorks()
        {
            await _upsertHelper.Upsert(new[]
            {
                new Customized {Id = "id1", SkørtNavn = "hej"},
                new Customized {Id = "id2", SkørtNavn = "hej igen"},
            });

            var rows = LoadWithUpsertHelper();

            Assert.That(rows.Select(r => r.Id), Is.EqualTo(new[] { "id1", "id2" }));
            Assert.That(rows.Select(r => r.SkørtNavn), Is.EqualTo(new[] { "hej", "hej igen" }));

            var rowsAgain = LoadWithPlainSelect();

            Assert.That(rowsAgain.Select(r => r.Id), Is.EqualTo(new[] { "id1", "id2" }));
            Assert.That(rowsAgain.Select(r => r.SkørtNavn), Is.EqualTo(new[] { "hej", "hej igen" }));
        }

        List<Customized> LoadWithPlainSelect()
        {
            var list = new List<Customized>();
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT * FROM [Table 001]";
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new Customized { Id = (string)reader["_id"], SkørtNavn = (string)reader["med mellemrum og æ"] });
                        }
                    }
                }
            }
            return list;
        }

        List<Customized> LoadWithUpsertHelper()
        {
            return _upsertHelper.LoadAll().OrderBy(r => r.Id).ToList();
        }

        [DebaserTableName("Table 001")]
        class Customized
        {
            [DebaserColumnName("_id")]
            public string Id { get; set; }

            [DebaserColumnName("med mellemrum og æ")]
            public string SkørtNavn { get; set; }
        }
    }
}