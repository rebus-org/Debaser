using Debaser.Core.Attributes;
using Newtonsoft.Json;
using NpgsqlTypes;
using Postgredebaser;

namespace Postgredebaser.Tests.Customization;

[TestFixture]
public class TestDebaserMapper : FixtureBase
{
    UpsertHelper<RowWithJson> _upsertHelper;

    protected override void SetUp()
    {
        _upsertHelper = new UpsertHelper<RowWithJson>(ConnectionString);

        _upsertHelper.DropSchema(dropTable: true);
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

    public class RowWithJson(int id, Json json)
    {
        public int Id { get; } = id;

        [DebaserMapper(typeof(JsonMapperino))]
        public Json Json { get; } = json;
    }

    public class Json(string text)
    {
        public string Text { get; } = text;
    }

    class JsonMapperino : IDebaserMapper
    {
        static readonly JsonSerializerSettings SerializerSettings = new() {TypeNameHandling=TypeNameHandling.All};
        public NpgsqlDbType NpgsqlDbType => NpgsqlDbType.Text;
        public int? SizeOrNull => null;
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