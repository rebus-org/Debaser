using System.Configuration;
using NUnit.Framework;

namespace Debaser.Tests
{
    public abstract class FixtureBase
    {
        protected static string ConnectionString => ConfigurationManager.ConnectionStrings["db"].ConnectionString;

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