using System.Globalization;
using EPiServer;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Core;
using EPiServer.DataAccess;
using Mediachase.Commerce.Catalog;
using Microsoft.Extensions.DependencyInjection;
using Optimizely.TestContainers.Models.Commerce;

namespace OptimizelyTestContainers.Tests;

public class CommerceCatalogIntegrationTests : OptimizelyCommerceIntegrationTestBase
{
    [Fact]
    public void Can_Save_Category()
    {
        var referenceConverter = Services.GetRequiredService<ReferenceConverter>();
        var contentRepository = Services.GetRequiredService<IContentRepository>();

        var rootLink = referenceConverter.GetRootLink();

        var aliensNode = contentRepository.GetDefault<NodeContent>(rootLink, CultureInfo.GetCultureInfo("en"));
        aliensNode.Name = "Aliens";
        var aliensNodeReference = contentRepository.Save(aliensNode, SaveAction.Publish);

        var testAlienProduct = contentRepository.GetDefault<TestProduct>(aliensNodeReference, CultureInfo.GetCultureInfo("en"));
        testAlienProduct.Name = "Snarbo";
        testAlienProduct.Description = new XhtmlString("<p>Some scary facts about Aliens!</p>");
        var testAlienProductReference = contentRepository.Save(testAlienProduct, SaveAction.Publish);
        
        Assert.NotNull(aliensNodeReference);
        Assert.NotNull(testAlienProductReference);

        aliensNode = contentRepository.Get<NodeContent>(aliensNodeReference);
        Assert.Equal("Aliens", aliensNode.Name);
        
        testAlienProduct = contentRepository.Get<TestProduct>(testAlienProductReference);
        Assert.Equal("Snarbo", testAlienProduct.Name);
    }
}