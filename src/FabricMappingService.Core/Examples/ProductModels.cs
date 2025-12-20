using FabricMappingService.Core.Attributes;

namespace FabricMappingService.Core.Examples;

/// <summary>
/// Example source model for product data from an external API.
/// </summary>
public class ExternalProductModel
{
    [MapTo("ProductId")]
    public string ProductCode { get; set; } = string.Empty;

    [MapTo("Name")]
    public string ProductTitle { get; set; } = string.Empty;

    [MapTo("Description")]
    public string ProductDescription { get; set; } = string.Empty;

    [MapTo("Price")]
    public string PriceString { get; set; } = string.Empty; // Will be converted to decimal

    [MapTo("Quantity")]
    public string StockLevel { get; set; } = string.Empty; // Will be converted to int

    [MapTo("CategoryName")]
    public string Category { get; set; } = string.Empty;

    [MapTo("IsAvailable")]
    public string InStock { get; set; } = string.Empty; // Will be converted to bool
}

/// <summary>
/// Example target model for product data in internal system.
/// </summary>
public class InternalProductModel
{
    public string ProductId { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int Quantity { get; set; }

    public string CategoryName { get; set; } = string.Empty;

    public bool IsAvailable { get; set; }
}
