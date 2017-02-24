using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
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
            await _upsertHelper.Upsert(new[] { new Price("P1", today, 10), });

            // assert
            var rows = await Load();
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
                            var revision = (int)reader["Revision"];
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

        class Date
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