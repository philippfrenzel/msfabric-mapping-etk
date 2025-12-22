namespace FabricMappingService.Api.Dtos;

/// <summary>
/// Request to create a new reference table.
/// </summary>
public class CreateReferenceTableRequest
{
    /// <summary>
    /// Gets or sets the name of the reference table.
    /// </summary>
    public string TableName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the columns for the reference table.
    /// </summary>
    public List<ColumnDefinition> Columns { get; set; } = [];

    /// <summary>
    /// Gets or sets whether the table is visible in the UI.
    /// </summary>
    public bool IsVisible { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to send notifications when new mappings are added.
    /// </summary>
    public bool NotifyOnNewMapping { get; set; }

    /// <summary>
    /// Gets or sets the OneLake item ID of the lakehouse table that provides the reference data.
    /// </summary>
    public string? SourceLakehouseItemId { get; set; }

    /// <summary>
    /// Gets or sets the workspace ID of the lakehouse that contains the reference table.
    /// </summary>
    public string? SourceWorkspaceId { get; set; }

    /// <summary>
    /// Gets or sets the name of the table in the lakehouse that provides the reference data.
    /// </summary>
    public string? SourceTableName { get; set; }

    /// <summary>
    /// Gets or sets the OneLake link to the source table.
    /// </summary>
    public string? SourceOneLakeLink { get; set; }
}

/// <summary>
/// Column definition for a reference table.
/// </summary>
public class ColumnDefinition
{
    /// <summary>
    /// Gets or sets the name of the column.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the data type of the column.
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
