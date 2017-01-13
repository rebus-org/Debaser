using NUnit.Framework;

namespace Debaser.Tests.Query
{
    [TestFixture]
    public class TestDeleting : FixtureBase
    {
        UpsertHelper<RowWithData> _upsertHelper;

        protected override void SetUp()
        {
            _upsertHelper = new UpsertHelper<RowWithData>("db");

            _upsertHelper.DropSchema();
            _upsertHelper.CreateSchema();
        }

        class RowWithData
        {

        }
    }
}