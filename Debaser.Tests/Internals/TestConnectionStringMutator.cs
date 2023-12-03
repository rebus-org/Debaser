using Debaser.Internals.Sql;
using NUnit.Framework;

namespace Debaser.Tests.Internals;

[TestFixture]
public class TestConnectionStringMutator : FixtureBase
{
    [Test]
    public void ItWorks()
    {
        var mutator = new ConnectionStringMutator("server=.; database=bim; user id=hej; password=det={}svært");

        var password = mutator.GetElement("password");
        var connectionString = mutator.ToString();

        Assert.That(password, Is.EqualTo("det={}svært"));
    }
}