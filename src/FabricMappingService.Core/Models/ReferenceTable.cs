namespace FabricMappingService.Core.Models;

/// <summary>
/// Represents a reference table (lookup table) for data mapping and classification.
/// Reference tables help group, rename, or standardize data values in a consistent form.
/// </summary>
public class ReferenceTable
{
    /// <summary>
    /// Gets or sets the name of the reference table.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the key column.
    /// The key column is automatically named "key" in the table.
    /// </summary>
    public string KeyColumnName { get; set; } = "key";

    /// <summary>
    /// Gets or sets the list of additional columns beyond the key column.
    /// </summary>
    public List<ReferenceTableColumn> Columns { get; set; } = [];

    /// <summary>
    /// Gets or sets the rows of data in the reference table.
    /// Each row contains a key and associated attribute values.
    /// </summary>
    public List<ReferenceTableRow> Rows { get; set; } = [];

    /// <summary>
    /// Gets or sets when the reference table was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets when the reference table was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets whether the reference table is visible in the UI.
    /// </summary>
    public bool IsVisible { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to send email notifications when new mappings are added.
    /// </summary>
    public bool NotifyOnNewMapping { get; set; }
}
