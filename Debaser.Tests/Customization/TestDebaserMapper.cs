using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Debaser.Attributes;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Debaser.Tests.Customization;

[TestFixture]
public class TestDebaserMapper : FixtureBase
{
    UpsertHelper<RowWithJson> _upsertHelper;

    protected override void SetUp()
    {
        _upsertHelper = new UpsertHelper<RowWithJson>(ConnectionString);

        _upsertHelper.DropSchema(dropTable: true, dropProcedure: true, dropType: true);
        _upsertHelper.CreateSchema();
    }

    [Test]
    public async Task CanRoundtripJson()
    {
        var rows = new[]
        {
            new RowWithJson(1, new Json("json1")),
            new RowWithJson(2, new Json("json2")),
            new RowWithJson(3, new Json("json3")),
        };

        await _upsertHelper.UpsertAsync(rows);

        var roundtrippedRows = _upsertHelper.LoadAll().OrderBy(r => r.Id).ToList();

        Assert.That(roundtrippedRows.Select(r => r.Json.Text), Is.EqualTo(new[]
        {
            "json1",
            "json2",
            "json3",
        }));
    }

    public class RowWithJson
    {
        public RowWithJson(int id, Json json)
        {
            Id = id;
            Json = json;
        }

        public int Id { get; }

        [DebaserMapper(typeof(JsonMapperino))]
        public Json Json { get; }
    }

    public class Json
    {
        public string Text { get; }

        public Json(string text)
        {
            Text = text;
        }
    }

    class JsonMapperino : IDebaserMapper
    {
        static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings{TypeNameHandling=TypeNameHandling.All};
        public SqlDbType SqlDbType => SqlDbType.NVarChar;
        public int? SizeOrNull => int.MaxValue;
        public int? AdditionalSizeOrNull => null;

        public object ToDatabase(object arg)
        {
            return JsonConvert.SerializeObject(arg, SerializerSettings);
        }

        public object FromDatabase(object arg)
        {
            return JsonConvert.DeserializeObject((string)arg, SerializerSettings);
        }
    }
}