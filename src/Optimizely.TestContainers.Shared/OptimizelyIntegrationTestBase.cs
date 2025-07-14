using EPiServer.Data;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Testcontainers.MsSql;

namespace Optimizely.TestContainers.Shared;

public abstract class OptimizelyIntegrationTestBase(bool includeCommerce) : IAsyncLifetime
{
    private IHost _host = null!;

    private MsSqlContainer _databaseContainer = null!;

    protected IServiceProvider Services { get; private set; } = null!;
    
    public virtual async Task InitializeAsync()
    {
        // Create SQL Server container
        var container = await CreateDatabaseContainer();
        
        // Create CMS database
        var cmsDatabaseConnectionString = await CreateNamedDatabase(container, "Cms");

        string? commerceDatabaseConnectionString = null;
        if (includeCommerce)
        {
            // Create Commerce database
            commerceDatabaseConnectionString = await CreateNamedDatabase(container, "Commerce");
        }
        
        // Build CMS host
        _host = Host.CreateDefaultBuilder()
           .ConfigureWebHostDefaults(webHostBuilder =>
            {
                webHostBuilder
                    .ConfigureServices((context, services) =>
                    {
                        // Must be set here too for initialization to work for CMS
                        services.Configure<DataAccessOptions>(o =>
                        {
                            o.SetConnectionString(cmsDatabaseConnectionString);
                        });
                    })
                    .ConfigureAppConfiguration((context, configBuilder) =>
                    {
                        var testSettings = new Dictionary<string, string?>
                        {
                            ["ConnectionStrings:EPiServerDB"] = cmsDatabaseConnectionString,
                            ["ConnectionStrings:EcfSqlConnection"] = commerceDatabaseConnectionString,
                        };

                        configBuilder.AddInMemoryCollection(testSettings);
                    });
                
                // To configure apps separately with Cms and Commerce Startup files in separate projects
                ConfigureWebHostBuilder(webHostBuilder); 
            })
            .ConfigureCmsDefaults()
           .Build();
        
        // Run initialization engine (simulate application startup) 
        var initializer = _host.Services.GetRequiredService<InitializationEngine>();
        if (initializer.InitializationState != InitializationState.Initialized)
            initializer.Initialize(); 
        
        Services = _host.Services;
        
        await _host.StartAsync();
    }

    protected abstract void ConfigureWebHostBuilder(IWebHostBuilder webHostBuilder);

    public async Task DisposeAsync()
    {
        await _host.StopAsync();
        
        await _databaseContainer.DisposeAsync();
    }
    
    private async Task<MsSqlContainer> CreateDatabaseContainer()
    {
        _databaseContainer = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .WithPassword("yourStrong(!)Password")
            .Build();

        await _databaseContainer.StartAsync();
        
        return _databaseContainer;
    }

    private async Task<string> CreateNamedDatabase(MsSqlContainer container, string databaseName)
    {
        var masterConnectionString = container.GetConnectionString();
        await using var connection = new SqlConnection(masterConnectionString);
        await connection.OpenAsync();
        await using var command = new SqlCommand($"CREATE DATABASE [{GetType().Name}-{databaseName}]", connection);
        await command.ExecuteNonQueryAsync();

        // Workaround to set separate database names inside container
        return masterConnectionString.Replace("master", databaseName);
    }
}