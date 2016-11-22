using System;
using System.Data;
using System.Linq;
using Debaser.Mapping;
using NUnit.Framework;

namespace Debaser.Tests.Mapping
{
    [TestFixture]
    public class TestAutoMapper : FixtureBase
    {
        AutoMapper _mapper;

        protected override void SetUp()
        {
            _mapper = new AutoMapper();
        }

        [Test]
        public void CanGetMapFromPoco()
        {
            var classMap = _mapper.GetMap(typeof(Poco));

            Assert.That(classMap.Type, Is.EqualTo(typeof(Poco)));
            Assert.That(classMap.Properties.Count(), Is.EqualTo(3));
            Assert.That(classMap.Properties.Select(p => p.Name), Is.EqualTo(new[]
            {
                nameof(Poco.Id),
                nameof(Poco.Decimal),
                nameof(Poco.DateTime),
            }));
            Assert.That(classMap.Properties.Select(p => p.ColumnInfo.SqlDbType), Is.EqualTo(new[]
            {
                SqlDbType.Int,
                SqlDbType.Decimal,
                SqlDbType.DateTime2,
            }));
        }

        class Poco
        {
            public int Id { get; set; }

            public decimal Decimal { get; set; }

            public DateTime DateTime { get; set; }
        }
    }
}