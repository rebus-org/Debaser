using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Debaser.Attributes;
using NUnit.Framework;
// ReSharper disable ArgumentsStyleLiteral

namespace Debaser.Tests.Revisioning
{
    [TestFixture]
    public class TestRevisioning : FixtureBase
    {
        UpsertHelper<Price> _upsertHelper;

        protected override void SetUp()
        {
            _upsertHelper = new UpsertHelper<Price>(ConnectionString);
            _upsertHelper.DropSchema(dropProcedure: true, dropTable: true, dropType: true);
            _upsertHelper.CreateSchema();
        }

        [Test]
        public async Task CanStoreRevisions()
        {
            // arrange
            var today = new Date(2017, 02, 24);
            await _upsertHelper.Upsert(new[] { new Price("P1", today, 10), });

            // act
            await _upsertHelper.Upsert(new[] { new Price("P1", today, 11), });

            // assert
            var rows = await Load();

            Assert.That(rows.Count, Is.EqualTo(2));

            Assert.That(rows.Select(r => r.PriceSource), Is.EqualTo(new[] { "P1", "P1" }));
            Assert.That(rows.Select(r => r.Day), Is.EqualTo(new[] { today, today }));
            Assert.That(rows.Select(r => r.Value), Is.EqualTo(new[] { 10, 11 }));
            Assert.That(rows.Select(r => r.Revision), Is.EqualTo(new[] { 0, 1 }));
        }

        static async Task<List<Price>> Load()
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT * FROM [Prices]";
                    using (var reader = command.ExecuteReader())
                    {
                        var list = new List<Price>();

                        while (reader.Read())
                        {
                            var priceSource = (string)reader["PriceSource"];
                            var value = (decimal)reader["Value"];
                            var dateTime = (DateTime)reader["Day"];
                            var date = new Date(dateTime.Year, dateTime.Month, dateTime.Day);
                            var revision = reader["Revision"] == DBNull.Value ? null : (int?)reader["Revision"];
                            var price = new Price(priceSource, date, value, revision);
                            list.Add(price);
                        }

                        return list;
                    }
                }
            }
        }

        [DebaserTableName("Prices")]
        class Price
        {
            public Price(string priceSource, Date day, decimal value, int? revision = null)
            {
                PriceSource = priceSource;
                Day = day;
                Value = value;
                Revision = revision;
            }

            [DebaserColumnName("PriceSource")]
            [DebaserKey]
            public string PriceSource { get; }

            [DebaserColumnName("Day")]
            [DebaserKey]
            [DebaserMapper(typeof(DateMapper))]
            public Date Day { get; }

            [DebaserColumnName("Value")]
            public decimal Value { get; }

            [DebaserColumnName("Revision")]
            [DebaserRevision]
            public int? Revision { get; }
        }

        class Date : IEquatable<Date>
        {
            public Date(int year, int month, int day)
            {
                Year = year;
                Month = month;
                Day = day;
            }

            public int Year { get; }
            public int Month { get; }
            public int Day { get; }

            public bool Equals(Date other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return Year == other.Year && Month == other.Month && Day == other.Day;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((Date)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    var hashCode = Year;
                    hashCode = (hashCode * 397) ^ Month;
                    hashCode = (hashCode * 397) ^ Day;
                    return hashCode;
                }
            }

            public static bool operator ==(Date left, Date right)
            {
                return Equals(left, right);
            }

            public static bool operator !=(Date left, Date right)
            {
                return !Equals(left, right);
            }
        }

        class DateMapper : IDebaserMapper
        {
            public SqlDbType SqlDbType => SqlDbType.DateTime;
            public int? SizeOrNull => null;
            public int? AdditionalSizeOrNull => null;

            public object ToDatabase(object arg)
            {
                var date = (Date)arg;
                return new DateTime(date.Year, date.Month, date.Day);
            }

            public object FromDatabase(object arg)
            {
                var dateTime = (DateTime)arg;
                return new Date(dateTime.Year, dateTime.Month, dateTime.Day);
            }
        }

    }
}