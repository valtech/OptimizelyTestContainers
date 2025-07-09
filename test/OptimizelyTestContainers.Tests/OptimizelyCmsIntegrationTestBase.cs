using EPiServer.Data;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Optimizely.TestContainers;
using Testcontainers.MsSql;

namespace OptimizelyTestContainers.Tests;

public class OptimizelyCmsIntegrationTestBase : IAsyncLifetime
{
    private IHost _host = null!;
    
    protected MsSqlContainer CmsDbContainer { get; private set; } = null!;
    
    protected IServiceProvider Services { get; private set; } = null!;

    public virtual async Task InitializeAsync()
    {
        // Start SQL Server container
        CmsDbContainer = new MsSqlBuilder()
            .WithName("Cms")
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .WithPassword("yourStrong(!)Password")
            .Build();
        
        await CmsDbContainer.StartAsync();
        
        // Build CMS host
        _host = Host.CreateDefaultBuilder()
            .ConfigureCmsDefaults()
            .ConfigureWebHostDefaults(webHost =>
            {
                CustomizebHostDetaults(webHost);

                webHost.UseStartup<Startup>();
            })
            .Build();
        
        CustomizeStartup();
        
        await _host.StartAsync();
        
        Services = _host.Services;
    }

    public virtual void CustomizebHostDetaults(IWebHostBuilder webBuilder)
    {
        webBuilder.ConfigureServices((context, services) =>
        {
            services.Configure<DataAccessOptions>(opt =>
            {
                var containerConnectionString = CmsDbContainer.GetConnectionString();

                opt.SetConnectionString(containerConnectionString);
            });
            
            // Add data importer service to setup default content for the tests
            services.AddTransient<OptimizelyDataImporter>();
        });
    }

    public virtual void CustomizeStartup()
    {
        // Run initialization engine (simulate application startup)
        var initializer = _host.Services.GetRequiredService<InitializationEngine>();
        if (initializer.InitializationState != InitializationState.Initialized)
            initializer.Initialize();
    }

    public virtual async Task DisposeAsync()
    {
        await _host.StopAsync();
        await CmsDbContainer.DisposeAsync();
    }
}