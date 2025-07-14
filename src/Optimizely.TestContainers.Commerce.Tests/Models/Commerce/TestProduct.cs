using System.ComponentModel.DataAnnotations;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Catalog.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;

namespace Optimizely.TestContainers.Commerce.Tests.Models.Commerce;

[CatalogContentType(
    GUID = "0B06DE9B-6AE3-40FB-909E-E718CCC260AE",
    DisplayName = "Test Product",
    Description = "Test product for integration tests.")]
public class TestProduct : ProductContent
{
    [Display(
        Name = "Description", 
        GroupName = SystemTabNames.Content, 
        Order = 1)]
    [Searchable]
    [CultureSpecific]
    [Tokenize]
    [IncludeInDefaultSearch]
    public virtual XhtmlString? Description { get; set; }
}