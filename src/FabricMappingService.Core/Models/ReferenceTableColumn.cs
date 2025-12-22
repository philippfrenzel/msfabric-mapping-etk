namespace FabricMappingService.Core.Models;

/// <summary>
/// Represents a column definition in a reference table.
/// </summary>
public class ReferenceTableColumn
{
    /// <summary>
    /// Gets or sets the name of the column.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the data type of the column (e.g., "string", "int", "decimal").
    /// </summary>
    public string DataType { get; set; } = "string";

    /// <summary>
    /// Gets or sets the description of the column.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the display order of the column.
    /// </summary>
    public int Order { get; set; }
}
