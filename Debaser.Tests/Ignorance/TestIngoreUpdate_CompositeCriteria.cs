﻿using Debaser.Attributes;

namespace Debaser.Tests.Ignorance;

[TestFixture]
public class TestIngoreUpdate_CompositeCriteria : FixtureBase
{
    UpsertHelper<SomeRowWithCompositeRevision> _upsertHelper;

    protected override void SetUp()
    {
        _upsertHelper = new UpsertHelper<SomeRowWithCompositeRevision>(ConnectionString);
        _upsertHelper.DropSchema(dropTable: true, dropProcedure: true, dropType: true);
        _upsertHelper.CreateSchema();
    }

    [Test]
    public async Task AllUpdateCriteriaMustBeFulfilledBeforeAnUpdateIsCarriedOut()
    {
        var t0 = DateTime.Now;
        var t1 = t0.AddSeconds(1);
        var tminus1 = t0.AddSeconds(-1);

        var r0 = 0;
        var r1 = 1;
        var rminus1 = -1;

        await _upsertHelper.UpsertAsync([new SomeRowWithCompositeRevision(1, "original", r0, t0)]);

        AssertDataIs("original");

        await _upsertHelper.UpsertAsync([new SomeRowWithCompositeRevision(1, "time not incremented", r1, t0)]);

        AssertDataIs("original");

        await _upsertHelper.UpsertAsync([new SomeRowWithCompositeRevision(1, "time decremented", r1, tminus1)]);

        AssertDataIs("original");

        await _upsertHelper.UpsertAsync([new SomeRowWithCompositeRevision(1, "revision not incremented", r0, t1)]);

        AssertDataIs("original");

        await _upsertHelper.UpsertAsync([new SomeRowWithCompositeRevision(1, "revision decrements", rminus1, t1)]);

        AssertDataIs("original");

        await _upsertHelper.UpsertAsync([new SomeRowWithCompositeRevision(1, "FINALLY", r1, t1)]);

        AssertDataIs("FINALLY");
    }

    void AssertDataIs(string expectedData)
    {
        Assert.That(_upsertHelper.LoadAll().Single().Data, Is.EqualTo(expectedData));
    }

    [DebaserUpdateCriteria("[S].[Rev] > [T].[Rev]")]
    [DebaserUpdateCriteria("[S].[LastUpdated] > [T].[LastUpdated]")]
    class SomeRowWithCompositeRevision(int id, string data, int rev, DateTime lastUpdated)
    {
        public int Id { get; } = id;

        public string Data { get; } = data;

        public int Rev { get; } = rev;

        public DateTime LastUpdated { get; } = lastUpdated;
    }
}