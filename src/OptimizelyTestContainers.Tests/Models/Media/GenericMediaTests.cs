using EPiServer.Core;
using EPiServer.DataAnnotations;
using OptimizelyTestContainers.Tests.Models.Media;

namespace OptimizelyTestContainers.Tests.Models.Media;

public class GenericMediaTests
{
    [Fact]
    public void GenericMedia_Should_Have_ContentType_Attribute()
    {
        // Arrange & Act
        var attribute = typeof(GenericMedia).GetCustomAttributes(typeof(ContentTypeAttribute), false)
            .FirstOrDefault() as ContentTypeAttribute;

        // Assert
        Assert.NotNull(attribute);
        Assert.Equal("EE3BD195-7CB0-4756-AB5F-E5E223CD9820", attribute.GUID);
    }

    [Fact]
    public void GenericMedia_Should_Inherit_From_MediaData()
    {
        // Arrange & Act
        var isMediaData = typeof(MediaData).IsAssignableFrom(typeof(GenericMedia));

        // Assert
        Assert.True(isMediaData);
    }

    [Fact]
    public void Description_Property_Should_Be_Virtual()
    {
        // Arrange
        var property = typeof(GenericMedia).GetProperty(nameof(GenericMedia.Description));

        // Act & Assert
        Assert.NotNull(property);
        Assert.True(property.GetMethod?.IsVirtual);
    }

    [Fact]
    public void GenericMedia_Can_Be_Instantiated()
    {
        // Act
        var genericMedia = new GenericMedia();

        // Assert
        Assert.NotNull(genericMedia);
    }

    [Fact]
    public void Description_Property_Can_Be_Set_And_Retrieved()
    {
        // Arrange
        var genericMedia = new GenericMedia();
        var expectedDescription = "This is a test media description";

        // Act
        genericMedia.Description = expectedDescription;

        // Assert
        Assert.Equal(expectedDescription, genericMedia.Description);
    }
}
