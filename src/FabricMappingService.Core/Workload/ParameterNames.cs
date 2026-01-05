namespace FabricMappingService.Core.Workload;

/// <summary>
/// Defines constant parameter names used in workload configurations.
/// </summary>
public static class ParameterNames
{
    // Reference Table Parameters
    public const string TableName = "tableName";
    public const string Columns = "columns";
    public const string IsVisible = "isVisible";
    public const string NotifyOnNewMapping = "notifyOnNewMapping";
    public const string SourceLakehouseItemId = "sourceLakehouseItemId";
    public const string SourceWorkspaceId = "sourceWorkspaceId";
    public const string SourceTableName = "sourceTableName";
    public const string SourceOneLakeLink = "sourceOneLakeLink";
    
    // Sync Parameters
    public const string KeyAttributeName = "keyAttributeName";
    public const string Data = "data";
    
    // Row Update Parameters
    public const string Key = "key";
    public const string Attributes = "attributes";
    
    // Mapping Parameters
    public const string SourceData = "sourceData";
    public const string TargetType = "targetType";
    
    // Item Parameters
    public const string DisplayName = "displayName";
    public const string WorkspaceId = "workspaceId";
    public const string LakehouseItemId = "lakehouseItemId";
    public const string LakehouseWorkspaceId = "lakehouseWorkspaceId";
    public const string ReferenceAttributeName = "referenceAttributeName";
    public const string MappingColumns = "mappingColumns";
    public const string OneLakeLink = "oneLakeLink";
    public const string Description = "description";
    
    // Item Operation Parameters
    public const string ItemId = "itemId";
}
