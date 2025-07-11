using System.Globalization;
using EPiServer;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Core;
using EPiServer.DataAccess;
using Mediachase.Commerce.Catalog;
using Microsoft.Extensions.DependencyInjection;
using Optimizely.TestContainers.Models.Commerce;

namespace OptimizelyTestContainers.Tests;

public class CommerceCatalogIntegrationTests() : OptimizelyIntegrationTestBase(includeCommerce: true)
{
    [Fact]
    public void Can_Save_Node_And_Product()
    {
        // Arrange #1
        var referenceConverter = Services.GetRequiredService<ReferenceConverter>();
        var contentRepository = Services.GetRequiredService<IContentRepository>();

        var rootLink = referenceConverter.GetRootLink();

        var aliensNode = contentRepository.GetDefault<NodeContent>(rootLink, CultureInfo.GetCultureInfo("en"));
        aliensNode.Name = "Aliens";

        // Act #1
        var aliensNodeReference = contentRepository.Save(aliensNode, SaveAction.Publish);
        
        // Arrange #2
        var testAlienProduct = contentRepository.GetDefault<TestProduct>(aliensNodeReference, CultureInfo.GetCultureInfo("en"));
        testAlienProduct.Name = "Snarbo";
        testAlienProduct.Description = new XhtmlString("<p>Some scary facts about Aliens!</p>");
        
        // Act #2
         var testAlienProductReference = contentRepository.Save(testAlienProduct, SaveAction.Publish);
        
        // Assert # 1 & 2
        Assert.NotNull(aliensNodeReference);
        Assert.NotNull(testAlienProductReference);
        
        // Act #3
        aliensNode = contentRepository.Get<NodeContent>(aliensNodeReference);
        testAlienProduct = contentRepository.Get<TestProduct>(testAlienProductReference);
        
        // Assert #3
        Assert.Equal("Aliens", aliensNode.Name);
        Assert.Equal("Snarbo", testAlienProduct.Name);
    }
}