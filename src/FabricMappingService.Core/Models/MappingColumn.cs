namespace FabricMappingService.Core.Models;

/// <summary>
/// Represents a column that is mapped to a reference attribute.
/// </summary>
public class MappingColumn
{
    /// <summary>
    /// Gets or sets the name of the column being mapped.
    /// </summary>
    public string ColumnName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the data type of the column.
    /// </summary>
    public string DataType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional description of the column.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets whether this column is required.
    /// </summary>
    public bool IsRequired { get; set; }

    /// <summary>
    /// Gets or sets the transformation to apply to values from this column.
    /// For example: "uppercase", "lowercase", "trim", etc.
    /// </summary>
    public string? Transformation { get; set; }
}
