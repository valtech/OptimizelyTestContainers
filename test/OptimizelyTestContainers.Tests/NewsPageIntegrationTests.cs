using EPiServer;
using EPiServer.Core;
using EPiServer.DataAccess;
using EPiServer.Security;
using Microsoft.Extensions.DependencyInjection;
using Optimizely.TestContainers;
using Optimizely.TestContainers.Models;
using Testcontainers.MsSql;

namespace OptimizelyTestContainers.Tests;

public class NewsPageIntegrationTests : IAsyncLifetime
{
    private readonly MsSqlContainer _dbContainer;
    private readonly IServiceProvider _serviceProvider;
    private readonly IContentRepository _contentRepository;
    private ContentReference _startPageRef;

    public NewsPageIntegrationTests()
    {
        _dbContainer = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .WithPassword("yourStrong(!)Password")
            .Build();
        
        // Simulated Optimizely CMS service setup
        var services = new ServiceCollection();
        
        // Register CMS core services for testing (simplified)
        services
            .AddCmsHost() // AddCms?
            .AddEmbeddedLocalization<Startup>();
        
        _serviceProvider = services.BuildServiceProvider();
        _contentRepository = _serviceProvider.GetRequiredService<IContentRepository>();
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
        
        // Add mock SiteDefinition etc. as needed...
        
        // Create a dummy start page
        var root = ContentReference.StartPage;
        var startPage = _contentRepository.GetDefault<PageData>(root);
        startPage.Name = "Start";
        _startPageRef = _contentRepository.Save(startPage, SaveAction.Publish, AccessLevel.NoAccess);
    }

    public async Task DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
    }

    [Fact]
    public void Can_Create_And_Read_NewsPage()
    {
        // Arrange
        var newsPage = _contentRepository.GetDefault<NewsPage>(_startPageRef);
        newsPage.Name = "Breaking News";
        newsPage.Title = "Aliens Invade Earth";

        // Act
        var savedRef = _contentRepository.Save(newsPage, SaveAction.Publish, AccessLevel.NoAccess);
        var loaded = _contentRepository.Get<NewsPage>(savedRef);

        // Assert
        Assert.NotNull(loaded);
        Assert.Equal("Aliens Invade Earth", loaded.Title);
    }
}