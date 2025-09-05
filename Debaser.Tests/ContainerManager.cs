using Debaser.Core.Internals.Tasks;
using Testcontainers.MsSql;

namespace Debaser.Tests;

[SetUpFixture]
public class ContainerManager
{
    const string LocalConnectionString = "server=.; database=debaser_test; trusted_connection=true; encrypt=false";
    
    static readonly string[] MachineNamesWithLocallyRunningPostgres = ["MHG-PC"];
    
    static readonly Lazy<MsSqlContainer> LazyContainer = new(() =>
    {
        var container = new MsSqlBuilder().Build();

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