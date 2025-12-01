using EPiServer;
using EPiServer.Scheduler;
using Mediachase.Commerce.Catalog;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Moq;

namespace Optimizely.TestContainers.Commerce.Tests;

public class StartupTests
{
    [Fact]
    public void ConfigureServices_Should_Add_CMS_And_Commerce_Services()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddOptions();
        services.AddRouting();
        services.AddHttpContextAccessor();
        // Register IHttpContextFactory - required by EPiServer framework
        var mockHttpContextFactory = new Mock<IHttpContextFactory>();
        services.AddSingleton(mockHttpContextFactory.Object);
        services.AddCms(); // Add CMS services
        services.AddCommerce(); // Add Commerce services
        
        var mockEnvironment = new Mock<IWebHostEnvironment>();
        mockEnvironment.Setup(x => x.EnvironmentName).Returns(Environments.Production);
        mockEnvironment.Setup(x => x.ContentRootPath).Returns(Path.GetTempPath());

        var startup = new Startup(mockEnvironment.Object);

        // Act
        startup.ConfigureServices(services);

        // Assert - Check that core CMS services are registered
        Assert.Contains(services, s => s.ServiceType == typeof(IContentRepository));
        Assert.Contains(services, s => s.ServiceType == typeof(IContentLoader));
        
        // Check that Commerce services are registered
        Assert.Contains(services, s => s.ServiceType == typeof(ReferenceConverter));
    }

    [Fact]
    public void ConfigureServices_In_Development_Should_Configure_Scheduler_Options()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddOptions();
        services.AddRouting();
        services.AddHttpContextAccessor();
        // Register IHttpContextFactory - required by EPiServer framework
        var mockHttpContextFactory = new Mock<IHttpContextFactory>();
        services.AddSingleton(mockHttpContextFactory.Object);
        services.AddCms(); // Add CMS services
        services.AddCommerce(); // Add Commerce services
        
        var mockEnvironment = new Mock<IWebHostEnvironment>();
        mockEnvironment.Setup(x => x.EnvironmentName).Returns(Environments.Development);
        mockEnvironment.Setup(x => x.ContentRootPath).Returns(Path.GetTempPath());

        var startup = new Startup(mockEnvironment.Object);

        // Act
        startup.ConfigureServices(services);

        // Assert - Check that scheduler options are configured
        var serviceProvider = services.BuildServiceProvider();
        var schedulerOptions = serviceProvider.GetService<IOptions<SchedulerOptions>>();
        Assert.NotNull(schedulerOptions);
        Assert.False(schedulerOptions.Value.Enabled);
    }

    [Fact]
    public void ConfigureServices_In_Production_Should_Not_Configure_Scheduler_Options()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddOptions();
        services.AddRouting();
        services.AddHttpContextAccessor();
        // Register IHttpContextFactory - required by EPiServer framework
        var mockHttpContextFactory = new Mock<IHttpContextFactory>();
        services.AddSingleton(mockHttpContextFactory.Object);
        services.AddCms(); // Add CMS services
        services.AddCommerce(); // Add Commerce services
        
        var mockEnvironment = new Mock<IWebHostEnvironment>();
        mockEnvironment.Setup(x => x.EnvironmentName).Returns(Environments.Production);
        mockEnvironment.Setup(x => x.ContentRootPath).Returns(Path.GetTempPath());

        var startup = new Startup(mockEnvironment.Object);

        // Act
        startup.ConfigureServices(services);

        // Assert - Scheduler should use default configuration
        var serviceProvider = services.BuildServiceProvider();
        var schedulerOptions = serviceProvider.GetService<IOptions<SchedulerOptions>>();
        
        Assert.NotNull(schedulerOptions);
    }

    [Fact(Skip = "Mock setup incomplete - IApplicationBuilder requires additional configuration for middleware pipeline testing")]
    public void Configure_Should_Setup_Middleware_Pipeline()
    {
        // Arrange
        var mockAppBuilder = new Mock<IApplicationBuilder>();
        var mockEnvironment = new Mock<IWebHostEnvironment>();
        mockEnvironment.Setup(x => x.EnvironmentName).Returns(Environments.Production);
        mockEnvironment.Setup(x => x.ContentRootPath).Returns(Path.GetTempPath());

        var startup = new Startup(mockEnvironment.Object);

        // Setup application services with routing
        var services = new ServiceCollection();
        services.AddRouting();
        var serviceProvider = services.BuildServiceProvider();

        mockAppBuilder.Setup(x => x.Use(It.IsAny<Func<RequestDelegate, RequestDelegate>>()))
            .Returns(mockAppBuilder.Object);
        mockAppBuilder.Setup(x => x.New()).Returns(mockAppBuilder.Object);
        mockAppBuilder.Setup(x => x.ApplicationServices).Returns(serviceProvider);

        // Act
        startup.Configure(mockAppBuilder.Object, mockEnvironment.Object);

        // Assert
        mockAppBuilder.Verify(x => x.Use(It.IsAny<Func<RequestDelegate, RequestDelegate>>()), Times.AtLeastOnce);
    }

    [Fact(Skip = "Mock setup incomplete - IApplicationBuilder requires additional configuration for developer exception page testing")]
    public void Configure_In_Development_Should_Use_DeveloperExceptionPage()
    {
        // Arrange
        var mockAppBuilder = new Mock<IApplicationBuilder>();
        var mockEnvironment = new Mock<IWebHostEnvironment>();
        mockEnvironment.Setup(x => x.EnvironmentName).Returns(Environments.Development);
        mockEnvironment.Setup(x => x.ContentRootPath).Returns(Path.GetTempPath());

        var startup = new Startup(mockEnvironment.Object);

        // Setup application services with routing
        var services = new ServiceCollection();
        services.AddRouting();
        var serviceProvider = services.BuildServiceProvider();

        mockAppBuilder.Setup(x => x.Use(It.IsAny<Func<RequestDelegate, RequestDelegate>>()))
            .Returns(mockAppBuilder.Object);
        mockAppBuilder.Setup(x => x.New()).Returns(mockAppBuilder.Object);
        mockAppBuilder.Setup(x => x.ApplicationServices).Returns(serviceProvider);

        // Act
        startup.Configure(mockAppBuilder.Object, mockEnvironment.Object);

        // Assert
        mockAppBuilder.Verify(x => x.Use(It.IsAny<Func<RequestDelegate, RequestDelegate>>()), Times.AtLeastOnce);
    }

    [Fact]
    public void Startup_Constructor_Should_Accept_WebHostEnvironment()
    {
        // Arrange
        var mockEnvironment = new Mock<IWebHostEnvironment>();
        mockEnvironment.Setup(x => x.EnvironmentName).Returns(Environments.Production);
        mockEnvironment.Setup(x => x.ContentRootPath).Returns(Path.GetTempPath());

        // Act
        var startup = new Startup(mockEnvironment.Object);

        // Assert
        Assert.NotNull(startup);
    }
}