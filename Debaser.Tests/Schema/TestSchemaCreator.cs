using System.Linq;
using Debaser.Internals.Schema;
using Debaser.Mapping;
using NUnit.Framework;

namespace Debaser.Tests.Schema
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

            var creator = new SchemaManager(ConnectionString, "testtable", "testdata", "testproc", properties, schema: "bimse");

            creator.DropSchema(true, true, true);
            creator.CreateSchema(true, true, true);
        }

        class SomeClass
        {
            public int Id { get; set; }
            public string Text { get; set; }
        }
    }
}