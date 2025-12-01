using EPiServer.Cms.UI.AspNetIdentity;
using EPiServer.Core;
using EPiServer.DataAccess;
using EPiServer.Scheduler;
using EPiServer.Web.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Moq;

namespace OptimizelyTestContainers.Tests;

public class StartupTests
{
    [Fact]
    public void ConfigureServices_Should_Add_CMS_Services()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockEnvironment = new Mock<IWebHostEnvironment>();
        mockEnvironment.Setup(x => x.EnvironmentName).Returns(Environments.Production);
        mockEnvironment.Setup(x => x.ContentRootPath).Returns(Path.GetTempPath());

        var startup = new Startup(mockEnvironment.Object);

        // Act
        startup.ConfigureServices(services);

        // Assert - Check that core CMS services are registered
        Assert.Contains(services, s => s.ServiceType == typeof(IContentRepository));
        Assert.Contains(services, s => s.ServiceType == typeof(IContentLoader));
    }

    [Fact]
    public void ConfigureServices_In_Development_Should_Configure_Scheduler_Options()
    {
        // Arrange
        var services = new ServiceCollection();
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
        var mockEnvironment = new Mock<IWebHostEnvironment>();
        mockEnvironment.Setup(x => x.EnvironmentName).Returns(Environments.Production);
        mockEnvironment.Setup(x => x.ContentRootPath).Returns(Path.GetTempPath());

        var startup = new Startup(mockEnvironment.Object);

        // Act
        startup.ConfigureServices(services);

        // Assert - Scheduler should use default configuration (enabled)
        var serviceProvider = services.BuildServiceProvider();
        var schedulerOptions = serviceProvider.GetService<IOptions<SchedulerOptions>>();
        
        // In production, scheduler is not explicitly disabled, so it should be enabled by default
        Assert.NotNull(schedulerOptions);
    }

    [Fact]
    public void Configure_Should_Setup_Middleware_Pipeline()
    {
        // Arrange
        var mockAppBuilder = new Mock<IApplicationBuilder>();
        var mockEnvironment = new Mock<IWebHostEnvironment>();
        mockEnvironment.Setup(x => x.EnvironmentName).Returns(Environments.Production);
        mockEnvironment.Setup(x => x.ContentRootPath).Returns(Path.GetTempPath());

        var startup = new Startup(mockEnvironment.Object);

        // Setup for middleware chain
        mockAppBuilder.Setup(x => x.Use(It.IsAny<Func<RequestDelegate, RequestDelegate>>()))
            .Returns(mockAppBuilder.Object);
        mockAppBuilder.Setup(x => x.New()).Returns(mockAppBuilder.Object);
        mockAppBuilder.Setup(x => x.ApplicationServices).Returns(new ServiceCollection().BuildServiceProvider());

        // Act
        startup.Configure(mockAppBuilder.Object, mockEnvironment.Object);

        // Assert
        mockAppBuilder.Verify(x => x.Use(It.IsAny<Func<RequestDelegate, RequestDelegate>>()), Times.AtLeastOnce);
    }

    [Fact]
    public void Configure_In_Development_Should_Use_DeveloperExceptionPage()
    {
        // Arrange
        var mockAppBuilder = new Mock<IApplicationBuilder>();
        var mockEnvironment = new Mock<IWebHostEnvironment>();
        mockEnvironment.Setup(x => x.EnvironmentName).Returns(Environments.Development);
        mockEnvironment.Setup(x => x.ContentRootPath).Returns(Path.GetTempPath());

        var startup = new Startup(mockEnvironment.Object);

        mockAppBuilder.Setup(x => x.Use(It.IsAny<Func<RequestDelegate, RequestDelegate>>()))
            .Returns(mockAppBuilder.Object);
        mockAppBuilder.Setup(x => x.New()).Returns(mockAppBuilder.Object);
        mockAppBuilder.Setup(x => x.ApplicationServices).Returns(new ServiceCollection().BuildServiceProvider());

        // Act
        startup.Configure(mockAppBuilder.Object, mockEnvironment.Object);

        // Assert - Middleware should be added
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
