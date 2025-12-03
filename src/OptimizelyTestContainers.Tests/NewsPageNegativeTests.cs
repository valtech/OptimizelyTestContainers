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

public class NewsPageNegativeTests() : OptimizelyIntegrationTestBase(includeCommerce: false)
{
    protected override void ConfigureWebHostBuilder(IWebHostBuilder webHostBuilder)
    {
        webHostBuilder.UseStartup<Startup>();

        webHostBuilder.ConfigureServices(services =>
        {
            services.AddTransient<OptimizelyDataImporter>();
        });
    }

    [Fact]
    public void Cannot_Load_NonExistent_NewsPage()
    {
        // Arrange
        var repo = Services.GetRequiredService<IContentRepository>();
        var nonExistentReference = new ContentReference(99999);

        // Act & Assert
        Assert.Throws<ContentNotFoundException>(() => repo.Get<NewsPage>(nonExistentReference));
    }

    [Fact]
    public void TryGet_Returns_False_For_NonExistent_Content()
    {
        // Arrange
        var repo = Services.GetRequiredService<IContentRepository>();
        var nonExistentReference = new ContentReference(99999);

        // Act
        var result = repo.TryGet<NewsPage>(nonExistentReference, out var content);

        // Assert
        Assert.False(result);
        Assert.Null(content);
    }

    [Fact]
    public void Cannot_Save_NewsPage_Without_Name()
    {
        // Arrange
        var repo = Services.GetRequiredService<IContentRepository>();

        // Import test data
        var basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        var episerverDataFile = Path.Combine(basePath, "DefaultSiteContent.episerverdata");
        var dataImporter = Services.GetRequiredService<OptimizelyDataImporter>();
        dataImporter.Import(episerverDataFile);

        var startPage = repo.GetChildren<StartPage>(ContentReference.RootPage).First();

        // Setup site definition
        var siteDefinitionRepo = Services.GetRequiredService<ISiteDefinitionRepository>();
        siteDefinitionRepo.Save(new SiteDefinition()
        {
            Name = "TestSite",
            StartPage = startPage.ContentLink,
            SiteUrl = new Uri("http://localhost"),
        });

        var allSites = siteDefinitionRepo.List();
        var site = allSites.First();

        // Create NewsPage without name
        var news = repo.GetDefault<NewsPage>(site.StartPage);
        news.Name = ""; // Empty name
        news.Title = "Test Title";

        // Act & Assert
        Assert.Throws<EPiServerException>(() => repo.Save(news, SaveAction.Publish, AccessLevel.NoAccess));
    }

    [Fact]
    public void Can_Save_NewsPage_As_Draft()
    {
        // Arrange
        var repo = Services.GetRequiredService<IContentRepository>();

        // Import test data
        var basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        var episerverDataFile = Path.Combine(basePath, "DefaultSiteContent.episerverdata");
        var dataImporter = Services.GetRequiredService<OptimizelyDataImporter>();
        dataImporter.Import(episerverDataFile);

        var startPage = repo.GetChildren<StartPage>(ContentReference.RootPage).First();

        // Setup site definition
        var siteDefinitionRepo = Services.GetRequiredService<ISiteDefinitionRepository>();
        siteDefinitionRepo.Save(new SiteDefinition()
        {
            Name = "TestSite",
            StartPage = startPage.ContentLink,
            SiteUrl = new Uri("http://localhost"),
        });

        var allSites = siteDefinitionRepo.List();
        var site = allSites.First();

        // Create NewsPage
        var news = repo.GetDefault<NewsPage>(site.StartPage);
        news.Name = "Draft News";
        news.Title = "Draft Title";

        // Act (Save as draft)
        var savedRef = repo.Save(news, SaveAction.CheckOut, AccessLevel.NoAccess);
        var loaded = repo.Get<NewsPage>(savedRef);

        // Assert
        Assert.NotNull(loaded);
        Assert.Equal("Draft Title", loaded.Title);
        Assert.False(loaded.Status == VersionStatus.Published);
    }

    [Fact]
    public void Can_Delete_NewsPage()
    {
        // Arrange
        var repo = Services.GetRequiredService<IContentRepository>();

        // Import test data
        var basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        var episerverDataFile = Path.Combine(basePath, "DefaultSiteContent.episerverdata");
        var dataImporter = Services.GetRequiredService<OptimizelyDataImporter>();
        dataImporter.Import(episerverDataFile);

        var startPage = repo.GetChildren<StartPage>(ContentReference.RootPage).First();

        // Setup site definition
        var siteDefinitionRepo = Services.GetRequiredService<ISiteDefinitionRepository>();
        siteDefinitionRepo.Save(new SiteDefinition()
        {
            Name = "TestSite",
            StartPage = startPage.ContentLink,
            SiteUrl = new Uri("http://localhost"),
        });

        var allSites = siteDefinitionRepo.List();
        var site = allSites.First();

        // Create and save NewsPage
        var news = repo.GetDefault<NewsPage>(site.StartPage);
        news.Name = "To Be Deleted";
        news.Title = "Delete Test";
        var savedRef = repo.Save(news, SaveAction.Publish, AccessLevel.NoAccess);

        // Act (Delete)
        repo.Delete(savedRef, true, AccessLevel.NoAccess);

        // Assert
        var result = repo.TryGet<NewsPage>(savedRef, out var deleted);
        Assert.False(result);
    }

    [Fact]
    public void Can_Create_Multiple_NewsPages_With_Same_Title()
    {
        // Arrange
        var repo = Services.GetRequiredService<IContentRepository>();

        // Import test data
        var basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        var episerverDataFile = Path.Combine(basePath, "DefaultSiteContent.episerverdata");
        var dataImporter = Services.GetRequiredService<OptimizelyDataImporter>();
        dataImporter.Import(episerverDataFile);

        var startPage = repo.GetChildren<StartPage>(ContentReference.RootPage).First();

        // Setup site definition
        var siteDefinitionRepo = Services.GetRequiredService<ISiteDefinitionRepository>();
        siteDefinitionRepo.Save(new SiteDefinition()
        {
            Name = "TestSite",
            StartPage = startPage.ContentLink,
            SiteUrl = new Uri("http://localhost"),
        });

        var allSites = siteDefinitionRepo.List();
        var site = allSites.First();

        // Create first NewsPage
        var news1 = repo.GetDefault<NewsPage>(site.StartPage);
        news1.Name = "Duplicate Test 1";
        news1.Title = "Same Title";
        var savedRef1 = repo.Save(news1, SaveAction.Publish, AccessLevel.NoAccess);

        // Create second NewsPage with same title
        var news2 = repo.GetDefault<NewsPage>(site.StartPage);
        news2.Name = "Duplicate Test 2";
        news2.Title = "Same Title";

        // Act
        var savedRef2 = repo.Save(news2, SaveAction.Publish, AccessLevel.NoAccess);

        // Assert - Both should be saved successfully
        var loaded1 = repo.Get<NewsPage>(savedRef1);
        var loaded2 = repo.Get<NewsPage>(savedRef2);
        
        Assert.NotNull(loaded1);
        Assert.NotNull(loaded2);
        Assert.Equal("Same Title", loaded1.Title);
        Assert.Equal("Same Title", loaded2.Title);
        Assert.NotEqual(loaded1.ContentLink, loaded2.ContentLink);
    }

    [Fact]
    public void Can_Update_Existing_NewsPage()
    {
        // Arrange
        var repo = Services.GetRequiredService<IContentRepository>();

        // Import test data
        var basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        var episerverDataFile = Path.Combine(basePath, "DefaultSiteContent.episerverdata");
        var dataImporter = Services.GetRequiredService<OptimizelyDataImporter>();
        dataImporter.Import(episerverDataFile);

        var startPage = repo.GetChildren<StartPage>(ContentReference.RootPage).First();

        // Setup site definition
        var siteDefinitionRepo = Services.GetRequiredService<ISiteDefinitionRepository>();
        siteDefinitionRepo.Save(new SiteDefinition()
        {
            Name = "TestSite",
            StartPage = startPage.ContentLink,
            SiteUrl = new Uri("http://localhost"),
        });

        var allSites = siteDefinitionRepo.List();
        var site = allSites.First();

        // Create NewsPage
        var news = repo.GetDefault<NewsPage>(site.StartPage);
        news.Name = "Original Name";
        news.Title = "Original Title";
        var savedRef = repo.Save(news, SaveAction.Publish, AccessLevel.NoAccess);

        // Act (Update)
        var writable = repo.Get<NewsPage>(savedRef).CreateWritableClone() as NewsPage;
        writable!.Title = "Updated Title";
        repo.Save(writable, SaveAction.Publish, AccessLevel.NoAccess);

        // Assert
        var loaded = repo.Get<NewsPage>(savedRef);
        Assert.Equal("Updated Title", loaded.Title);
    }

    [Fact]
    public void Cannot_Get_Wrong_Content_Type()
    {
        // Arrange
        var repo = Services.GetRequiredService<IContentRepository>();

        // Import test data
        var basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        var episerverDataFile = Path.Combine(basePath, "DefaultSiteContent.episerverdata");
        var dataImporter = Services.GetRequiredService<OptimizelyDataImporter>();
        /* TODO: 
           OptimizelyTestContainers.Tests.NewsPageNegativeTests.Cannot_Save_NewsPage_Without_Name (10s 443ms): Error Message:
       System.Exception : Failed to Deserialize object to Dynamic Data Store. BinaryFormatter serialization and deserial
      ization have been removed. See https://aka.ms/binaryformatter for more information.
      Stack Trace:
         at OptimizelyTestContainers.Tests.OptimizelyDataImporter.Import(String importFilePath) in D:\Git\Valtech\Optimi
      zelyTestContainers\src\OptimizelyTestContainers.Tests\OptimizelyDataImporter.cs:line 35
         
        */
        dataImporter.Import(episerverDataFile);

        var startPage = repo.GetChildren<StartPage>(ContentReference.RootPage).First();

        // Act & Assert - Try to get StartPage as NewsPage
        Assert.Throws<TypeMismatchException>(() => repo.Get<NewsPage>(startPage.ContentLink));
    }
}
