namespace FabricMappingService.Core.Models;

/// <summary>
/// Configuration options for the mapping service.
/// </summary>
public class MappingConfiguration
{
    /// <summary>
    /// Gets or sets whether to perform case-sensitive property name matching.
    /// Default is true.
    /// </summary>
    public bool CaseSensitive { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to ignore properties without explicit mapping attributes.
    /// If true, only properties with MapTo attribute will be mapped.
    /// Default is false.
    /// </summary>
    public bool IgnoreUnmapped { get; set; } = false;

    /// <summary>
    /// Gets or sets whether to throw exceptions on mapping errors.
    /// If false, errors will be logged but not thrown.
    /// Default is true.
    /// </summary>
    public bool ThrowOnError { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to map null values.
    /// If false, null source values will be skipped.
    /// Default is true.
    /// </summary>
    public bool MapNullValues { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum depth for nested object mapping.
    /// This prevents infinite recursion in circular references.
    /// Default is 10.
    /// </summary>
    public int MaxDepth { get; set; } = 10;
}
