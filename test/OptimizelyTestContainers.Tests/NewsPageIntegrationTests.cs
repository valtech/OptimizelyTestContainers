using System.Reflection;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAccess;
using EPiServer.Security;
using EPiServer.Web;
using Microsoft.Extensions.DependencyInjection;
using Optimizely.TestContainers.Models.Pages;

namespace OptimizelyTestContainers.Tests;

public class NewsPageIntegrationTest : OptimizelyIntegrationTestBase
{
    [Fact]
    public void Can_Create_And_Read_NewsPage()
    {
        // Arrange
        var repo = Services.GetRequiredService<IContentRepository>();
        
        // Import test data
        var basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        var episerverDataFile = Path.Combine(basePath, "DefaultSiteContent.episerverdata");
        
        var dataImporter = Services.GetRequiredService<OptimizelyDataImporter>();
        dataImporter.Import(episerverDataFile);
        
        
        // Find StartPage from root
        // Setup site definition
        
        var startPage = repo.GetChildren<StartPage>(ContentReference.RootPage).First();
        
        var siteDefinitionRepo = Services.GetRequiredService<ISiteDefinitionRepository>();
        
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