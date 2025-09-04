using Debaser.Attributes;

namespace Debaser.Tests.Readme;

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

    class Person(string ssn, string fullName)
    {
        [DebaserKey]
        public string Ssn { get; } = ssn;

        public string FullName { get; } = fullName;
    }

    [Test]
    public void CheckTenantPerson()
    {
        var helper = new UpsertHelper<TenantPerson>(ConnectionString);
        helper.DropSchema(dropTable: true, dropProcedure: true, dropType: true);
        helper.CreateSchema();
    }

    class TenantPerson(string tenantId, string ssn, string fullName)
    {
        [DebaserKey]
        public string TenantId { get; } = tenantId;

        [DebaserKey]
        public string Ssn { get; } = ssn;

        public string FullName { get; } = fullName;
    }
}