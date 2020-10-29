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

            await _upsertHelper.UpsertAsync(new[] {originalRow});

            var roundtrippedRow = _upsertHelper.LoadAll().First();

            Assert.That(roundtrippedRow.Id, Is.EqualTo(1));
            Assert.That(roundtrippedRow.Amount.Amount, Is.EqualTo(100));
            Assert.That(roundtrippedRow.Amount.Currency.CurrencyCode, Is.EqualTo("DKK"));

        }

        class RowWithMoney
        {
            [DebaserKey]
            public int Id { get; }

            public Money Amount { get; }

            public RowWithMoney(int id, Money amount)
            {
                Id = id;
                Amount = amount;
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