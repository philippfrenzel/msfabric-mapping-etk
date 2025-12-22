# Fabric Reference Table & Data Mapping Service

A comprehensive reference table (lookup table) and data mapping service for Microsoft Fabric Extensibility Toolkit that enables data classification, harmonization, and transformation using reference tables and attribute-based configuration.

## 🎯 Overview

This project implements a powerful and flexible reference table service in C# that integrates with Microsoft Fabric using the Extensibility Toolkit. It provides **reference tables (Referenztabellen)** for data classification and harmonization, plus attribute-based data mapping capabilities. Reference tables act as lookup tables (KeyMapping outports) that help structure data consistently and make it comparable across different sources, making it ideal for master data management, data classification, ETL processes, and legacy system modernization.

## 🏆 Competition Entry

This project is created for the [Microsoft Fabric Extensibility Toolkit Contest](https://blog.fabric.microsoft.com/en-us/blog/announcing-the-fabric-extensibility-toolkit-contest) to demonstrate the capabilities of building custom workloads for Microsoft Fabric.

## ✨ Features

### Primary: Reference Tables (KeyMapping)
- **Reference Tables (Lookup Tables)**: Create and manage reference tables for data classification and harmonization
- **KeyMapping Outports**: Provide reference tables as KeyMapping outports for Fabric data products
- **Manual Master Data**: Centrally maintained system-independent master data
- **Automated Sync**: Automatically sync reference tables from source data (outports)
- **Data Classification**: Group and classify cost types, diagnoses, product categories, etc.
- **Label Harmonization**: Standardize labels and codes from different data sources
- **OneLake Integration**: Store and consume reference tables via OneLake

### Additional: Attribute-Based Mapping
- **Attribute-Based Mapping**: Use custom attributes to define mappings between source and target properties
- **Type Conversion**: Automatic conversion between compatible types (string to int, bool, decimal, etc.)
- **Flexible Configuration**: Configure mapping behavior at both class and property levels
- **Batch Operations**: Map collections of objects efficiently

### Technical Features
- **Error Handling**: Detailed error reporting and validation
- **REST API**: Full-featured ASP.NET Core Web API for reference table and mapping operations
- **Extensible**: Support for custom converters and mapping profiles
- **Microsoft Fabric Integration**: Native integration with Fabric workspaces via Extensibility Toolkit

## 🏗️ Architecture

```
FabricMappingService/
├── .ai/                                 # AI assistant context and commands
│   ├── context/                         # Context documentation for AI tools
│   │   ├── fabric-workload.md          # Extensibility Toolkit knowledge
│   │   ├── fabric.md                   # Microsoft Fabric platform context
│   │   └── mapping-service.md          # Custom mapping service context
│   └── commands/                        # Command templates
│       ├── workload/                    # Workload operations
│       └── item/                        # Item operations
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
│   ├── workload-manifest.json          # Main workload manifest
│   ├── Product.json                    # Frontend metadata and UI configuration
│   ├── items/                          # Item type definitions
│   │   ├── ReferenceTableItem/        # Reference Table item type
│   │   ├── MappingConfigurationItem/  # Mapping Configuration item type
│   │   └── MappingJobItem/            # Mapping Job item type
│   ├── assets/                         # Visual assets
│   │   └── images/                    # Icons and images
│   └── translations/                   # Localization files
├── scripts/                            # Automation scripts
└── docs/                               # Documentation

```

## 🚀 Quick Start

### Prerequisites

- .NET 10.0 SDK or later (for local development)
  - For Azure deployment, retarget to .NET 8.0 or 9.0 in `.csproj` files if needed
- PowerShell 7 (for automation scripts)
- Visual Studio 2022, VS Code, or GitHub Codespaces
- Microsoft Fabric workspace (for integration)

### Automated Setup

Use the provided setup script for automated environment configuration:

```powershell
# Windows PowerShell
.\scripts\Setup\Setup.ps1

# macOS/Linux
pwsh ./scripts/Setup/Setup.ps1
```

### Manual Setup

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

### Using Development Scripts

```powershell
# Start development server with hot reload
.\scripts\Run\StartDevServer.ps1

# Build and test
.\scripts\Build\Build.ps1

# Publish for deployment
.\scripts\Build\Publish.ps1
```

The API will be available at `https://localhost:5001` (or the port specified in launchSettings.json).

### GitHub Codespaces

This project includes a complete dev container configuration. Click "Code" → "Open with Codespaces" to get started instantly with a pre-configured development environment.

## 📖 Usage Examples

### Reference Tables (Primary Use Case)

Reference tables (Referenztabellen) provide a powerful way to classify, group, and harmonize data values. They act as lookup tables (KeyMapping outports in Fabric) that help structure data consistently and make it comparable across different sources.

#### What is a Reference Table?

A reference table is essentially a list that defines how certain values are grouped, renamed, or standardized. It works like a lookup table that helps with data analysis by providing clear structure and comparability.

**Primary Use Cases:**
- **Manual Master Data**: Centrally maintained system-independent master data
- **Cost Type Mapping**: Classifying and mapping cost categories
- **Diagnosis Classification**: Standardizing medical or technical diagnoses
- **Label Harmonization**: Unifying labels from different sources
- **Product Grouping**: Creating product type hierarchies
- **Code Mapping**: Mapping external codes to internal classifications

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
- Use outport type **"KeyMapping"** for reference table data
- The key element is automatically designated as "key"
- Reference tables can be consumed as outports by other data products

**Example workflow:**
1. Create reference table via API or sync from source data
2. Read the mapping: `var mapping = mappingIO.ReadMapping("tableName")`
3. Provide as KeyMapping outport for consumption by analytics

```csharp
// Example: Providing reference table as KeyMapping outport
var mapping = mappingIO.ReadMapping("produkttyp");

// Convert to DataFrame/output format
// Write to KeyMapping outport for consumption
outportWriter.Write(mapping, "Produkttyp_Mapping", outportType: "KeyMapping");
```

### Basic Attribute Mapping (Additional Feature)

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

The project includes a complete workload manifest for Fabric integration following the Microsoft Fabric Extensibility Toolkit patterns. The manifest structure is organized as follows:

**`fabric-manifest/`** directory contains:
- **`workload-manifest.json`**: Main workload manifest with authentication, backend endpoints, and item type definitions
- **`Product.json`**: Frontend metadata and UI configuration for the Fabric experience
- **`items/`**: Item type definitions for ReferenceTable, MappingConfiguration, and MappingJob
- **`assets/`**: Visual assets including icons and images
- **`translations/`**: Localization files for internationalization

See [fabric-manifest/README.md](fabric-manifest/README.md) for detailed documentation.

### AI Assistant Integration

The **`.ai/`** directory provides context documentation for AI tools and agents:
- **`context/`**: Knowledge base about Fabric platform, Extensibility Toolkit, and this service
  - `fabric-workload.md`: Extensibility Toolkit development patterns
  - `fabric.md`: Microsoft Fabric platform architecture and APIs
  - `mapping-service.md`: Custom mapping service context and usage
- **`commands/`**: Command templates for common workload and item operations

This structure enables AI assistants (like GitHub Copilot) to better understand the project context and provide more accurate code suggestions and assistance.

### Setup for Fabric

1. **Register your application** in Microsoft Entra ID
2. **Update the manifest** with your AAD App ID and backend URL in `fabric-manifest/workload-manifest.json`
3. **Deploy the API** to Azure App Service or your hosting platform
4. **Register the workload** in your Fabric tenant
5. **Configure permissions** for workspace access

### Item Types

The service defines three item types for Fabric:

- **ReferenceTable**: Reference tables for data classification (KeyMapping outports) - provides lookup tables for data harmonization
- **MappingConfiguration**: Store and manage attribute-based mapping configurations
- **MappingJob**: Execute and monitor mapping operations

## 📚 API Endpoints

### Reference Table Endpoints (Primary)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/reference-tables` | List all reference tables |
| GET | `/api/reference-tables/{tableName}` | Get reference table data |
| POST | `/api/reference-tables` | Create a new reference table |
| POST | `/api/reference-tables/sync` | Sync reference table with data |
| PUT | `/api/reference-tables/{tableName}/rows` | Add or update a row |
| DELETE | `/api/reference-tables/{tableName}` | Delete a reference table |

### Attribute Mapping Endpoints (Additional)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/` | Service information |
| GET | `/api/mapping/info` | Available mappings |
| GET | `/api/mapping/health` | Health check |
| POST | `/api/mapping/customer/legacy-to-modern` | Map legacy customer |
| POST | `/api/mapping/product/external-to-internal` | Map external product |
| POST | `/api/mapping/customer/batch-legacy-to-modern` | Batch map customers |

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

### Reference Tables (Primary Use Cases)
- **Master Data Management**: Centrally maintain system-independent master data accessible as KeyMapping outports
- **Data Classification**: Group and classify cost types, diagnoses, or product categories with structured hierarchies
- **Label Harmonization**: Standardize labels and codes from different data sources for consistent analytics
- **Product Hierarchies**: Create product type classifications and groupings for organized reporting
- **Code Mapping**: Map external codes to internal classifications for seamless data integration
- **Cost Center Mapping**: Classify and standardize cost center codes across different systems
- **Medical Code Classification**: Standardize ICD codes, diagnosis codes, or procedure codes
- **Customer Segmentation**: Create and maintain customer classification schemes

### Attribute Mapping (Additional Use Cases)
- **Legacy System Modernization**: Map data from old systems to modern formats
- **API Integration**: Transform data between different API schemas
- **Data Migration**: Convert data during system migrations
- **ETL Processes**: Transform data in Extract-Transform-Load pipelines
- **Multi-tenant Applications**: Map data structures across different tenants

## 📚 Documentation

- **[Project Setup Guide](docs/PROJECT_SETUP.md)**: Complete environment setup instructions
- **[Project Structure](docs/PROJECT_STRUCTURE.md)**: Repository organization and conventions
- **[API Documentation](docs/API.md)**: Detailed API endpoint reference
- **[Fabric Integration](docs/FABRIC-INTEGRATION.md)**: Microsoft Fabric integration guide
- **[Scripts Documentation](scripts/README.md)**: Automation scripts reference
- **[Security Policy](SECURITY.md)**: Security reporting guidelines
- **[Support](SUPPORT.md)**: Getting help and support resources
- **[Code of Conduct](CODE_OF_CONDUCT.md)**: Community guidelines

## 🤝 Contributing

This is a competition entry, but suggestions and feedback are welcome! Please read our [Code of Conduct](CODE_OF_CONDUCT.md) before contributing.

For support, see [SUPPORT.md](SUPPORT.md).

## 📄 License

This project is created for the Microsoft Fabric Extensibility Toolkit Contest.

## 👤 Author

Philipp Frenzel ([@philippfrenzel](https://github.com/philippfrenzel))

## 🙏 Acknowledgments

- Microsoft Fabric Team for the Extensibility Toolkit
- Microsoft for hosting the competition
- The .NET community for excellent tools and libraries
- Based on design principles from [Microsoft Fabric Tools Workload](https://github.com/microsoft/Microsoft-Fabric-tools-workload)

## 📧 Contact

For questions or feedback about this project, please open an issue on GitHub or see [SUPPORT.md](SUPPORT.md).

---

**Built with ❤️ for the Microsoft Fabric Extensibility Toolkit Contest**

This project follows the design principles and best practices established by the [Microsoft Fabric Extensibility Toolkit](https://github.com/microsoft/fabric-extensibility-toolkit).

