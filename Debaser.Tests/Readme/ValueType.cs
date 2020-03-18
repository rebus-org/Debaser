using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Debaser.Attributes;
using NUnit.Framework;

namespace Debaser.Tests.Readme
{
    [TestFixture]
    public class ValueType : FixtureBase
    {
        UpsertHelper<CurrencyCrossRates> _upsertHelper;

        protected override void SetUp()
        {
            _upsertHelper = new UpsertHelper<CurrencyCrossRates>(ConnectionString);
            _upsertHelper.DropSchema(dropTable: true, dropProcedure: true, dropType: true);
            _upsertHelper.CreateSchema();
        }

        [Test]
        public async Task ItWorks()
        {
            await _upsertHelper.UpsertAsync(new[]
            {
                new CurrencyCrossRates(new Date(2017, 1, 17), "EUR", "USD", 5.5m),
            });

            var rows = _upsertHelper.LoadAll().ToList();
            
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

    class CurrencyCrossRates
    {
        public CurrencyCrossRates(Date date, string @base, string quote, decimal rate)
        {
            Date = date;
            Base = @base;
            Quote = quote;
            Rate = rate;
        }

        [DebaserKey]
        [DebaserMapper(typeof(DateMapper))]
        public Date Date { get; }

        [DebaserKey]
        public string Base { get; }

        [DebaserKey]
        public string Quote { get; }

        public decimal Rate { get; }
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
}