# Fabric Reference Table & Data Mapping Service Context

## Overview

This is a comprehensive reference table (lookup table) and data mapping service for Microsoft Fabric Extensibility Toolkit. It provides reference tables (Referenztabellen) for data classification and harmonization, plus attribute-based data mapping capabilities.

## Service Purpose

The service acts as a **KeyMapping outport provider** for Microsoft Fabric, enabling:
- Reference tables for data classification and harmonization
- Manual master data management (system-independent)
- Automated synchronization from source data
- Attribute-based data transformation
- ETL and data integration support

## Core Concepts

### Reference Tables (Primary Feature)

Reference tables are lookup tables (KeyMapping outports) that provide:
- **Data Classification**: Group and classify cost types, diagnoses, product categories
- **Label Harmonization**: Standardize labels and codes from different data sources
- **Manual Master Data**: Centrally maintained system-independent master data
- **Automated Sync**: Sync reference tables from source data (outports)
- **KeyMapping Outports**: Provide as KeyMapping outports for Fabric data products

### Attribute-Based Mapping (Additional Feature)

Attribute-based mapping provides:
- Property-level mapping using custom attributes
- Type conversion between compatible types
- Flexible configuration at class and property levels
- Batch operations for collections

## Architecture

### Backend Service (ASP.NET Core)

```
src/
├── FabricMappingService.Core/       # Core mapping library
│   ├── Attributes/                  # Custom mapping attributes
│   ├── Converters/                  # Type converters
│   ├── Exceptions/                  # Custom exceptions
│   ├── Models/                      # Configuration models
│   ├── Services/                    # MappingIO and AttributeMappingService
│   └── Examples/                    # Example models
└── FabricMappingService.Api/        # REST API
    ├── Controllers/                 # ReferenceTableController, MappingController
    ├── Dtos/                        # Data transfer objects
    └── Program.cs                   # API configuration
```

### Key Components

#### MappingIO Service
- `CreateReferenceTable()` - Create new reference table with columns
- `SyncMapping()` - Sync reference table from source data (adds new keys only)
- `ReadMapping()` - Read reference table as Dictionary<string, Dictionary<string, object?>>
- `AddOrUpdateRow()` - Add or update a single row
- `DeleteReferenceTable()` - Delete a reference table

#### AttributeMappingService
- `Map<TSource, TTarget>()` - Map single object
- `MapCollection<TSource, TTarget>()` - Map collections
- `MapWithResult<TSource, TTarget>()` - Map with detailed results

## Item Types

### ReferenceTable Item
- **Purpose**: Reference table for data classification (KeyMapping outport)
- **Icon**: Table
- **Capabilities**: Create, Read, Update, Delete, Share, Export
- **Properties**: tablename, columns, isvisible, outporttype
- **Use Cases**: Cost type mapping, diagnosis classification, label harmonization

### MappingConfiguration Item
- **Purpose**: Configuration for data attribute mappings
- **Icon**: Settings
- **Capabilities**: Create, Read, Update, Delete, Share
- **Properties**: sourcetype, targettype, profilename

### MappingJob Item
- **Purpose**: Execute data mapping operations
- **Icon**: Processing
- **Capabilities**: Create, Read, Delete, Execute
- **Properties**: configurationid, status

## API Endpoints

### Reference Table Endpoints (Primary)
- `GET /api/reference-tables` - List all reference tables
- `GET /api/reference-tables/{tableName}` - Get reference table data
- `POST /api/reference-tables` - Create new reference table
- `POST /api/reference-tables/sync` - Sync reference table with data
- `PUT /api/reference-tables/{tableName}/rows` - Add/update row
- `DELETE /api/reference-tables/{tableName}` - Delete reference table

### Attribute Mapping Endpoints (Additional)
- `GET /api/mapping/info` - Available mappings
- `GET /api/mapping/health` - Health check
- `POST /api/mapping/customer/legacy-to-modern` - Map customer data
- `POST /api/mapping/product/external-to-internal` - Map product data
- `POST /api/mapping/customer/batch-legacy-to-modern` - Batch map customers

## Usage Patterns

### Manual Reference Table Creation

```csharp
var mappingIO = new MappingIO(storage);

var columns = new List<ReferenceTableColumn>
{
    new() { Name = "Category", DataType = "string", Order = 1 },
    new() { Name = "Group", DataType = "string", Order = 2 }
};

mappingIO.CreateReferenceTable("vertragsprodukte", columns, true, false);

mappingIO.AddOrUpdateRow("vertragsprodukte", "VTP001", 
    new Dictionary<string, object?> { ["Category"] = "Insurance", ["Group"] = "Health" });
```

### Automated Reference Table Sync

```csharp
var products = new List<ProductData>
{
    new() { Produkt = "VTP001", Name = "Product A", Price = 100m }
};

int newKeysAdded = mappingIO.SyncMapping(
    data: products,
    keyAttributeName: "Produkt",
    mappingTableName: "produkttyp"
);
```

### Attribute Mapping

```csharp
var mapper = new AttributeMappingService();
var modern = mapper.Map<LegacyCustomerModel, ModernCustomerModel>(legacy);
```

## Integration with Fabric

### KeyMapping Outports
- Reference tables are provided as **KeyMapping outports**
- The key element is automatically designated as "key"
- Can be consumed by other data products in Fabric
- Supports OneLake integration for storage

### Workflow
1. Create reference table via API or sync from source data
2. Read mapping: `var mapping = mappingIO.ReadMapping("tableName")`
3. Provide as KeyMapping outport for consumption

## Technical Stack

- **.NET 10.0** - Framework
- **ASP.NET Core** - Web API
- **xUnit** - Testing
- **Swashbuckle** - OpenAPI/Swagger
- **Microsoft Fabric Extensibility Toolkit** - Integration

## Best Practices

### Reference Tables
1. Use `SyncMapping()` for automated table creation from source data
2. Only NEW keys are added - existing keys are NOT updated
3. The key column is always named "key" (automatic)
4. Use descriptive column names for classifications
5. Provide as KeyMapping outport type in Fabric

### Attribute Mapping
1. Use `[MapTo]` attribute for property mapping
2. Apply `[IgnoreMapping]` to exclude properties
3. Use `[MappingProfile]` for class-level configuration
4. Handle type conversions automatically
5. Use batch operations for collections

## Security Considerations

- Input validation for all API requests
- Proper error handling and logging
- Authentication via Entra ID (Azure AD)
- Secure storage for sensitive configuration

## Testing

- Comprehensive unit tests in `tests/FabricMappingService.Tests/`
- Test coverage for reference tables and attribute mapping
- Type conversion tests
- Error handling tests

## Future Enhancements

- Advanced filtering and querying of reference tables
- Version control for reference tables
- Import/export capabilities
- UI components for reference table management
- Advanced transformation rules
- Integration with more data sources
