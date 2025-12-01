using EPiServer.Core;
using EPiServer.Core.Transfer;
using EPiServer.Enterprise;
using Microsoft.Extensions.Logging;
using Moq;

namespace OptimizelyTestContainers.Tests;

public class OptimizelyDataImporterTests
{
    private readonly Mock<ILogger<OptimizelyDataImporter>> _mockLogger;
    private readonly Mock<IDataImporter> _mockDataImporter;
    private readonly Mock<IContentEvents> _mockContentEvents;

    public OptimizelyDataImporterTests()
    {
        _mockLogger = new Mock<ILogger<OptimizelyDataImporter>>();
        _mockDataImporter = new Mock<IDataImporter>();
        _mockContentEvents = new Mock<IContentEvents>();
    }

    [Fact]
    public void Import_Should_Subscribe_To_PublishedContent_Event()
    {
        // Arrange
        var importer = new OptimizelyDataImporter(_mockLogger.Object, _mockDataImporter.Object, _mockContentEvents.Object);
        var importLog = new TransferLog();
        var tempFile = CreateTempImportFile();

        _mockDataImporter.Setup(x => x.Import(It.IsAny<Stream>(), It.IsAny<ContentReference>(), It.IsAny<ImportOptions>()))
            .Returns(importLog);

        // Act
        importer.Import(tempFile);

        // Assert
        _mockContentEvents.VerifyAdd(x => x.PublishedContent += It.IsAny<EventHandler<ContentEventArgs>>(), Times.Once);

        // Cleanup
        File.Delete(tempFile);
    }

    [Fact]
    public void Import_Should_Call_DataImporter_With_Correct_Options()
    {
        // Arrange
        var importer = new OptimizelyDataImporter(_mockLogger.Object, _mockDataImporter.Object, _mockContentEvents.Object);
        var importLog = new TransferLog();
        var tempFile = CreateTempImportFile();

        _mockDataImporter.Setup(x => x.Import(It.IsAny<Stream>(), It.IsAny<ContentReference>(), It.IsAny<ImportOptions>()))
            .Returns(importLog);

        // Act
        importer.Import(tempFile);

        // Assert
        _mockDataImporter.Verify(x => x.Import(
            It.IsAny<Stream>(),
            ContentReference.RootPage,
            It.Is<ImportOptions>(o =>
                o.KeepIdentity == true &&
                o.EnsureContentNameUniqueness == false &&
                o.ValidateDestination == true &&
                o.TransferType == TypeOfTransfer.Importing &&
                o.AutoCloseStream == true
            )), Times.Once);

        // Cleanup
        File.Delete(tempFile);
    }

    [Fact]
    public void Import_Should_Throw_Exception_When_Errors_Present()
    {
        // Arrange
        var importer = new OptimizelyDataImporter(_mockLogger.Object, _mockDataImporter.Object, _mockContentEvents.Object);
        var importLog = new TransferLog();
        importLog.AddError("Test error message");
        var tempFile = CreateTempImportFile();

        _mockDataImporter.Setup(x => x.Import(It.IsAny<Stream>(), It.IsAny<ContentReference>(), It.IsAny<ImportOptions>()))
            .Returns(importLog);

        // Act & Assert
        var exception = Assert.Throws<Exception>(() => importer.Import(tempFile));
        Assert.Equal("Test error message", exception.Message);

        // Cleanup
        File.Delete(tempFile);
    }

    [Fact]
    public void Import_Should_Log_Warnings_When_Present()
    {
        // Arrange
        var importer = new OptimizelyDataImporter(_mockLogger.Object, _mockDataImporter.Object, _mockContentEvents.Object);
        var importLog = new TransferLog();
        importLog.AddWarning("Test warning 1");
        importLog.AddWarning("Test warning 2");
        var tempFile = CreateTempImportFile();

        _mockDataImporter.Setup(x => x.Import(It.IsAny<Stream>(), It.IsAny<ContentReference>(), It.IsAny<ImportOptions>()))
            .Returns(importLog);

        // Act
        importer.Import(tempFile);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Test warning 1")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Test warning 2")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        // Cleanup
        File.Delete(tempFile);
    }

    [Fact]
    public void Import_Should_Not_Log_Warnings_When_None_Present()
    {
        // Arrange
        var importer = new OptimizelyDataImporter(_mockLogger.Object, _mockDataImporter.Object, _mockContentEvents.Object);
        var importLog = new TransferLog();
        var tempFile = CreateTempImportFile();

        _mockDataImporter.Setup(x => x.Import(It.IsAny<Stream>(), It.IsAny<ContentReference>(), It.IsAny<ImportOptions>()))
            .Returns(importLog);

        // Act
        importer.Import(tempFile);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);

        // Cleanup
        File.Delete(tempFile);
    }

    [Fact]
    public void Import_Should_Throw_FileNotFoundException_When_File_Does_Not_Exist()
    {
        // Arrange
        var importer = new OptimizelyDataImporter(_mockLogger.Object, _mockDataImporter.Object, _mockContentEvents.Object);
        var nonExistentFile = Path.Combine(Path.GetTempPath(), $"nonexistent_{Guid.NewGuid()}.episerverdata");

        // Act & Assert
        Assert.Throws<FileNotFoundException>(() => importer.Import(nonExistentFile));
    }

    [Fact]
    public void Import_Should_Complete_Successfully_With_No_Errors_Or_Warnings()
    {
        // Arrange
        var importer = new OptimizelyDataImporter(_mockLogger.Object, _mockDataImporter.Object, _mockContentEvents.Object);
        var importLog = new TransferLog();
        var tempFile = CreateTempImportFile();

        _mockDataImporter.Setup(x => x.Import(It.IsAny<Stream>(), It.IsAny<ContentReference>(), It.IsAny<ImportOptions>()))
            .Returns(importLog);

        // Act
        var exception = Record.Exception(() => importer.Import(tempFile));

        // Assert
        Assert.Null(exception);

        // Cleanup
        File.Delete(tempFile);
    }

    private string CreateTempImportFile()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.episerverdata");
        File.WriteAllText(tempFile, "test content");
        return tempFile;
    }
}
