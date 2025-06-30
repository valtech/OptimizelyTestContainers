using EPiServer.Cms.Shell;
using EPiServer.Data;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.Framework.Web;
using EPiServer.Web;
using EPiServer.Web.Templating;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Optimizely.TestContainers;
using Testcontainers.MsSql;

namespace OptimizelyTestContainers.Tests;

public class OptimizelyIntegrationTestBase : IAsyncLifetime
{
    private IHost _host = null!;
    private MsSqlContainer _dbContainer  = null!;
    
    public IServiceProvider Services { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        // Start SQL Server container
        _dbContainer = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .WithPassword("yourStrong(!)Password")
            .Build();

        await _dbContainer.StartAsync();

        // Build CMS host
        _host = Host.CreateDefaultBuilder()
            .ConfigureCmsDefaults()
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.ConfigureServices((context, services) =>
                {
                    // Add CMS services
                    services.AddCms();
                    services.AddCmsHost();
                    services.AddCmsFrameworkWeb();
                    services.AddCmsCoreWeb(); 
                    services.AddCmsTemplating();
                    services.AddCmsUI();

                    // Override connection string to use container connection
                    services.Configure<DataAccessOptions>(opt =>
                    {
                        var containerConnectionString = _dbContainer.GetConnectionString();

                        opt.SetConnectionString(containerConnectionString);
                    });

                    // Add data importer service to setup default content for the tests
                    services.AddTransient<OptimizelyDataImporter>();
                });
                
                // Use the Alloy startup by default
                webBuilder.UseStartup<Startup>();
                
            })
            .Build();
        
        await _host.StartAsync();

        // Run initialization engine (simulate application startup)
        var initializer = _host.Services.GetRequiredService<InitializationEngine>();
        if (initializer.InitializationState != InitializationState.Initialized)
            initializer.Initialize();

        Services = _host.Services;
    }

    public async Task DisposeAsync()
    {
        await _host.StopAsync();
        await _dbContainer.DisposeAsync();
    }
}