using System.ComponentModel.DataAnnotations;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Catalog.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using Optimizely.TestContainers.Commerce.Tests.Models.Commerce;

namespace Optimizely.TestContainers.Commerce.Tests.Models.Commerce;

public class TestProductTests
{
    [Fact]
    public void TestProduct_Should_Have_CatalogContentType_Attribute()
    {
        // Arrange & Act
        var attribute = typeof(TestProduct).GetCustomAttributes(typeof(CatalogContentTypeAttribute), false)
            .FirstOrDefault() as CatalogContentTypeAttribute;

        // Assert
        Assert.NotNull(attribute);
        Assert.Equal("0B06DE9B-6AE3-40FB-909E-E718CCC260AE", attribute.GUID);
        Assert.Equal("Test Product", attribute.DisplayName);
        Assert.Equal("Test product for integration tests.", attribute.Description);
    }

    [Fact]
    public void TestProduct_Should_Inherit_From_ProductContent()
    {
        // Arrange & Act
        var isProductContent = typeof(ProductContent).IsAssignableFrom(typeof(TestProduct));

        // Assert
        Assert.True(isProductContent);
    }

    [Fact]
    public void Description_Property_Should_Be_Virtual()
    {
        // Arrange
        var property = typeof(TestProduct).GetProperty(nameof(TestProduct.Description));

        // Act & Assert
        Assert.NotNull(property);
        Assert.True(property.GetMethod?.IsVirtual);
    }

    [Fact]
    public void Description_Property_Should_Have_Display_Attribute()
    {
        // Arrange
        var property = typeof(TestProduct).GetProperty(nameof(TestProduct.Description));

        // Act
        var displayAttribute = property?.GetCustomAttributes(typeof(DisplayAttribute), false)
            .FirstOrDefault() as DisplayAttribute;

        // Assert
        Assert.NotNull(displayAttribute);
        Assert.Equal("Description", displayAttribute.Name);
        Assert.Equal(SystemTabNames.Content, displayAttribute.GroupName);
        Assert.Equal(1, displayAttribute.Order);
    }

    [Fact]
    public void Description_Property_Should_Have_Searchable_Attribute()
    {
        // Arrange
        var property = typeof(TestProduct).GetProperty(nameof(TestProduct.Description));

        // Act
        var attribute = property?.GetCustomAttributes(typeof(SearchableAttribute), false)
            .FirstOrDefault();

        // Assert
        Assert.NotNull(attribute);
    }

    [Fact]
    public void Description_Property_Should_Have_CultureSpecific_Attribute()
    {
        // Arrange
        var property = typeof(TestProduct).GetProperty(nameof(TestProduct.Description));

        // Act
        var attribute = property?.GetCustomAttributes(typeof(CultureSpecificAttribute), false)
            .FirstOrDefault();

        // Assert
        Assert.NotNull(attribute);
    }

    [Fact]
    public void Description_Property_Should_Have_Tokenize_Attribute()
    {
        // Arrange
        var property = typeof(TestProduct).GetProperty(nameof(TestProduct.Description));

        // Act
        var attribute = property?.GetCustomAttributes(typeof(TokenizeAttribute), false)
            .FirstOrDefault();

        // Assert
        Assert.NotNull(attribute);
    }

    [Fact]
    public void Description_Property_Should_Have_IncludeInDefaultSearch_Attribute()
    {
        // Arrange
        var property = typeof(TestProduct).GetProperty(nameof(TestProduct.Description));

        // Act
        var attribute = property?.GetCustomAttributes(typeof(IncludeInDefaultSearchAttribute), false)
            .FirstOrDefault();

        // Assert
        Assert.NotNull(attribute);
    }

    [Fact]
    public void TestProduct_Can_Be_Instantiated()
    {
        // Act
        var testProduct = new TestProduct();

        // Assert
        Assert.NotNull(testProduct);
    }

    [Fact]
    public void Description_Property_Can_Be_Set_And_Retrieved()
    {
        // Arrange
        var testProduct = new TestProduct();
        var expectedDescription = new XhtmlString("<p>Test product description</p>");

        // Act
        testProduct.Description = expectedDescription;

        // Assert
        Assert.Equal(expectedDescription, testProduct.Description);
    }
}
