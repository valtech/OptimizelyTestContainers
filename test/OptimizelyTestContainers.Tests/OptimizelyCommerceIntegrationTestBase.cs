using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Testcontainers.MsSql;

namespace OptimizelyTestContainers.Tests;

public class OptimizelyCommerceIntegrationTestBase : OptimizelyCmsIntegrationTestBase
{
    public MsSqlContainer CommerceDbContainer { get; private set; } = null!;

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        
        // Start SQL Server container
        CommerceDbContainer = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .WithPassword("yourStrong(!)Password")
            .Build();
        
        await CommerceDbContainer.StartAsync();
    }

    public override void CustomizebHostDetaults(IWebHostBuilder webBuilder)
    {

        webBuilder.ConfigureAppConfiguration((context, configBuilder) =>
        {
            var testSettings = new Dictionary<string, string?>
            {
                //["ConnectionStrings:EPiServerDB"] = CmsDbContainer.GetConnectionString(),
                ["ConnectionStrings:EcfSqlConnection"] = CommerceDbContainer.GetConnectionString(),
            };

            configBuilder.AddInMemoryCollection(testSettings);
        });
        
        base.CustomizebHostDetaults(webBuilder);
    }

    public override void CustomizeStartup()
    {
        // TOOD: No-op for now
        base.CustomizeStartup();
    }

    public override async Task DisposeAsync()
    {
        await CommerceDbContainer.DisposeAsync();
        
        await base.DisposeAsync();
    }
}