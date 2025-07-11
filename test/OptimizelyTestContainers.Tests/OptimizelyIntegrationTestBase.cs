using EPiServer.Data;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Optimizely.TestContainers;
using Testcontainers.MsSql;

namespace OptimizelyTestContainers.Tests;

public class OptimizelyIntegrationTestBase(bool includeCommerce) : IAsyncLifetime
{
    private IHost _host = null!;

    private MsSqlContainer _cmsDbContainer = null!;

    private MsSqlContainer _commerceDbContainer = null!;
    
    protected IServiceProvider Services { get; private set; } = null!;

    public virtual async Task InitializeAsync()
    {
        // Create Cms SQL Server container
        _cmsDbContainer = CreateNamedSqlContainer( "Cms");
        
        // Create Commerce SQL Server container
        if (includeCommerce)
        {
            _commerceDbContainer = CreateNamedSqlContainer("Commerce");
        }
        
        // Start database containers
        await _cmsDbContainer.StartAsync();

        if (includeCommerce)
        {
            await _commerceDbContainer.StartAsync();
        }

        // Build CMS host
        _host = Host.CreateDefaultBuilder()
            .ConfigureCmsDefaults()
            .ConfigureWebHostDefaults(webHostBuilder =>
            {
                webHostBuilder.ConfigureServices((context, services) =>
                {
                    services.Configure<DataAccessOptions>(opt =>
                    {
                        var containerConnectionString = _cmsDbContainer.GetConnectionString();

                        opt.SetConnectionString(containerConnectionString);
                    });
            
                    // Add data importer service to setup default content for the tests
                    services.AddTransient<OptimizelyDataImporter>();
                });

                webHostBuilder.ConfigureAppConfiguration((context, configBuilder) =>
                {
                    var testSettings = new Dictionary<string, string?>
                    {
                        // TODO: Find Constant for connection string!
                        ["ConnectionStrings:EcfSqlConnection"] = _commerceDbContainer.GetConnectionString()
                    };

                    configBuilder.AddInMemoryCollection(testSettings);
                });

                if (includeCommerce)
                {
                    webHostBuilder.UseStartup<StartupWithCmsAndCommerce>();
                }
                else
                {
                    webHostBuilder.UseStartup<StartupWithCms>(); 
                }

            })
            .Build();
        
        // Run initialization engine (simulate application startup)
        var initializer = _host.Services.GetRequiredService<InitializationEngine>();
        if (initializer.InitializationState != InitializationState.Initialized)
            initializer.Initialize(); 
        
        Services = _host.Services;
        
        await _host.StartAsync();
    }
    
    public async Task DisposeAsync()
    {
        await _host.StopAsync();
        
        await _cmsDbContainer.DisposeAsync();

        if (includeCommerce)
        {
            await _commerceDbContainer.DisposeAsync();
        }
    }
    
    private MsSqlContainer CreateNamedSqlContainer(string componentName)
    {
        return new MsSqlBuilder()
            .WithName(componentName + GetType()) // Unique name per test class and component
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .WithPassword("yourStrong(!)Password")
            .Build();
    }
}