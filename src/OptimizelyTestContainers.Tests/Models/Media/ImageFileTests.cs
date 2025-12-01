using EPiServer.Core;
using EPiServer.DataAnnotations;
using EPiServer.Framework.DataAnnotations;
using OptimizelyTestContainers.Tests.Models.Media;

namespace OptimizelyTestContainers.Tests.Models.Media;

public class ImageFileTests
{
    [Fact]
    public void ImageFile_Should_Have_ContentType_Attribute()
    {
        // Arrange & Act
        var attribute = typeof(ImageFile).GetCustomAttributes(typeof(ContentTypeAttribute), false)
            .FirstOrDefault() as ContentTypeAttribute;

        // Assert
        Assert.NotNull(attribute);
        Assert.Equal("0A89E464-56D4-449F-AEA8-2BF774AB8730", attribute.GUID);
    }

    [Fact]
    public void ImageFile_Should_Have_MediaDescriptor_Attribute()
    {
        // Arrange & Act
        var attribute = typeof(ImageFile).GetCustomAttributes(typeof(MediaDescriptorAttribute), false)
            .FirstOrDefault() as MediaDescriptorAttribute;

        // Assert
        Assert.NotNull(attribute);
        Assert.Equal("jpg,jpeg,jpe,ico,gif,bmp,png", attribute.ExtensionString);
    }

    [Fact]
    public void ImageFile_Should_Inherit_From_ImageData()
    {
        // Arrange & Act
        var isImageData = typeof(ImageData).IsAssignableFrom(typeof(ImageFile));

        // Assert
        Assert.True(isImageData);
    }

    [Fact]
    public void Copyright_Property_Should_Be_Virtual()
    {
        // Arrange
        var property = typeof(ImageFile).GetProperty(nameof(ImageFile.Copyright));

        // Act & Assert
        Assert.NotNull(property);
        Assert.True(property.GetMethod?.IsVirtual);
    }

    [Fact]
    public void ImageFile_Can_Be_Instantiated()
    {
        // Act
        var imageFile = new ImageFile();

        // Assert
        Assert.NotNull(imageFile);
    }

    [Fact]
    public void Copyright_Property_Can_Be_Set_And_Retrieved()
    {
        // Arrange
        var imageFile = new ImageFile();
        var expectedCopyright = "© 2024 Test Company";

        // Act
        imageFile.Copyright = expectedCopyright;

        // Assert
        Assert.Equal(expectedCopyright, imageFile.Copyright);
    }
}
