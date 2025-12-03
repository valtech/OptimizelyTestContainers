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
/// Integration tests for Commerce catalog functionality (catalogs, nodes, products).
/// Tests Commerce-specific content types and operations.
/// </summary>
[Collection("CommerceCatalogIntegrationTests")]
public class CommerceCatalogIntegrationTests() : OptimizelyIntegrationTestBase(includeCommerce: true)
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
    public void Can_Save_Catalog_And_Node_And_Product()
    {
        // Arrange
        var referenceConverter = Services.GetRequiredService<ReferenceConverter>();
        var contentRepository = Services.GetRequiredService<IContentRepository>();

        var rootLink = referenceConverter.GetRootLink();

        var aliensCatalog = contentRepository.GetDefault<CatalogContent>(rootLink);
        aliensCatalog.Name = "Aliens";
        aliensCatalog.DefaultCurrency = Currency.USD;
        aliensCatalog.DefaultLanguage = "en";
        aliensCatalog.WeightBase = "kgs"; // From WeightBaseSelectionFactory
        aliensCatalog.LengthBase = "cm"; // From LengthBaseSelectionFactory
        
        var alienCatalogReference = contentRepository.Save(aliensCatalog, SaveAction.Publish, AccessLevel.NoAccess);
        
        var aliensNode = contentRepository.GetDefault<NodeContent>(alienCatalogReference, CultureInfo.GetCultureInfo("en"));
        aliensNode.Name = "NeuralViz Aliens";

        // Act
       var aliensNodeReference = contentRepository.Save(aliensNode, SaveAction.Publish, AccessLevel.NoAccess);
        
        // Arrange
        var testAlienProduct = contentRepository.GetDefault<TestProduct>(aliensNodeReference, CultureInfo.GetCultureInfo("en"));
        testAlienProduct.Name = "Snarbo";
        testAlienProduct.Description = new XhtmlString("<p>Some scary facts about Aliens!</p>");
        
        // Act
         var testAlienProductReference = contentRepository.Save(testAlienProduct, SaveAction.Publish, AccessLevel.NoAccess);
        
        // Assert
        Assert.NotNull(aliensNodeReference);
        Assert.NotNull(testAlienProductReference);
        
        // Act
        aliensCatalog = contentRepository.Get<CatalogContent>(alienCatalogReference);
        aliensNode = contentRepository.Get<NodeContent>(aliensNodeReference);
        testAlienProduct = contentRepository.Get<TestProduct>(testAlienProductReference);
        
        // Assert
        Assert.Equal("Aliens", aliensCatalog.Name);
        Assert.Equal("NeuralViz Aliens", aliensNode.Name);
        Assert.Equal("Snarbo", testAlienProduct.Name);
    }
}