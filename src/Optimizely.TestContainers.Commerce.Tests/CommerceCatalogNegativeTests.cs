using System.ComponentModel.DataAnnotations;
using System.Globalization;
using EPiServer;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Core;
using EPiServer.DataAccess;
using EPiServer.Security;
using Mediachase.Commerce;
using Mediachase.Commerce.Catalog;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Optimizely.TestContainers.Commerce.Tests.Models.Commerce;
using Optimizely.TestContainers.Shared;

namespace Optimizely.TestContainers.Commerce.Tests;

/// <summary>
/// Negative/edge case integration tests for Commerce catalog functionality.
/// Tests error handling, validation, and edge cases for Commerce operations.
/// </summary>
[Collection("CommerceCatalogNegativeTests")]
public class CommerceCatalogNegativeTests() : OptimizelyIntegrationTestBase(includeCommerce: true)
{
    /// <summary>
    /// Configure web host with Commerce-specific Startup and services.
    /// The base class provides CMS, Commerce, and Find configuration automatically.
    /// </summary>
    protected override void ConfigureWebHostBuilder(IWebHostBuilder webHostBuilder)
    {
        // Register the Startup class that configures Commerce services and content types
        webHostBuilder.UseStartup<Startup>();
    }

    [Fact]
    public void Cannot_Load_NonExistent_Product()
    {
        // Arrange
        var contentRepository = Services.GetRequiredService<IContentRepository>();
        var nonExistentReference = new ContentReference(99999);

        // Act & Assert
        Assert.Throws<ContentNotFoundException>(() => contentRepository.Get<TestProduct>(nonExistentReference));
    }

    [Fact]
    public void TryGet_Returns_False_For_NonExistent_Product()
    {
        // Arrange
        var contentRepository = Services.GetRequiredService<IContentRepository>();
        var nonExistentReference = new ContentReference(99999);

        // Act
        var result = contentRepository.TryGet<TestProduct>(nonExistentReference, out var product);

        // Assert
        Assert.False(result);
        Assert.Null(product);
    }

    [Fact]
    public void Cannot_Save_Catalog_Without_Name()
    {
        // Arrange
        var referenceConverter = Services.GetRequiredService<ReferenceConverter>();
        var contentRepository = Services.GetRequiredService<IContentRepository>();

        var rootLink = referenceConverter.GetRootLink();

        var catalog = contentRepository.GetDefault<CatalogContent>(rootLink);
        catalog.Name = ""; // Empty name
        catalog.DefaultCurrency = Currency.USD;
        catalog.DefaultLanguage = "en";
        catalog.WeightBase = "kgs";
        catalog.LengthBase = "cm";

        // Act & Assert
        Assert.Throws<ValidationException>(() => contentRepository.Save(catalog, SaveAction.Publish, AccessLevel.NoAccess));
    }

    [Fact]
    public void Can_Delete_Product()
    {
        // Arrange
        var referenceConverter = Services.GetRequiredService<ReferenceConverter>();
        var contentRepository = Services.GetRequiredService<IContentRepository>();

        var rootLink = referenceConverter.GetRootLink();

        var catalog = contentRepository.GetDefault<CatalogContent>(rootLink);
        catalog.Name = "Delete Test Catalog";
        catalog.DefaultCurrency = Currency.USD;
        catalog.DefaultLanguage = "en";
        catalog.WeightBase = "kgs";
        catalog.LengthBase = "cm";
        
        var catalogReference = contentRepository.Save(catalog, SaveAction.Publish, AccessLevel.NoAccess);
        
        var node = contentRepository.GetDefault<NodeContent>(catalogReference, CultureInfo.GetCultureInfo("en"));
        node.Name = "Delete Test Node";
        var nodeReference = contentRepository.Save(node, SaveAction.Publish, AccessLevel.NoAccess);
        
        var product = contentRepository.GetDefault<TestProduct>(nodeReference, CultureInfo.GetCultureInfo("en"));
        product.Name = "To Be Deleted";
        product.Description = new XhtmlString("<p>Test</p>");
        var productReference = contentRepository.Save(product, SaveAction.Publish, AccessLevel.NoAccess);

        // Act (Delete)
        contentRepository.Delete(productReference, true, AccessLevel.NoAccess);

        // Assert
        var result = contentRepository.TryGet<TestProduct>(productReference, out var deleted);
        Assert.False(result);
    }

    [Fact]
    public void Can_Update_Existing_Product()
    {
        // Arrange
        var referenceConverter = Services.GetRequiredService<ReferenceConverter>();
        var contentRepository = Services.GetRequiredService<IContentRepository>();

        var rootLink = referenceConverter.GetRootLink();

        var catalog = contentRepository.GetDefault<CatalogContent>(rootLink);
        catalog.Name = "Update Test Catalog";
        catalog.DefaultCurrency = Currency.USD;
        catalog.DefaultLanguage = "en";
        catalog.WeightBase = "kgs";
        catalog.LengthBase = "cm";
        
        var catalogReference = contentRepository.Save(catalog, SaveAction.Publish, AccessLevel.NoAccess);
        
        var node = contentRepository.GetDefault<NodeContent>(catalogReference, CultureInfo.GetCultureInfo("en"));
        node.Name = "Update Test Node";
        var nodeReference = contentRepository.Save(node, SaveAction.Publish, AccessLevel.NoAccess);
        
        var product = contentRepository.GetDefault<TestProduct>(nodeReference, CultureInfo.GetCultureInfo("en"));
        product.Name = "Original Product Name";
        product.Description = new XhtmlString("<p>Original Description</p>");
        var productReference = contentRepository.Save(product, SaveAction.Publish, AccessLevel.NoAccess);

        // Act (Update)
        var writable = contentRepository.Get<TestProduct>(productReference).CreateWritableClone() as TestProduct;
        writable!.Description = new XhtmlString("<p>Updated Description</p>");
        contentRepository.Save(writable, SaveAction.Publish, AccessLevel.NoAccess);

        // Assert
        var loaded = contentRepository.Get<TestProduct>(productReference);
        Assert.Equal("<p>Updated Description</p>", loaded.Description?.ToHtmlString());
    }

    [Fact]
    public void Can_Create_Product_As_Draft()
    {
        // Arrange
        var referenceConverter = Services.GetRequiredService<ReferenceConverter>();
        var contentRepository = Services.GetRequiredService<IContentRepository>();

        var rootLink = referenceConverter.GetRootLink();

        var catalog = contentRepository.GetDefault<CatalogContent>(rootLink);
        catalog.Name = "Draft Test Catalog";
        catalog.DefaultCurrency = Currency.USD;
        catalog.DefaultLanguage = "en";
        catalog.WeightBase = "kgs";
        catalog.LengthBase = "cm";
        
        var catalogReference = contentRepository.Save(catalog, SaveAction.Publish, AccessLevel.NoAccess);
        
        var node = contentRepository.GetDefault<NodeContent>(catalogReference, CultureInfo.GetCultureInfo("en"));
        node.Name = "Draft Test Node";
        var nodeReference = contentRepository.Save(node, SaveAction.Publish, AccessLevel.NoAccess);
        
        var product = contentRepository.GetDefault<TestProduct>(nodeReference, CultureInfo.GetCultureInfo("en"));
        product.Name = "Draft Product";
        product.Description = new XhtmlString("<p>Draft Description</p>");

        // Act (Save as draft)
        var productReference = contentRepository.Save(product, SaveAction.CheckOut, AccessLevel.NoAccess);
        var loaded = contentRepository.Get<TestProduct>(productReference);

        // Assert
        Assert.NotNull(loaded);
        Assert.Equal("Draft Product", loaded.Name);
        Assert.False(loaded.Status == VersionStatus.Published);
    }

    [Fact]
    public void Cannot_Get_Wrong_Content_Type_From_Catalog()
    {
        // Arrange
        var referenceConverter = Services.GetRequiredService<ReferenceConverter>();
        var contentRepository = Services.GetRequiredService<IContentRepository>();

        var rootLink = referenceConverter.GetRootLink();

        var catalog = contentRepository.GetDefault<CatalogContent>(rootLink);
        catalog.Name = "Type Test Catalog";
        catalog.DefaultCurrency = Currency.USD;
        catalog.DefaultLanguage = "en";
        catalog.WeightBase = "kgs";
        catalog.LengthBase = "cm";
        
        var catalogReference = contentRepository.Save(catalog, SaveAction.Publish, AccessLevel.NoAccess);

        // Act & Assert - Try to get Catalog as Product
        Assert.Throws<TypeMismatchException>(() => contentRepository.Get<TestProduct>(catalogReference));
    }

    [Fact]
    public void Can_Create_Multiple_Products_In_Same_Node()
    {
        // Arrange
        var referenceConverter = Services.GetRequiredService<ReferenceConverter>();
        var contentRepository = Services.GetRequiredService<IContentRepository>();

        var rootLink = referenceConverter.GetRootLink();

        var catalog = contentRepository.GetDefault<CatalogContent>(rootLink);
        catalog.Name = "Multiple Products Catalog";
        catalog.DefaultCurrency = Currency.USD;
        catalog.DefaultLanguage = "en";
        catalog.WeightBase = "kgs";
        catalog.LengthBase = "cm";
        
        var catalogReference = contentRepository.Save(catalog, SaveAction.Publish, AccessLevel.NoAccess);
        
        var node = contentRepository.GetDefault<NodeContent>(catalogReference, CultureInfo.GetCultureInfo("en"));
        node.Name = "Multiple Products Node";
        var nodeReference = contentRepository.Save(node, SaveAction.Publish, AccessLevel.NoAccess);
        
        // Create first product
        var product1 = contentRepository.GetDefault<TestProduct>(nodeReference, CultureInfo.GetCultureInfo("en"));
        product1.Name = "Product 1";
        product1.Description = new XhtmlString("<p>Description 1</p>");
        var productRef1 = contentRepository.Save(product1, SaveAction.Publish, AccessLevel.NoAccess);

        // Create second product
        var product2 = contentRepository.GetDefault<TestProduct>(nodeReference, CultureInfo.GetCultureInfo("en"));
        product2.Name = "Product 2";
        product2.Description = new XhtmlString("<p>Description 2</p>");

        // Act
        var productRef2 = contentRepository.Save(product2, SaveAction.Publish, AccessLevel.NoAccess);

        // Assert
        var loaded1 = contentRepository.Get<TestProduct>(productRef1);
        var loaded2 = contentRepository.Get<TestProduct>(productRef2);
        
        Assert.NotNull(loaded1);
        Assert.NotNull(loaded2);
        Assert.Equal("Product 1", loaded1.Name);
        Assert.Equal("Product 2", loaded2.Name);
        Assert.NotEqual(loaded1.ContentLink, loaded2.ContentLink);
    }
}