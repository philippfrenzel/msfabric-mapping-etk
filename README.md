# Fabric Data Attribute Mapping Service

A comprehensive data attribute mapping service for Microsoft Fabric Extensibility Toolkit that enables seamless transformation of data between different object structures using attribute-based configuration.

## 🎯 Overview

This project implements a powerful and flexible data attribute mapping service in C# that integrates with Microsoft Fabric using the Extensibility Toolkit. It allows you to map data between different object structures using custom attributes, making it ideal for data integration, ETL processes, and legacy system modernization.

## 🏆 Competition Entry

This project is created for the [Microsoft Fabric Extensibility Toolkit Contest](https://blog.fabric.microsoft.com/en-us/blog/announcing-the-fabric-extensibility-toolkit-contest) to demonstrate the capabilities of building custom workloads for Microsoft Fabric.

## ✨ Features

- **Attribute-Based Mapping**: Use custom attributes to define mappings between source and target properties
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

- .NET 10.0 SDK or later
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

## 🧪 Testing

The project includes comprehensive unit tests:

```bash
dotnet test
```

Test coverage includes:
- Attribute mapping functionality
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

- **Legacy System Modernization**: Map data from old systems to modern formats
- **API Integration**: Transform data between different API schemas
- **Data Migration**: Convert data during system migrations
- **ETL Processes**: Transform data in Extract-Transform-Load pipelines
- **Multi-tenant Applications**: Map data structures across different tenants

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

