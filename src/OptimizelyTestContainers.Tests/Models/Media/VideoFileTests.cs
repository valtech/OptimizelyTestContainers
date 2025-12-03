using System.ComponentModel.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAnnotations;
using EPiServer.Framework.DataAnnotations;
using EPiServer.Web;
using Optimizely.TestContainers.Models.Media;

namespace OptimizelyTestContainers.Tests.Models.Media;

public class VideoFileTests
{
    [Fact]
    public void VideoFile_Should_Have_ContentType_Attribute()
    {
        // Arrange & Act
        var attribute = typeof(VideoFile).GetCustomAttributes(typeof(ContentTypeAttribute), false)
            .FirstOrDefault() as ContentTypeAttribute;

        // Assert
        Assert.NotNull(attribute);
        Assert.Equal("85468104-E06F-47E5-A317-FC9B83D3CBA6", attribute.GUID);
    }

    [Fact]
    public void VideoFile_Should_Have_MediaDescriptor_Attribute()
    {
        // Arrange & Act
        var attribute = typeof(VideoFile).GetCustomAttributes(typeof(MediaDescriptorAttribute), false)
            .FirstOrDefault() as MediaDescriptorAttribute;

        // Assert
        Assert.NotNull(attribute);
        Assert.Equal("flv,mp4,webm", attribute.ExtensionString);
    }

    [Fact]
    public void VideoFile_Should_Inherit_From_VideoData()
    {
        // Arrange & Act
        var isVideoData = typeof(VideoData).IsAssignableFrom(typeof(VideoFile));

        // Assert
        Assert.True(isVideoData);
    }

    [Fact]
    public void Copyright_Property_Should_Be_Virtual()
    {
        // Arrange
        var property = typeof(VideoFile).GetProperty(nameof(VideoFile.Copyright));

        // Act & Assert
        Assert.NotNull(property);
        Assert.True(property.GetMethod?.IsVirtual);
    }

    [Fact]
    public void PreviewImage_Property_Should_Be_Virtual()
    {
        // Arrange
        var property = typeof(VideoFile).GetProperty(nameof(VideoFile.PreviewImage));

        // Act & Assert
        Assert.NotNull(property);
        Assert.True(property.GetMethod?.IsVirtual);
    }

    [Fact]
    public void PreviewImage_Property_Should_Have_UIHint_Attribute()
    {
        // Arrange
        var property = typeof(VideoFile).GetProperty(nameof(VideoFile.PreviewImage));

        // Act
        var attribute = property?.GetCustomAttributes(typeof(UIHintAttribute), false)
            .FirstOrDefault() as UIHintAttribute;

        // Assert
        Assert.NotNull(attribute);
        Assert.Equal(UIHint.Image, attribute.UIHint);
    }

    [Fact]
    public void VideoFile_Can_Be_Instantiated()
    {
        // Act
        var videoFile = new VideoFile();

        // Assert
        Assert.NotNull(videoFile);
    }

    [Fact]
    public void Copyright_Property_Can_Be_Set_And_Retrieved()
    {
        // Arrange
        var videoFile = new VideoFile();
        var expectedCopyright = "© 2024 Video Productions";

        // Act
        videoFile.Copyright = expectedCopyright;

        // Assert
        Assert.Equal(expectedCopyright, videoFile.Copyright);
    }

    [Fact]
    public void PreviewImage_Property_Can_Be_Set_And_Retrieved()
    {
        // Arrange
        var videoFile = new VideoFile();
        var expectedReference = new ContentReference(123);

        // Act
        videoFile.PreviewImage = expectedReference;

        // Assert
        Assert.Equal(expectedReference, videoFile.PreviewImage);
    }
}
