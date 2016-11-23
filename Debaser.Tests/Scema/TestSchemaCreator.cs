using System.Linq;
using Debaser.Mapping;
using Debaser.Schema;
using NUnit.Framework;

namespace Debaser.Tests.Scema
{
    [TestFixture]
    public class TestSchemaCreator : FixtureBase
    {
        [Test]
        public void CanCreateSchema()
        {
            var mapper = new AutoMapper();
            var map = mapper.GetMap(typeof(SomeClass));
            var properties = map.Properties;
            var keyProperties = properties.Where(p => p.IsKey);

            var creator = new SchemaManager(ConnectionString, "testtable", "testdata", "testproc", keyProperties, properties, schema: "bimse");

            creator.DropSchema();
            creator.CreateSchema();
        }

        class SomeClass
        {
            public int Id { get; set; }
            public string Text { get; set; }
        }
    }
}