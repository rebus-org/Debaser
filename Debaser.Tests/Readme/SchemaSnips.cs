using Debaser.Attributes;
using NUnit.Framework;

namespace Debaser.Tests.Readme
{
    [TestFixture]
    public class SchemaSnips : FixtureBase
    {
        [Test]
        public void Checkperson()
        {
            var helper = new UpsertHelper<Person>(ConnectionString);
            helper.DropSchema(dropTable: true, dropProcedure: true, dropType: true);
            helper.CreateSchema();
        }

        class Person
        {
            public Person(string ssn, string fullName)
            {
                Ssn = ssn;
                FullName = fullName;
            }

            [DebaserKey]
            public string Ssn { get; }

            public string FullName { get; }
        }

        [Test]
        public void CheckTenantPerson()
        {
            var helper = new UpsertHelper<TenantPerson>(ConnectionString);
            helper.DropSchema(dropTable: true, dropProcedure: true, dropType: true);
            helper.CreateSchema();
        }

        class TenantPerson
        {
            public TenantPerson(string tenantId, string ssn, string fullName)
            {
                TenantId = tenantId;
                Ssn = ssn;
                FullName = fullName;
            }

            [DebaserKey]
            public string TenantId { get; }

            [DebaserKey]
            public string Ssn { get; }

            public string FullName { get; }
        }
    }
}