using System.Reflection;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAccess;
using EPiServer.Security;
using EPiServer.Web;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Optimizely.TestContainers.Shared;
using OptimizelyTestContainers.Tests.Models.Pages;

namespace OptimizelyTestContainers.Tests;

public class NewsPageIntegrationTest() : OptimizelyIntegrationTestBase(includeCommerce: false)
{
    protected override void ConfigureWebHostBuilder(IWebHostBuilder webHostBuilder)
    {
        webHostBuilder.UseStartup<Startup>();

        webHostBuilder.ConfigureServices(services =>
        {
            // Add data importer service to setup default content for the tests
            services.AddTransient<OptimizelyDataImporter>();
        });
    }
    
    [Fact]
    public void Can_Create_And_Read_NewsPage()
    {
        // Arrange
        var repo = Services.GetRequiredService<IContentRepository>();
        
        // Import test data
        var basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        var episerverDataFile = Path.Combine(basePath, "DefaultSiteContent.episerverdata");
        var dataImporter = Services.GetRequiredService<OptimizelyDataImporter>();
        
        // Run data importer service to set up default content for the tests
        dataImporter.Import(episerverDataFile);
        
        // Find StartPage from root
        var startPage = repo.GetChildren<StartPage>(ContentReference.RootPage).First();
        
        // Setup site definition
        var siteDefinitionRepo = Services.GetRequiredService<ISiteDefinitionRepository>();
        siteDefinitionRepo.Save(new SiteDefinition()
        {
            Name = "TestSite",
            StartPage = startPage.ContentLink,
            SiteUrl = new Uri("http://localhost"),
        });

        // Find first site
        var allSites = siteDefinitionRepo.List();
        var site = allSites.First();
        
        // Create NewsPage
        var news = repo.GetDefault<NewsPage>(site.StartPage);
        news.Name = "Alien Invasion";
        news.Title = "Martians Landed in Stockholm";

        // Act (Save and Load NewsPage)
        var savedRef = repo.Save(news, SaveAction.Publish, AccessLevel.NoAccess);
        var loaded = repo.Get<NewsPage>(savedRef);

        // Assert
        Assert.NotNull(loaded);
        Assert.Equal("Martians Landed in Stockholm", loaded.Title);
    }
}