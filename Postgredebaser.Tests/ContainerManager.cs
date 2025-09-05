using Debaser.Core.Internals.Tasks;
using Testcontainers.PostgreSql;

namespace Postgredebaser.Tests;

[SetUpFixture]
public class ContainerManager
{
    const string LocalConnectionString = "host=localhost; database=postgredebaser_test; user id=user; password=password;";
    
    static readonly string[] MachineNamesWithLocallyRunningPostgres = ["MHG-PC"];
    
    static readonly Lazy<PostgreSqlContainer> LazyContainer = new(() =>
    {
        var container = new PostgreSqlBuilder()
            .WithUsername("user")
            .WithPassword("password")
            .Build();

        AsyncHelpers.RunSync(() => container.StartAsync());
       
        return container;
    });
    
    internal static string ConnectionString => MachineNamesWithLocallyRunningPostgres.Contains(Environment.MachineName) 
        ? LocalConnectionString 
        : LazyContainer.Value.GetConnectionString();

    [OneTimeTearDown]
    public void StopContainer()
    {
        if (!LazyContainer.IsValueCreated) return;

        AsyncHelpers.RunSync(() => LazyContainer.Value.StopAsync());
    }
}