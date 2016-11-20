using NUnit.Framework;

namespace Debaser.Tests
{
    public abstract class FixtureBase
    {
        [SetUp]
        public void InnerSetUp()
        {
            SetUp();
        }

        protected virtual void SetUp()
        {
            
        }
    }
}