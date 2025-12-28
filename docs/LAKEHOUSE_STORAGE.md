# Lakehouse Storage Configuration Guide

This guide explains how to configure and use the lakehouse-based storage for reference tables in the Fabric Mapping Service.

## Overview

The Fabric Mapping Service supports two storage modes for reference tables:
1. **In-Memory Storage** (default for development) - Data is stored in memory and lost on restart
2. **Lakehouse Storage** (recommended for production) - Data is persisted to a lakehouse as JSON files

## Why Lakehouse Storage?

Lakehouse storage provides several benefits:
- **Persistence**: Reference table configurations and data survive service restarts
- **Scalability**: Store large reference tables without memory constraints
- **Integration**: Configurations and mappings live together in the same lakehouse as source data
- **Portability**: JSON format is human-readable and easy to migrate
- **Fabric Native**: Aligns with Microsoft Fabric's lakehouse-first architecture

## Configuration

### appsettings.json

Configure storage mode in your `appsettings.json` file:

```json
{
  "LakehouseStorage": {
    "UseInMemoryStorage": false,
    "BasePath": "./data/lakehouse",
    "WorkspaceId": "your-workspace-id",
    "LakehouseItemId": "your-lakehouse-id"
  }
}
```

#### Configuration Options

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `UseInMemoryStorage` | bool | Yes | `true` for in-memory storage, `false` for lakehouse storage |
| `BasePath` | string | Yes | Base directory path for storing reference tables |
| `WorkspaceId` | string | No | Microsoft Fabric workspace ID (optional, for Fabric integration) |
| `LakehouseItemId` | string | No | Microsoft Fabric lakehouse item ID (optional, for Fabric integration) |

### Environment-Specific Configuration

#### Development (`appsettings.Development.json`)

For development, use in-memory storage for faster iteration:

```json
{
  "LakehouseStorage": {
    "UseInMemoryStorage": true,
    "BasePath": "./data/lakehouse-dev"
  }
}
```

#### Production (`appsettings.json`)

For production, use lakehouse storage with appropriate paths:

```json
{
  "LakehouseStorage": {
    "UseInMemoryStorage": false,
    "BasePath": "/mnt/lakehouse/FabricMappingService",
    "WorkspaceId": "12345678-1234-1234-1234-123456789012",
    "LakehouseItemId": "87654321-4321-4321-4321-210987654321"
  }
}
```

## Storage Structure

When using lakehouse storage, reference tables are organized as follows:

```
{BasePath}/
├── ReferenceTableConfigurations/
│   ├── products_config.json
│   ├── categories_config.json
│   └── customers_config.json
└── ReferenceTableData/
    ├── products_data.json
    ├── categories_data.json
    └── customers_data.json
```

### Configuration Files

Configuration files (`*_config.json`) store table metadata:

```json
{
  "name": "products",
  "keyColumnName": "key",
  "columns": [
    {
      "name": "Category",
      "dataType": "string",
      "description": "Product category",
      "order": 1
    },
    {
      "name": "SubCategory",
      "dataType": "string",
      "description": "Product subcategory",
      "order": 2
    }
  ],
  "isVisible": true,
  "notifyOnNewMapping": true,
  "sourceLakehouseItemId": "lakehouse-123",
  "sourceWorkspaceId": "workspace-456",
  "sourceTableName": "ProductsTable",
  "sourceOneLakeLink": "https://onelake.dfs.fabric.microsoft.com/...",
  "createdAt": "2024-01-15T10:30:00Z",
  "updatedAt": "2024-01-15T14:22:00Z"
}
```

### Data Files

Data files (`*_data.json`) store the actual reference table rows:

```json
{
  "PROD001": {
    "key": "PROD001",
    "Category": "Electronics",
    "SubCategory": "Computers"
  },
  "PROD002": {
    "key": "PROD002",
    "Category": "Electronics",
    "SubCategory": "Phones"
  }
}
```

## Usage Examples

### Creating a Reference Table

When lakehouse storage is enabled, reference tables are automatically persisted:

```bash
curl -X POST https://localhost:5001/api/reference-tables \
  -H "Content-Type: application/json" \
  -d '{
    "tableName": "products",
    "columns": [
      {
        "name": "Category",
        "dataType": "string",
        "description": "Product category",
        "order": 1
      }
    ],
    "isVisible": true,
    "notifyOnNewMapping": false
  }'
```

This will create:
- `/data/lakehouse/ReferenceTableConfigurations/products_config.json`
- `/data/lakehouse/ReferenceTableData/products_data.json`

### Syncing Data

Sync operations also persist to lakehouse:

```bash
curl -X POST https://localhost:5001/api/reference-tables/sync \
  -H "Content-Type: application/json" \
  -d '{
    "mappingTableName": "products",
    "keyAttributeName": "ProductCode",
    "data": [
      { "ProductCode": "PROD001", "Name": "Laptop" },
      { "ProductCode": "PROD002", "Name": "Mouse" }
    ]
  }'
```

### Reading Data

Reading operations transparently load from lakehouse:

```bash
curl https://localhost:5001/api/reference-tables/products
```

## Migration from In-Memory Storage

To migrate from in-memory to lakehouse storage:

1. **Backup Current Data**: Export all reference tables via the API
2. **Update Configuration**: Change `UseInMemoryStorage` to `false` in `appsettings.json`
3. **Set BasePath**: Configure appropriate lakehouse path
4. **Restart Service**: Restart the API service
5. **Re-import Data**: Import the backed-up reference tables

### Backup Script Example

```bash
# Backup all reference tables
curl https://localhost:5001/api/reference-tables > tables.json

# For each table, backup its data
for table in $(jq -r '.tables[]' tables.json); do
  curl https://localhost:5001/api/reference-tables/$table > backup_${table}.json
done
```

### Restore Script Example

```bash
# Restore each table
for file in backup_*.json; do
  table=$(basename $file .json | sed 's/backup_//')
  # First create the table structure
  # Then sync the data
done
```

## Best Practices

### Development
- Use `UseInMemoryStorage: true` for fast iteration
- Set BasePath to a local temporary directory
- Don't commit the data directory to version control

### Production
- Use `UseInMemoryStorage: false` for data persistence
- Set BasePath to a mounted lakehouse volume
- Configure appropriate permissions on the lakehouse path
- Implement regular backups of the ReferenceTableConfigurations and ReferenceTableData directories
- Monitor disk space usage

### Security
- Ensure proper file system permissions on BasePath
- Use Fabric's built-in security for workspace and lakehouse access
- Don't store sensitive data in reference table configurations
- Implement audit logging for reference table changes

## Troubleshooting

### Issue: Files not being created

**Cause**: Insufficient permissions on BasePath

**Solution**: Ensure the application has write permissions:
```bash
chmod 755 /path/to/lakehouse
```

### Issue: Data not persisting between restarts

**Cause**: `UseInMemoryStorage` is still set to `true`

**Solution**: Verify appsettings.json configuration and restart the service.

### Issue: Large reference tables causing performance issues

**Cause**: Loading large JSON files synchronously

**Solution**: 
- Consider implementing pagination for very large reference tables
- Use Azure Data Lake Storage Gen2 APIs for production lakehouse access
- Implement caching for frequently accessed reference tables

## Future Enhancements

The current implementation uses file-based storage. Future versions may include:

1. **Azure Data Lake Storage Gen2 Integration**: Direct integration with ADLS Gen2 APIs
2. **Fabric REST API Integration**: Use Fabric APIs to access OneLake directly
3. **Caching Layer**: In-memory cache with write-through to lakehouse
4. **Versioning**: Track changes to reference table configurations and data
5. **Partitioning**: Support for partitioned reference tables for better performance
6. **Delta Lake Format**: Store data in Delta Lake format instead of JSON

## API Changes

No API changes are required when switching between storage modes. All endpoints work identically with both in-memory and lakehouse storage.

## Related Documentation

- [API Documentation](API.md) - Complete API reference
- [Fabric Integration Guide](FABRIC-INTEGRATION.md) - Microsoft Fabric integration details
- [Project Setup Guide](PROJECT_SETUP.md) - Environment setup instructions
