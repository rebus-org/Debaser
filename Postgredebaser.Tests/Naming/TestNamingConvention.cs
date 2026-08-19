using Postgredebaser.Mapping;

// ReSharper disable UnusedMember.Local
// ReSharper disable ClassNeverInstantiated.Local

namespace Postgredebaser.Tests.Naming;

[TestFixture]
public class TestNamingConvention : FixtureBase
{
    [Test]
    public void ConvertsPropertyNamesToSnakeCase()
    {
        var classMap = new AutoMapper().GetMap(typeof(NamesOfAllShapes));

        Assert.That(classMap.Properties.Select(p => p.ColumnName), Is.EqualTo([
            "id",
            "order_line",
            "http_status",
            "customer_id",
            "address1",
            "io_stream"
        ]));
    }

    [Test]
    public void LeavesPropertyNamesAlone()
    {
        var classMap = new AutoMapper().GetMap(typeof(NamesOfAllShapes));

        Assert.That(classMap.Properties.Select(p => p.PropertyName), Is.EqualTo([
            nameof(NamesOfAllShapes.Id),
            nameof(NamesOfAllShapes.OrderLine),
            nameof(NamesOfAllShapes.HTTPStatus),
            nameof(NamesOfAllShapes.CustomerID),
            nameof(NamesOfAllShapes.Address1),
            nameof(NamesOfAllShapes.IOStream)
        ]));
    }

    class NamesOfAllShapes
    {
        public int Id { get; set; }

        public string OrderLine { get; set; }

        public string HTTPStatus { get; set; }

        public Guid CustomerID { get; set; }

        public string Address1 { get; set; }

        public string IOStream { get; set; }
    }

    [Test]
    public void PreservesUnderscoresAlreadyThere()
    {
        var classMap = new AutoMapper().GetMap(typeof(AlreadyHasUnderscores));

        Assert.That(classMap.Properties.Select(p => p.ColumnName), Is.EqualTo([
            "id",
            "already_snake_case"
        ]));
    }

    class AlreadyHasUnderscores
    {
        public int Id { get; set; }

        public string AlreadySnake_Case { get; set; }
    }

    [Test]
    public void SeparatesDigitsFromTheWordFollowingThem()
    {
        var classMap = new AutoMapper().GetMap(typeof(NamesWithDigits));

        Assert.That(classMap.Properties.Select(p => p.ColumnName), Is.EqualTo([
            "id",
            "sha256_hash",
            "ipv4_address",
            "utf8_encoding",
            "x509_certificate",
            "address1_line"
        ]));
    }

    class NamesWithDigits
    {
        public int Id { get; set; }

        public string Sha256Hash { get; set; }

        public string Ipv4Address { get; set; }

        public string Utf8Encoding { get; set; }

        public string X509Certificate { get; set; }

        public string Address1Line { get; set; }
    }
}
