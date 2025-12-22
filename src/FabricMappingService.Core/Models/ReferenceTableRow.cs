namespace FabricMappingService.Core.Models;

/// <summary>
/// Represents a single row of data in a reference table.
/// </summary>
public class ReferenceTableRow
{
    /// <summary>
    /// Gets or sets the key value for this row.
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the attribute values for this row.
    /// The dictionary key is the column name, and the value is the cell data.
    /// </summary>
    public Dictionary<string, object?> Attributes { get; set; } = [];

    /// <summary>
    /// Gets or sets when this row was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets when this row was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets whether this row is newly added (not yet mapped/classified).
    /// </summary>
    public bool IsNew { get; set; } = true;
}
