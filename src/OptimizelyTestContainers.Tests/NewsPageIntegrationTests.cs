using EPiServer.Core;
using EPiServer.DataAnnotations;
using OptimizelyTestContainers.Tests.Models.Pages;

namespace OptimizelyTestContainers.Tests;

public class NewsPageIntegrationTests
{
    [Fact]
    public void NewsPage_Should_Have_ContentType_Attribute()
    {
        // Arrange & Act
        var attribute = typeof(NewsPage).GetCustomAttributes(typeof(ContentTypeAttribute), false)
            .FirstOrDefault() as ContentTypeAttribute;

        // Assert
        Assert.NotNull(attribute);
        Assert.Equal(Guid.Parse("7B873919-11AC-4DF4-B9E8-09F414F76164"), Guid.Parse(attribute.GUID));
        Assert.Equal("News Page", attribute.DisplayName);
    }

    [Fact]
    public void NewsPage_Should_Inherit_From_PageData()
    {
        // Arrange & Act
        var isPageData = typeof(PageData).IsAssignableFrom(typeof(NewsPage));

        // Assert
        Assert.True(isPageData);
    }

    [Fact]
    public void Title_Property_Should_Be_Virtual()
    {
        // Arrange
        var property = typeof(NewsPage).GetProperty(nameof(NewsPage.Title));

        // Act & Assert
        Assert.NotNull(property);
        Assert.True(property.GetMethod?.IsVirtual);
    }

    [Fact]
    public void NewsPage_Can_Be_Instantiated()
    {
        // Act
        var newsPage = new NewsPage();

        // Assert
        Assert.NotNull(newsPage);
    }

    [Fact]
    public void Title_Property_Can_Be_Set_And_Retrieved()
    {
        // Arrange
        var newsPage = new NewsPage();
        var expectedTitle = "Test News Title";

        // Act
        newsPage.Title = expectedTitle;

        // Assert
        Assert.Equal(expectedTitle, newsPage.Title);
    }
}