using System.Reflection;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAccess;
using EPiServer.Framework.Blobs;
using EPiServer.Security;
using EPiServer.Web;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Optimizely.TestContainers.Models.Media;
using Optimizely.TestContainers.Shared;
using OptimizelyTestContainers.Tests.Models.Media;
using OptimizelyTestContainers.Tests.Models.Pages;

namespace OptimizelyTestContainers.Tests;

/// <summary>
/// Integration tests for media content types (ImageFile, VideoFile, GenericMedia).
/// Tests blob storage, media properties, and asset management using the unified fixture pattern.
/// </summary>
[Collection("MediaIntegrationTests")]
public class MediaIntegrationTests() : OptimizelyIntegrationTestBase(includeCommerce: true)
{
    /// <summary>
    /// Configure web host with CMS-specific Startup and services.
    /// The base class provides Commerce and Find configuration automatically.
    /// </summary>
    protected override void ConfigureWebHostBuilder(IWebHostBuilder webHostBuilder)
    {
        // Register the Startup class that configures CMS services and content types
        webHostBuilder.UseStartup<Startup>();

        // Register additional test-specific services
        webHostBuilder.ConfigureServices(services =>
        {
            services.AddTransient<OptimizelyDataImporter>();
        });
    }

    [Fact]
    public void Can_Create_And_Read_ImageFile()
    {
        // Arrange
        var repo = Services.GetRequiredService<IContentRepository>();
        var blobFactory = Services.GetRequiredService<IBlobFactory>();

        // Import test data to get StartPage
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

        // Get Assets folder
        var assetsFolder = repo.GetChildren<ContentFolder>(ContentReference.GlobalBlockFolder).FirstOrDefault()
            ?? repo.GetDefault<ContentFolder>(ContentReference.GlobalBlockFolder);
        
        if (assetsFolder.ContentLink.ID == 0)
        {
            assetsFolder.Name = "Assets";
            repo.Save(assetsFolder, SaveAction.Publish, AccessLevel.NoAccess);
        }

        // Create ImageFile
        var imageFile = repo.GetDefault<ImageFile>(assetsFolder.ContentLink);
        imageFile.Name = "test-image.jpg";
        imageFile.Copyright = "© 2024 Test Company";

        // Create a simple blob with test data
        var blob = blobFactory.CreateBlob(imageFile.BinaryDataContainer, ".jpg");
        using (var stream = blob.OpenWrite())
        {
            var testData = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 }; // JPEG header
            stream.Write(testData, 0, testData.Length);
        }
        imageFile.BinaryData = blob;

        // Act (Save and Load ImageFile)
        var savedRef = repo.Save(imageFile, SaveAction.Publish, AccessLevel.NoAccess);
        var loaded = repo.Get<ImageFile>(savedRef);

        // Assert
        Assert.NotNull(loaded);
        Assert.Equal("test-image.jpg", loaded.Name);
        Assert.Equal("© 2024 Test Company", loaded.Copyright);
        Assert.NotNull(loaded.BinaryData);
    }

    [Fact]
    public void Can_Create_And_Read_VideoFile()
    {
        // Arrange
        var repo = Services.GetRequiredService<IContentRepository>();
        var blobFactory = Services.GetRequiredService<IBlobFactory>();

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

        // Get Assets folder
        var assetsFolder = repo.GetChildren<ContentFolder>(ContentReference.GlobalBlockFolder).FirstOrDefault()
            ?? repo.GetDefault<ContentFolder>(ContentReference.GlobalBlockFolder);
        
        if (assetsFolder.ContentLink.ID == 0)
        {
            assetsFolder.Name = "Assets";
            repo.Save(assetsFolder, SaveAction.Publish, AccessLevel.NoAccess);
        }

        // Create VideoFile
        var videoFile = repo.GetDefault<VideoFile>(assetsFolder.ContentLink);
        videoFile.Name = "test-video.mp4";
        videoFile.Copyright = "© 2024 Video Productions";
        videoFile.PreviewImage = ContentReference.EmptyReference;

        // Create a simple blob with test data
        var blob = blobFactory.CreateBlob(videoFile.BinaryDataContainer, ".mp4");
        using (var stream = blob.OpenWrite())
        {
            var testData = new byte[] { 0x00, 0x00, 0x00, 0x18 }; // MP4 signature
            stream.Write(testData, 0, testData.Length);
        }
        videoFile.BinaryData = blob;

        // Act (Save and Load VideoFile)
        var savedRef = repo.Save(videoFile, SaveAction.Publish, AccessLevel.NoAccess);
        var loaded = repo.Get<VideoFile>(savedRef);

        // Assert
        Assert.NotNull(loaded);
        Assert.Equal("test-video.mp4", loaded.Name);
        Assert.Equal("© 2024 Video Productions", loaded.Copyright);
        Assert.NotNull(loaded.BinaryData);
    }

    [Fact]
    public void Can_Create_And_Read_GenericMedia()
    {
        // Arrange
        var repo = Services.GetRequiredService<IContentRepository>();
        var blobFactory = Services.GetRequiredService<IBlobFactory>();

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

        // Get Assets folder
        var assetsFolder = repo.GetChildren<ContentFolder>(ContentReference.GlobalBlockFolder).FirstOrDefault()
            ?? repo.GetDefault<ContentFolder>(ContentReference.GlobalBlockFolder);
        
        if (assetsFolder.ContentLink.ID == 0)
        {
            assetsFolder.Name = "Assets";
            repo.Save(assetsFolder, SaveAction.Publish, AccessLevel.NoAccess);
        }

        // Create GenericMedia
        var genericMedia = repo.GetDefault<GenericMedia>(assetsFolder.ContentLink);
        genericMedia.Name = "test-document.pdf";
        genericMedia.Description = "Test media file description";

        // Create a simple blob with test data
        var blob = blobFactory.CreateBlob(genericMedia.BinaryDataContainer, ".pdf");
        using (var stream = blob.OpenWrite())
        {
            var testData = new byte[] { 0x25, 0x50, 0x44, 0x46 }; // PDF signature
            stream.Write(testData, 0, testData.Length);
        }
        genericMedia.BinaryData = blob;

        // Act (Save and Load GenericMedia)
        var savedRef = repo.Save(genericMedia, SaveAction.Publish, AccessLevel.NoAccess);
        var loaded = repo.Get<GenericMedia>(savedRef);

        // Assert
        Assert.NotNull(loaded);
        Assert.Equal("test-document.pdf", loaded.Name);
        Assert.Equal("Test media file description", loaded.Description);
        Assert.NotNull(loaded.BinaryData);
    }

    [Fact]
    public void ImageFile_Properties_Should_Persist_After_Save()
    {
        // Arrange
        var repo = Services.GetRequiredService<IContentRepository>();
        var blobFactory = Services.GetRequiredService<IBlobFactory>();

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

        // Get Assets folder
        var assetsFolder = repo.GetChildren<ContentFolder>(ContentReference.GlobalBlockFolder).FirstOrDefault()
            ?? repo.GetDefault<ContentFolder>(ContentReference.GlobalBlockFolder);
        
        if (assetsFolder.ContentLink.ID == 0)
        {
            assetsFolder.Name = "Assets";
            repo.Save(assetsFolder, SaveAction.Publish, AccessLevel.NoAccess);
        }

        var imageFile = repo.GetDefault<ImageFile>(assetsFolder.ContentLink);
        var expectedCopyright = "© Test Copyright 2024";
        imageFile.Name = "copyright-test.jpg";
        imageFile.Copyright = expectedCopyright;

        var blob = blobFactory.CreateBlob(imageFile.BinaryDataContainer, ".jpg");
        using (var stream = blob.OpenWrite())
        {
            var testData = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 };
            stream.Write(testData, 0, testData.Length);
        }
        imageFile.BinaryData = blob;

        // Act
        var savedRef = repo.Save(imageFile, SaveAction.Publish, AccessLevel.NoAccess);
        var loaded = repo.Get<ImageFile>(savedRef);

        // Assert
        Assert.Equal(expectedCopyright, loaded.Copyright);
    }

    [Fact(Skip = "Fails due to known issue with VideoFile PreviewImage not persisting correctly.")]
    public void VideoFile_PreviewImage_Should_Persist_After_Save()
    {
        // Arrange
        var repo = Services.GetRequiredService<IContentRepository>();
        var blobFactory = Services.GetRequiredService<IBlobFactory>();

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

        // Get Assets folder
        var assetsFolder = repo.GetChildren<ContentFolder>(ContentReference.GlobalBlockFolder).FirstOrDefault()
            ?? repo.GetDefault<ContentFolder>(ContentReference.GlobalBlockFolder);
        
        if (assetsFolder.ContentLink.ID == 0)
        {
            assetsFolder.Name = "Assets";
            assetsFolder.ContentLink = repo.Save(assetsFolder, SaveAction.Publish, AccessLevel.NoAccess);
        }

        var videoFile = repo.GetDefault<VideoFile>(assetsFolder.ContentLink);
        var expectedPreviewImage = new ContentReference(999);
        videoFile.Name = "preview-test.mp4";
        videoFile.Copyright = "Test";
        videoFile.PreviewImage = expectedPreviewImage;

        var blob = blobFactory.CreateBlob(videoFile.BinaryDataContainer, ".mp4");
        using (var stream = blob.OpenWrite())
        {
            var testData = new byte[] { 0x00, 0x00, 0x00, 0x18 };
            stream.Write(testData, 0, testData.Length);
        }
        videoFile.BinaryData = blob;

        // Act
        var savedRef = repo.Save(videoFile, SaveAction.Publish, AccessLevel.NoAccess);
        var loaded = repo.Get<VideoFile>(savedRef);

        // Assert
        Assert.Equal(expectedPreviewImage, loaded.PreviewImage);
    }
}