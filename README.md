# Fabric Data Attribute Mapping Service

A comprehensive data attribute mapping service for Microsoft Fabric Extensibility Toolkit that enables seamless transformation of data between different object structures using attribute-based configuration.

## 🎯 Overview

This project implements a powerful and flexible data attribute mapping service in C# that integrates with Microsoft Fabric using the Extensibility Toolkit. It allows you to map data between different object structures using custom attributes, making it ideal for data integration, ETL processes, and legacy system modernization.

## 🏆 Competition Entry

This project is created for the [Microsoft Fabric Extensibility Toolkit Contest](https://blog.fabric.microsoft.com/en-us/blog/announcing-the-fabric-extensibility-toolkit-contest) to demonstrate the capabilities of building custom workloads for Microsoft Fabric.

## ✨ Features

- **Attribute-Based Mapping**: Use custom attributes to define mappings between source and target properties
- **Reference Tables (Lookup Tables)**: Create and manage reference tables for data classification and harmonization
- **Type Conversion**: Automatic conversion between compatible types (string to int, bool, decimal, etc.)
- **Flexible Configuration**: Configure mapping behavior at both class and property levels
- **Batch Operations**: Map collections of objects efficiently
- **Error Handling**: Detailed error reporting and validation
- **REST API**: Full-featured ASP.NET Core Web API for mapping operations
- **Extensible**: Support for custom converters and mapping profiles
- **Microsoft Fabric Integration**: Ready for integration with Fabric workspaces

## 🏗️ Architecture

```
FabricMappingService/
├── src/
│   ├── FabricMappingService.Core/       # Core mapping library
│   │   ├── Attributes/                  # Custom mapping attributes
│   │   ├── Converters/                  # Type converters
│   │   ├── Exceptions/                  # Custom exceptions
│   │   ├── Models/                      # Configuration models
│   │   ├── Services/                    # Mapping service implementation
│   │   └── Examples/                    # Example models
│   └── FabricMappingService.Api/        # REST API
│       ├── Controllers/                 # API controllers
│       ├── Dtos/                        # Data transfer objects
│       └── Program.cs                   # API configuration
├── tests/
│   └── FabricMappingService.Tests/      # Unit tests
├── fabric-manifest/                     # Fabric workload manifest
└── docs/                                # Documentation

```

## 🚀 Quick Start

### Prerequisites

- .NET 10.0 SDK or later (for local development)
  - For Azure deployment, retarget to .NET 8.0 or 9.0 in `.csproj` files if needed
- Visual Studio 2022 or VS Code (optional)
- Microsoft Fabric workspace (for integration)

### Building the Project

```bash
# Clone the repository
git clone https://github.com/philippfrenzel/msfabric-mapping-etk.git
cd msfabric-mapping-etk

# Build the solution
dotnet build

# Run tests
dotnet test

# Run the API
cd src/FabricMappingService.Api
dotnet run
```

The API will be available at `https://localhost:5001` (or the port specified in launchSettings.json).

## 📖 Usage Examples

### Basic Attribute Mapping

Define your source and target models with mapping attributes:

```csharp
using FabricMappingService.Core.Attributes;

// Source model
public class LegacyCustomerModel
{
    [MapTo("CustomerId")]
    public int Id { get; set; }

    [MapTo("FullName")]
    public string CustomerName { get; set; }

    [IgnoreMapping]
    public string InternalNotes { get; set; }
}

// Target model
public class ModernCustomerModel
{
    public int CustomerId { get; set; }
    public string FullName { get; set; }
}
```

Map the objects:

```csharp
using FabricMappingService.Core.Services;

var mapper = new AttributeMappingService();

var legacy = new LegacyCustomerModel 
{ 
    Id = 123, 
    CustomerName = "John Doe" 
};

var modern = mapper.Map<LegacyCustomerModel, ModernCustomerModel>(legacy);
// modern.CustomerId = 123
// modern.FullName = "John Doe"
```

### Using the REST API

#### Map a single customer

```bash
curl -X POST https://localhost:5001/api/mapping/customer/legacy-to-modern \
  -H "Content-Type: application/json" \
  -d '{
    "id": 123,
    "customerName": "John Doe",
    "email": "john@example.com",
    "phone": "+1234567890",
    "createdDate": "2024-01-01T00:00:00Z",
    "status": true,
    "country": "USA"
  }'
```

Response:

```json
{
  "success": true,
  "data": {
    "customerId": 123,
    "fullName": "John Doe",
    "emailAddress": "john@example.com",
    "phoneNumber": "+1234567890",
    "registrationDate": "2024-01-01T00:00:00Z",
    "isActive": true,
    "country": "USA"
  },
  "errors": [],
  "warnings": [],
  "mappedPropertiesCount": 7
}
```

#### Get service information

```bash
curl https://localhost:5001/api/mapping/info
```

#### Health check

```bash
curl https://localhost:5001/api/mapping/health
```

### Advanced Features

#### Custom Mapping Profile

```csharp
[MappingProfile("StrictMapping", IgnoreUnmapped = true, CaseSensitive = false)]
public class SourceModel
{
    [MapTo("TargetId")]
    public int Id { get; set; }
}
```

#### Batch Mapping

```csharp
var sources = new List<LegacyCustomerModel> { /* ... */ };
var results = mapper.MapCollection<LegacyCustomerModel, ModernCustomerModel>(sources);
```

#### Detailed Results

```csharp
var result = mapper.MapWithResult<SourceModel, TargetModel>(source);
if (result.Success)
{
    Console.WriteLine($"Mapped {result.MappedPropertiesCount} properties");
    // Use result.Result
}
else
{
    Console.WriteLine($"Errors: {string.Join(", ", result.Errors)}");
}
```

### Reference Tables (Lookup Tables)

Reference tables (Referenztabellen) provide a powerful way to classify, group, and harmonize data values. They act as lookup tables that help structure data consistently and make it comparable across different sources.

#### What is a Reference Table?

A reference table is essentially a list that defines how certain values are grouped, renamed, or standardized. It works like a lookup table that helps with data analysis by providing clear structure and comparability.

**Use Cases:**
- **Manual Master Data**: Centrally maintained system-independent master data
- **Cost Type Mapping**: Classifying and mapping cost categories
- **Diagnosis Classification**: Standardizing medical or technical diagnoses
- **Label Harmonization**: Unifying labels from different sources
- **Product Grouping**: Creating product type hierarchies

#### Scenario 1: Manual Reference Table (Independent of Source Data)

Create a reference table manually for custom classifications that don't directly map to source system data.

**Create an empty reference table:**

```csharp
var mappingIO = new MappingIO(storage);

var columns = new List<ReferenceTableColumn>
{
    new() { Name = "Category", DataType = "string", Order = 1, Description = "Product category" },
    new() { Name = "Group", DataType = "string", Order = 2, Description = "Product group" }
};

mappingIO.CreateReferenceTable(
    tableName: "vertragsprodukte",
    columns: columns,
    isVisible: true,
    notifyOnNewMapping: false);
```

**Add rows to the table:**

```csharp
mappingIO.AddOrUpdateRow(
    tableName: "vertragsprodukte",
    key: "VTP001",
    attributes: new Dictionary<string, object?>
    {
        ["Category"] = "Insurance",
        ["Group"] = "Health"
    });
```

**Read the mapping:**

```csharp
var mappingData = mappingIO.ReadMapping("vertragsprodukte");
// Returns: Dictionary<string, Dictionary<string, object?>>
// Key: "VTP001" -> { "key": "VTP001", "Category": "Insurance", "Group": "Health" }
```

#### Scenario 2: Automated Reference Table from Source Data

Automatically create reference tables from source data (outports). This approach is more structured and professional when codes from data sources need to be classified.

**Define your source data model:**

```csharp
public class ProductData
{
    public string Produkt { get; set; }  // This will be the key
    public string Name { get; set; }
    public decimal Price { get; set; }
}
```

**Sync the reference table with source data:**

```csharp
var mappingIO = new MappingIO(storage);

var products = new List<ProductData>
{
    new() { Produkt = "VTP001", Name = "Product A", Price = 100m },
    new() { Produkt = "VTP002", Name = "Product B", Price = 200m },
    new() { Produkt = "VTP003", Name = "Product C", Price = 300m }
};

// Synchronize the reference table
// Creates table if it doesn't exist, adds new keys only
int newKeysAdded = mappingIO.SyncMapping(
    data: products,
    keyAttributeName: "Produkt",  // Property containing the key values
    mappingTableName: "produkttyp");

Console.WriteLine($"Added {newKeysAdded} new keys");
```

**Important Notes:**
- The key column is automatically named "key"
- `SyncMapping` only adds NEW keys, existing keys are NOT updated
- Removed values are NOT automatically deleted (must be done manually)
- The key should not be changed as it's automatically created
- Notifications for new mappings are sent only once per key

**Read and use the mapping:**

```csharp
// Read the reference table
var produktMapping = mappingIO.ReadMapping("produkttyp");

// Use the mapping data
foreach (var entry in produktMapping)
{
    Console.WriteLine($"Key: {entry.Key}, Data: {string.Join(", ", entry.Value)}");
}
```

#### Using Reference Tables via REST API

**Create a reference table:**

```bash
curl -X POST https://localhost:5001/api/reference-tables \
  -H "Content-Type: application/json" \
  -d '{
    "tableName": "vertragsprodukte",
    "columns": [
      {
        "name": "Category",
        "dataType": "string",
        "description": "Product category",
        "order": 1
      },
      {
        "name": "Group",
        "dataType": "string",
        "description": "Product group",
        "order": 2
      }
    ],
    "isVisible": true,
    "notifyOnNewMapping": false
  }'
```

**Sync a reference table with data:**

```bash
curl -X POST https://localhost:5001/api/reference-tables/sync \
  -H "Content-Type: application/json" \
  -d '{
    "mappingTableName": "produkttyp",
    "keyAttributeName": "Produkt",
    "data": [
      { "Produkt": "VTP001", "Name": "Product A", "Price": 100 },
      { "Produkt": "VTP002", "Name": "Product B", "Price": 200 }
    ]
  }'
```

**Response:**
```json
{
  "success": true,
  "tableName": "produkttyp",
  "newKeysAdded": 2,
  "totalKeys": 2
}
```

**Read a reference table:**

```bash
curl https://localhost:5001/api/reference-tables/produkttyp
```

**List all reference tables:**

```bash
curl https://localhost:5001/api/reference-tables
```

**Add or update a row:**

```bash
curl -X PUT https://localhost:5001/api/reference-tables/produkttyp/rows \
  -H "Content-Type: application/json" \
  -d '{
    "key": "VTP001",
    "attributes": {
      "Category": "Insurance",
      "Group": "Health",
      "SubGroup": "Basic Coverage"
    }
  }'
```

**Delete a reference table:**

```bash
curl -X DELETE https://localhost:5001/api/reference-tables/produkttyp
```

#### Integration with Microsoft Fabric

When creating outports in the Fabric client:
- Use outport type **"Keymapping"** for reference table data
- The key element is automatically designated as "key"
- Reference tables can be consumed as outports by other data products

**Example workflow:**
1. Create reference table via API or sync from source data
2. Read the mapping: `var mapping = mappingIO.ReadMapping("tableName")`
3. Provide as outport for consumption by analytics

```csharp
// Example: Providing reference table as outport
var mapping = mappingIO.ReadMapping("produkttyp");

// Convert to DataFrame/output format
// Write to outport for consumption
outportWriter.Write(mapping, "Produkttyp_Mapping");
```

## 🧪 Testing

The project includes comprehensive unit tests:

```bash
dotnet test
```

Test coverage includes:
- Attribute mapping functionality
- Reference table operations
- Type conversion
- Error handling
- Batch operations
- Configuration options

## 🔧 Configuration

Configure the mapping service behavior:

```csharp
var configuration = new MappingConfiguration
{
    CaseSensitive = true,           // Case-sensitive property matching
    IgnoreUnmapped = false,         // Map properties without attributes
    ThrowOnError = true,            // Throw exceptions on errors
    MapNullValues = true,           // Map null values
    MaxDepth = 10                   // Max depth for nested mapping
};

var mapper = new AttributeMappingService(configuration);
```

## 🌐 Microsoft Fabric Integration

### Workload Manifest

The project includes a complete workload manifest for Fabric integration (`fabric-manifest/workload-manifest.json`).

### Setup for Fabric

1. **Register your application** in Microsoft Entra ID
2. **Update the manifest** with your AAD App ID and backend URL
3. **Deploy the API** to Azure App Service or your hosting platform
4. **Register the workload** in your Fabric tenant
5. **Configure permissions** for workspace access

### Item Types

The service defines two item types for Fabric:

- **MappingConfiguration**: Store and manage mapping configurations
- **MappingJob**: Execute mapping operations

## 📚 API Endpoints

### Attribute Mapping Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/` | Service information |
| GET | `/api/mapping/info` | Available mappings |
| GET | `/api/mapping/health` | Health check |
| POST | `/api/mapping/customer/legacy-to-modern` | Map legacy customer |
| POST | `/api/mapping/product/external-to-internal` | Map external product |
| POST | `/api/mapping/customer/batch-legacy-to-modern` | Batch map customers |

### Reference Table Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/reference-tables` | List all reference tables |
| GET | `/api/reference-tables/{tableName}` | Get reference table data |
| POST | `/api/reference-tables` | Create a new reference table |
| POST | `/api/reference-tables/sync` | Sync reference table with data |
| PUT | `/api/reference-tables/{tableName}/rows` | Add or update a row |
| DELETE | `/api/reference-tables/{tableName}` | Delete a reference table |

## 🎨 Custom Attributes

### MapTo
Maps a source property to a target property with a different name.

```csharp
[MapTo("TargetPropertyName", Optional = false)]
public string SourceProperty { get; set; }
```

### MappingProfile
Defines mapping behavior at the class level.

```csharp
[MappingProfile("ProfileName", IgnoreUnmapped = true)]
public class MyModel { }
```

### IgnoreMapping
Excludes a property from mapping.

```csharp
[IgnoreMapping]
public string InternalField { get; set; }
```

## 🔌 Extension Points

### Custom Converters

Implement `IPropertyConverter` for custom type conversions:

```csharp
public class CustomConverter : IPropertyConverter
{
    public object? Convert(object? sourceValue, Type targetType)
    {
        // Your conversion logic
    }

    public bool CanConvert(Type sourceType, Type targetType)
    {
        // Return true if conversion is supported
    }
}
```

Use it in attributes:

```csharp
[MapTo("Target", ConverterType = typeof(CustomConverter))]
public string Source { get; set; }
```

## 📊 Use Cases

### Attribute Mapping
- **Legacy System Modernization**: Map data from old systems to modern formats
- **API Integration**: Transform data between different API schemas
- **Data Migration**: Convert data during system migrations
- **ETL Processes**: Transform data in Extract-Transform-Load pipelines
- **Multi-tenant Applications**: Map data structures across different tenants

### Reference Tables
- **Master Data Management**: Centrally maintain system-independent master data
- **Data Classification**: Group and classify cost types, diagnoses, or product categories
- **Label Harmonization**: Standardize labels from different data sources
- **Product Hierarchies**: Create product type classifications and groupings
- **Code Mapping**: Map external codes to internal classifications

## 🤝 Contributing

This is a competition entry, but suggestions and feedback are welcome!

## 📄 License

This project is created for the Microsoft Fabric Extensibility Toolkit Contest.

## 👤 Author

Philipp Frenzel ([@philippfrenzel](https://github.com/philippfrenzel))

## 🙏 Acknowledgments

- Microsoft Fabric Team for the Extensibility Toolkit
- Microsoft for hosting the competition
- The .NET community for excellent tools and libraries

## 📧 Contact

For questions or feedback about this project, please open an issue on GitHub.

---

**Built with ❤️ for the Microsoft Fabric Extensibility Toolkit Contest**

