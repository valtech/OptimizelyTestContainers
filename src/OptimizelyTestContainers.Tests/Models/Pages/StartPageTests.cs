using System.ComponentModel.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using OptimizelyTestContainers.Tests.Models.Pages;

namespace OptimizelyTestContainers.Tests.Models.Pages;

public class StartPageTests
{
    [Fact]
    public void StartPage_Should_Have_ContentType_Attribute()
    {
        // Arrange & Act
        var attribute = typeof(StartPage).GetCustomAttributes(typeof(ContentTypeAttribute), false)
            .FirstOrDefault() as ContentTypeAttribute;

        // Assert
        Assert.NotNull(attribute);
        Assert.Equal("19671657-B684-4D95-A61F-8DD4FE60D559", attribute.GUID);
    }

    [Fact]
    public void StartPage_Should_Inherit_From_PageData()
    {
        // Arrange & Act
        var isPageData = typeof(PageData).IsAssignableFrom(typeof(StartPage));

        // Assert
        Assert.True(isPageData);
    }

    [Fact]
    public void MainContentArea_Property_Should_Have_Display_Attribute()
    {
        // Arrange
        var property = typeof(StartPage).GetProperty(nameof(StartPage.MainContentArea));

        // Act
        var displayAttribute = property?.GetCustomAttributes(typeof(DisplayAttribute), false)
            .FirstOrDefault() as DisplayAttribute;

        // Assert
        Assert.NotNull(displayAttribute);
        Assert.Equal(SystemTabNames.Content, displayAttribute.GroupName);
        Assert.Equal(320, displayAttribute.Order);
    }

    [Fact]
    public void MainContentArea_Property_Should_Have_CultureSpecific_Attribute()
    {
        // Arrange
        var property = typeof(StartPage).GetProperty(nameof(StartPage.MainContentArea));

        // Act
        var attribute = property?.GetCustomAttributes(typeof(CultureSpecificAttribute), false)
            .FirstOrDefault();

        // Assert
        Assert.NotNull(attribute);
    }

    [Fact]
    public void MainContentArea_Property_Should_Be_Virtual()
    {
        // Arrange
        var property = typeof(StartPage).GetProperty(nameof(StartPage.MainContentArea));

        // Act & Assert
        Assert.NotNull(property);
        Assert.True(property.GetMethod?.IsVirtual);
    }

    [Fact]
    public void StartPage_Can_Be_Instantiated()
    {
        // Act
        var startPage = new StartPage();

        // Assert
        Assert.NotNull(startPage);
    }
}
