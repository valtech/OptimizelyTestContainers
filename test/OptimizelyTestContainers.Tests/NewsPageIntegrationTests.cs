using System.Reflection;
using EPiServer;
using EPiServer.Cms.Shell;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using EPiServer.Framework.Initialization;
using EPiServer.Core;
using EPiServer.Data;
using EPiServer.DataAccess;
using EPiServer.Framework;
using EPiServer.Framework.Web;
using EPiServer.Security;
using EPiServer.Web;
using EPiServer.Web.Templating;
using Microsoft.AspNetCore.Hosting;
using Optimizely.TestContainers;
using Optimizely.TestContainers.Models;
using Optimizely.TestContainers.Models.Pages;
using OptimizelyTestContainers.Tests;
using Testcontainers.MsSql;

public class NewsPageIntegrationTest : IAsyncLifetime
{
    private IHost _host;
    private IServiceProvider _services;
    private MsSqlContainer _dbContainer;

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
                    services.AddCms();
                    services.AddCmsHost();
                    services.AddCmsFrameworkWeb();
                    
                    services.AddCmsCoreWeb(); // Adds core CMS services
                    services.AddCmsTemplating();
                    services.AddCmsUI();

                    services.Configure<DataAccessOptions>(opt =>
                    {
                        var cs = _dbContainer.GetConnectionString();

                        opt.SetConnectionString(cs);
                    });

                    services.AddTransient<OptimizelyDataImporter>();
                });
                
                webBuilder.UseStartup<Startup>();
            })
            .Build();

        
        
        
        await _host.StartAsync();

        // Run initialization engine (simulate application startup)
        var initializer = _host.Services.GetRequiredService<InitializationEngine>();
        if (initializer.InitializationState != InitializationState.Initialized)
            initializer.Initialize();

        _services = _host.Services;
    }

    public async Task DisposeAsync()
    {
        await _host.StopAsync();
        await _dbContainer.DisposeAsync();
    }

    [Fact]
    public void Can_Create_And_Read_NewsPage()
    {
        // Arrange
        var repo = _services.GetRequiredService<IContentRepository>();
        
        // Import test data
        var basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        var episerverDataFile = Path.Combine(basePath, "DefaultSiteContent.episerverdata");
        
        var dataImporter = _services.GetRequiredService<OptimizelyDataImporter>();
        dataImporter.Import(episerverDataFile);
        
        
        // Find StartPage from root
        // Setup site definition
        
        var startPage = repo.GetChildren<StartPage>(ContentReference.RootPage).First();
        
        var siteDefinitionRepo = _services.GetRequiredService<ISiteDefinitionRepository>();
        
        siteDefinitionRepo.Save(new SiteDefinition()
        {
            Name = "TestSite",
            StartPage = startPage.ContentLink,
            SiteUrl = new Uri("http://localhost"),
        });

        var allSites = siteDefinitionRepo.List();

        var site = allSites.First();
        
        var news = repo.GetDefault<NewsPage>(site.StartPage);
        news.Name = "Alien Invasion";
        news.Title = "Martians Landed in Stockholm";

        // Act
        var savedRef = repo.Save(news, SaveAction.Publish, AccessLevel.NoAccess);
        var loaded = repo.Get<NewsPage>(savedRef);

        // Assert
        Assert.NotNull(loaded);
        Assert.Equal("Martians Landed in Stockholm", loaded.Title);
    }
}
