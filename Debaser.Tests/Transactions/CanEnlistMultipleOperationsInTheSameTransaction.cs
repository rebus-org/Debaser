using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace Debaser.Tests.Transactions;

[TestFixture]
public class CanEnlistMultipleOperationsInTheSameTransaction : FixtureBase
{
    UpsertHelper<SomethingToSave> _helper;

    protected override void SetUp()
    {
        base.SetUp();

        _helper = new UpsertHelper<SomethingToSave>(ConnectionString);

        var helper = new UpsertHelper<SomethingToSave>(ConnectionString);

        helper.DropSchema(dropProcedure: true, dropTable: true, dropType: true);
        helper.CreateSchema();
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task CanDoIt(bool commit)
    {
        async Task DoStuffWithTransaction(bool shouldCommit)
        {
            await using var connection = new SqlConnection(ConnectionString);
            await connection.OpenAsync();

            await using var transaction = connection.BeginTransaction();

            await _helper.UpsertAsync(connection, [new SomethingToSave("id2")],
                transaction: transaction);
            await _helper.UpsertAsync(connection, [new SomethingToSave("id3")],
                transaction: transaction);

            if (shouldCommit)
            {
                await transaction.CommitAsync();
            }
        }

        // insert row
        await _helper.UpsertAsync([new SomethingToSave("id1")]);

        await DoStuffWithTransaction(shouldCommit: commit);

        var allRows = _helper.LoadAll().ToList();

        if (commit)
        {
            Assert.That(allRows.Count, Is.EqualTo(3));
            Assert.That(allRows.OrderBy(r => r.Id).Select(r => r.Id), Is.EqualTo(new[] { "id1", "id2", "id3" }));
        }
        else
        {
            Assert.That(allRows.Count, Is.EqualTo(1));
            Assert.That(allRows.OrderBy(r => r.Id).Select(r => r.Id), Is.EqualTo(new[] { "id1" }));
        }
    }

    class SomethingToSave
    {
        public string Id { get; }

        public SomethingToSave(string id) => Id = id;
    }
}