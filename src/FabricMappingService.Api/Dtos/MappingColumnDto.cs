namespace FabricMappingService.Api.Dtos;

/// <summary>
/// DTO for a mapping column.
/// </summary>
public class MappingColumnDto
{
    /// <summary>
    /// Gets or sets the name of the column.
    /// </summary>
    public string ColumnName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the data type of the column.
    /// </summary>
    public string DataType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the column.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets whether this column is required.
    /// </summary>
    public bool IsRequired { get; set; }

    /// <summary>
    /// Gets or sets the transformation to apply to values.
    /// </summary>
    public string? Transformation { get; set; }
}
