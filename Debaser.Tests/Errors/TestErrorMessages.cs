using System;
using System.Data;
using System.Threading.Tasks;
using Debaser.Attributes;
using NUnit.Framework;
// ReSharper disable ObjectCreationAsStatement

namespace Debaser.Tests.Errors;

[TestFixture]
public class TestErrorMessages : FixtureBase
{
    [Test]
    public void CheckErrorMethodWhenIsIsMissing() => CheckError<MissingId>();

    class MissingId
    {
        public string Text { get; set; }
    }

    [Test]
    public void CheckErrorWhenAmbiguousAttributesAreUsed() => CheckError<BothMapperAndType>();

    class BothMapperAndType
    {
        [DebaserMapper(typeof(string))]
        [DebaserSqlType(SqlDbType.NVarChar, 10)]
        public string Whatever { get; set; }
    }

    [Test]
    public void CheckErrorWhenUnsupportedTypeIsUsed() => CheckError<HasUnsupportedPropertyType>();

    class HasUnsupportedPropertyType
    {
        public string Id { get; set; }
        public ApplicationException ApplicationException { get; set; }
    }

    static void CheckError<T>()
    {
        var exception = Assert.Throws<ArgumentException>(() => new UpsertHelper<T>(ConnectionString));

        Console.WriteLine(exception);
    }
}