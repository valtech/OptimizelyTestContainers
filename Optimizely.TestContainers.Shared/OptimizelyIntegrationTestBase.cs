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

    // Since we use same container with different db names we can remove one of these :P
    private MsSqlContainer _databaseContainer = null!;

    protected IServiceProvider Services { get; private set; } = null!;
    
    public virtual async Task InitializeAsync()
    {
        // Create Cms SQL Server container
        var container = await CreateDatabaseContainer();
        
        // Create CMS databse
        var cmsDatabaseConnectionString = await CreateNamedDatabaseConnectionString(container, "Cms");

        string? commerceDatabaseConnectionString = null;
        if (includeCommerce)
        {
            commerceDatabaseConnectionString = await CreateNamedDatabaseConnectionString(container, "Commerce");
        }
        
        
        // Build CMS host
        _host = Host.CreateDefaultBuilder()
           .ConfigureWebHostDefaults(webHostBuilder =>
            {
                webHostBuilder
                    .ConfigureServices((context, services) =>
                    {
                        // TODO: Only include in CMS test
                        // Add data importer service to setup default content for the tests
                       //services.AddTransient<OptimizelyDataImporter>();
                        
                        // Must be set here too for initialization to work for CMS
                        services.Configure<DataAccessOptions>(o =>
                        {
                            o.SetConnectionString(cmsDatabaseConnectionString);
                        });
                    })
                    .ConfigureAppConfiguration((context, configBuilder) =>
                    {
                        // Workaround to set separate database names inisde container
                        /*if (includeCommerce && !string.IsNullOrWhiteSpace(commerceDatabaseConnectionString))
                        {*/
                            var testSettings = new Dictionary<string, string?>
                            {
                                // TODO: Find Constant for connection string!
                                ["ConnectionStrings:EPiServerDB"] = cmsDatabaseConnectionString,
                                ["ConnectionStrings:EcfSqlConnection"] = commerceDatabaseConnectionString,
                            };

                            configBuilder.AddInMemoryCollection(testSettings);
                        /*}*/
                    });
                
                ConfiureWebHostBuilder(webHostBuilder); 
                
                // TOOD: Run startup in each test project!
                /*if (includeCommerce && !string.IsNullOrWhiteSpace(commerceDatabaseConnectionString))
                {
                    webHostBuilder.UseStartup<Optimizely.TestContainers.Startup>();
                }
                else
                {
                    webHostBuilder.UseStartup<StartupWithCms>(); 
                }*/
                
                

            })
            .ConfigureCmsDefaults()
           .Build();

        // TOOD:Try to remove this as well!
        //AssemblyScanner.ExcludedAssemblies.Add("Mediachase");
        //AssemblyScanner.ExcludedAssemblies.Add("EPiServer.Commerce");
        
        // Run initialization engine (simulate application startup) 
        var initializer = _host.Services.GetRequiredService<InitializationEngine>();
        if (initializer.InitializationState != InitializationState.Initialized)
            initializer.Initialize(); 
        
        Services = _host.Services;
        
        await _host.StartAsync();
    }

    protected abstract void ConfiureWebHostBuilder(IWebHostBuilder webHostBuilder);

    public async Task DisposeAsync()
    {
        await _host.StopAsync();
        
        await _databaseContainer.DisposeAsync();
    }
    
    private async Task<MsSqlContainer> CreateDatabaseContainer()
    {
        var container = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .WithPassword("yourStrong(!)Password")
            .Build();

        await container.StartAsync();
        
        _databaseContainer = container;
        
        return _databaseContainer;
    }

    private async Task<string> CreateNamedDatabaseConnectionString(MsSqlContainer container, string databaseName)
    {
        databaseName = $"{GetType().Name}-{databaseName}";
        
        var masterConnectionString = container.GetConnectionString();
        await using var connection = new SqlConnection(masterConnectionString);
        await connection.OpenAsync();
        await using var command = new SqlCommand($"CREATE DATABASE [{databaseName}]", connection);
        await command.ExecuteNonQueryAsync();

        // Workaround to set separate database names inside container
        return masterConnectionString.Replace("master", databaseName);
    }
}