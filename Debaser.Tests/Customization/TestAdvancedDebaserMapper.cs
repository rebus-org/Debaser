using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Debaser.Attributes;
using NUnit.Framework;

namespace Debaser.Tests.Customization
{
    [TestFixture]
    public class TestAdvancedDebaserMapper : FixtureBase
    {
        UpsertHelper<RowWithMoney> _upsertHelper;

        protected override void SetUp()
        {
            _upsertHelper = new UpsertHelper<RowWithMoney>(ConnectionString);

            _upsertHelper.DropSchema(dropTable: true, dropProcedure: true, dropType: true);
            _upsertHelper.CreateSchema();
        }

        [Test]
        public async Task CanRoundtripRowWithMoney()
        {
            var originalRow = new RowWithMoney(1, new Money(100, new Currency("DKK")));

            await _upsertHelper.UpsertAsync(new[] { originalRow });

            var roundtrippedRow = _upsertHelper.LoadAll().First();

            Assert.That(roundtrippedRow.Id, Is.EqualTo(1));
            Assert.That(roundtrippedRow.Amount.Amount, Is.EqualTo(100));
            Assert.That(roundtrippedRow.Amount.Currency.CurrencyCode, Is.EqualTo("DKK"));

        }

        class RowWithMoney
        {
            [DebaserKey]
            public int Id { get; }

            [DebaserMapper(typeof(MoneyMapper))]
            public Money Amount { get; }

            public RowWithMoney(int id, Money amount)
            {
                Id = id;
                Amount = amount;
            }
        }

        class MoneyMapper : IDebaserMapper2
        {
            const string AmountColumnName = "Amount";
            const string CurrencyColumnName = "Currency";

            public IEnumerable<ColumnSpec> GetColumnSpecs()
            {
                return new[]
                {
                    new ColumnSpec(AmountColumnName, SqlDbType.Decimal, sizeOrNull: 15, additionalSizeOrNull: 5),
                    new ColumnSpec(CurrencyColumnName, SqlDbType.NVarChar, sizeOrNull: 3),
                };
            }

            public Dictionary<string, object> ToDatabase(object arg)
            {
                var money = arg as Money ?? throw new ArgumentException($"Argument {arg} is not a Money");

                return new Dictionary<string, object>
                {
                    [AmountColumnName] = money.Amount,
                    [CurrencyColumnName] = money.Currency.CurrencyCode
                };
            }

            public object FromDatabase(Dictionary<string, object> arg)
            {
                var amount = (decimal)arg[AmountColumnName];
                var currency = (string)arg[CurrencyColumnName];
                return new Money(amount, new Currency(currency));
            }
        }

        class Money
        {
            public decimal Amount { get; }
            public Currency Currency { get; }

            public Money(decimal amount, Currency currency)
            {
                Amount = amount;
                Currency = currency;
            }
        }

        class Currency
        {
            public string CurrencyCode { get; }

            public Currency(string currencyCode)
            {
                CurrencyCode = currencyCode;
            }
        }
    }
}