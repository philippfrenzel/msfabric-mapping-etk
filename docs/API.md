# API Documentation

## Overview

The Fabric Reference Table & Data Mapping Service API provides RESTful endpoints for managing reference tables (KeyMapping outports) and performing data attribute mapping operations. The primary focus is on reference tables for data classification and harmonization. All endpoints return JSON responses and support standard HTTP status codes.

## Base URL

```
Development: https://localhost:5001
Production: YOUR_PRODUCTION_URL
```

## Authentication

For Fabric integration, the API uses Microsoft Entra ID (Azure AD) authentication. Include the bearer token in the Authorization header:

```
Authorization: Bearer YOUR_ACCESS_TOKEN
```

## Endpoints

## Reference Table Endpoints (Primary Feature)

Reference tables provide KeyMapping outports for data classification and harmonization in Microsoft Fabric.

### GET /api/reference-tables

List all reference tables.

**Response:**
```json
{
  "success": true,
  "tables": [
    "vertragsprodukte",
    "produkttyp",
    "cost_types"
  ],
  "count": 3
}
```

### GET /api/reference-tables/{tableName}

Get reference table data by name. Returns all rows with their key-value pairs.

**Parameters:**
- `tableName` (path): The name of the reference table

**Response:**
```json
{
  "success": true,
  "tableName": "produkttyp",
  "data": {
    "VTP001": {
      "key": "VTP001",
      "Category": "Insurance",
      "Group": "Health"
    },
    "VTP002": {
      "key": "VTP002",
      "Category": "Insurance",
      "Group": "Life"
    }
  }
}
```

**Status Codes:**
- `200 OK`: Reference table retrieved successfully
- `404 Not Found`: Reference table does not exist
- `500 Internal Server Error`: Server error

### POST /api/reference-tables

Create a new empty reference table with custom columns.

**Request Body:**
```json
{
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
}
```

**Response:**
```json
{
  "success": true,
  "tableName": "vertragsprodukte"
}
```

**Status Codes:**
- `201 Created`: Reference table created successfully
- `400 Bad Request`: Invalid request data or table already exists
- `500 Internal Server Error`: Server error

### POST /api/reference-tables/sync

Sync a reference table with data from a source. Creates the table if it doesn't exist, or adds only NEW keys to existing table. This is the recommended approach for automated reference table creation from outport data.

**Request Body:**
```json
{
  "mappingTableName": "produkttyp",
  "keyAttributeName": "Produkt",
  "data": [
    {
      "Produkt": "VTP001",
      "Name": "Product A",
      "Price": 100
    },
    {
      "Produkt": "VTP002",
      "Name": "Product B",
      "Price": 200
    }
  ]
}
```

**Important Notes:**
- The `keyAttributeName` property value becomes the key for each row
- Only NEW keys are added (duplicates are ignored)
- Existing keys are NOT updated
- Removed values must be deleted manually
- The key column is automatically named "key"

**Response:**
```json
{
  "success": true,
  "tableName": "produkttyp",
  "newKeysAdded": 2,
  "totalKeys": 2
}
```

**Status Codes:**
- `200 OK`: Sync completed successfully
- `400 Bad Request`: Invalid request data or missing key attribute
- `500 Internal Server Error`: Server error

### PUT /api/reference-tables/{tableName}/rows

Add or update a row in an existing reference table. This is used to add classification attributes to keys.

**Parameters:**
- `tableName` (path): The name of the reference table

**Request Body:**
```json
{
  "key": "VTP001",
  "attributes": {
    "Category": "Insurance",
    "Group": "Health",
    "SubGroup": "Basic Coverage"
  }
}
```

**Response:**
```json
{
  "success": true,
  "tableName": "produkttyp",
  "key": "VTP001",
  "message": "Row updated successfully"
}
```

**Status Codes:**
- `200 OK`: Row updated successfully
- `400 Bad Request`: Invalid request data
- `404 Not Found`: Reference table does not exist
- `500 Internal Server Error`: Server error

### DELETE /api/reference-tables/{tableName}

Delete a reference table.

**Parameters:**
- `tableName` (path): The name of the reference table

**Response:**
```json
{
  "success": true,
  "message": "Reference table 'produkttyp' deleted"
}
```

**Status Codes:**
- `200 OK`: Reference table deleted successfully
- `404 Not Found`: Reference table does not exist
- `500 Internal Server Error`: Server error

## Attribute Mapping Endpoints (Additional Feature)

### GET / (Root)

Returns service information and available endpoints.

**Response:**
```json
{
  "service": "Fabric Mapping Service",
  "version": "1.0.0",
  "description": "Data Attribute Mapping Service for Microsoft Fabric Extensibility Toolkit",
  "endpoints": [...],
  "documentation": "/openapi/v1.json"
}
```

### GET /api/mapping/info

Get detailed information about the mapping service and available mappings.

**Response:**
```json
{
  "serviceName": "Fabric Mapping Service",
  "version": "1.0.0",
  "description": "Attribute-based data mapping service...",
  "availableMappings": [...],
  "supportedFeatures": [...]
}
```

### GET /api/mapping/health

Health check endpoint to verify service availability.

**Response:**
```json
{
  "status": "Healthy",
  "timestamp": "2024-01-01T12:00:00Z",
  "service": "FabricMappingService"
}
```

### POST /api/mapping/customer/legacy-to-modern

Map legacy customer data to modern format.

**Request Body:**
```json
{
  "id": 123,
  "customerName": "John Doe",
  "email": "john@example.com",
  "phone": "+1234567890",
  "createdDate": "2024-01-01T00:00:00Z",
  "status": true,
  "country": "USA"
}
```

**Response:**
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
  "mappedPropertiesCount": 7,
  "metadata": {
    "sourceType": "LegacyCustomerModel",
    "targetType": "ModernCustomerModel"
  }
}
```

**Status Codes:**
- `200 OK`: Mapping successful
- `400 Bad Request`: Invalid request data
- `500 Internal Server Error`: Server error

### POST /api/mapping/product/external-to-internal

Map external product data to internal format with type conversion.

**Request Body:**
```json
{
  "productCode": "PROD-123",
  "productTitle": "Sample Product",
  "productDescription": "A great product",
  "priceString": "99.99",
  "stockLevel": "50",
  "category": "Electronics",
  "inStock": "true"
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "productId": "PROD-123",
    "name": "Sample Product",
    "description": "A great product",
    "price": 99.99,
    "quantity": 50,
    "categoryName": "Electronics",
    "isAvailable": true
  },
  "errors": [],
  "warnings": [],
  "mappedPropertiesCount": 7,
  "metadata": {
    "sourceType": "ExternalProductModel",
    "targetType": "InternalProductModel"
  }
}
```

### POST /api/mapping/customer/batch-legacy-to-modern

Batch map multiple legacy customers to modern format.

**Request Body:**
```json
[
  {
    "id": 1,
    "customerName": "John Doe",
    "email": "john@example.com",
    "phone": "+1234567890",
    "createdDate": "2024-01-01T00:00:00Z",
    "status": true,
    "country": "USA"
  },
  {
    "id": 2,
    "customerName": "Jane Smith",
    "email": "jane@example.com",
    "phone": "+0987654321",
    "createdDate": "2024-01-02T00:00:00Z",
    "status": true,
    "country": "Canada"
  }
]
```

**Response:**
```json
{
  "success": true,
  "totalItems": 2,
  "successCount": 2,
  "failureCount": 0,
  "results": [
    {
      "success": true,
      "data": { ... },
      "errors": [],
      "warnings": []
    },
    {
      "success": true,
      "data": { ... },
      "errors": [],
      "warnings": []
    }
  ]
}
```

## Error Handling

All endpoints return consistent error responses:

```json
{
  "success": false,
  "data": null,
  "errors": [
    "Error message 1",
    "Error message 2"
  ],
  "warnings": [],
  "mappedPropertiesCount": null
}
```

## Rate Limiting

Currently no rate limiting is implemented. For production deployment, consider implementing rate limiting based on your requirements.

## OpenAPI/Swagger

The API includes OpenAPI documentation available at:
- Development: `https://localhost:5001/openapi/v1.json`

## Examples

### Reference Table Workflow (Primary Use Case)

#### Complete Workflow: From Source Data to KeyMapping Outport

```bash
# Step 1: Sync reference table from source data
curl -X POST https://localhost:5001/api/reference-tables/sync \
  -H "Content-Type: application/json" \
  -d '{
    "mappingTableName": "produkttyp",
    "keyAttributeName": "Produkt",
    "data": [
      { "Produkt": "VTP001", "Name": "Product A", "Price": 100 },
      { "Produkt": "VTP002", "Name": "Product B", "Price": 200 },
      { "Produkt": "VTP003", "Name": "Product C", "Price": 300 }
    ]
  }'

# Step 2: Add classification attributes to each key
curl -X PUT https://localhost:5001/api/reference-tables/produkttyp/rows \
  -H "Content-Type: application/json" \
  -d '{
    "key": "VTP001",
    "attributes": {
      "ProductType": "Basic",
      "TargetGroup": "Individual"
    }
  }'

curl -X PUT https://localhost:5001/api/reference-tables/produkttyp/rows \
  -H "Content-Type: application/json" \
  -d '{
    "key": "VTP002",
    "attributes": {
      "ProductType": "Premium",
      "TargetGroup": "Individual"
    }
  }'

# Step 3: Read the complete reference table (KeyMapping outport)
curl https://localhost:5001/api/reference-tables/produkttyp

# Result can now be used as KeyMapping outport in Fabric
```

#### Manual Reference Table Creation

```bash
# Create empty reference table
curl -X POST https://localhost:5001/api/reference-tables \
  -H "Content-Type: application/json" \
  -d '{
    "tableName": "cost_types",
    "columns": [
      { "name": "MainCategory", "dataType": "string", "order": 1 },
      { "name": "SubCategory", "dataType": "string", "order": 2 }
    ],
    "isVisible": true,
    "notifyOnNewMapping": false
  }'

# Add rows manually
curl -X PUT https://localhost:5001/api/reference-tables/cost_types/rows \
  -H "Content-Type: application/json" \
  -d '{
    "key": "COST001",
    "attributes": {
      "MainCategory": "Medical",
      "SubCategory": "Consultation"
    }
  }'

# Read the reference table
curl https://localhost:5001/api/reference-tables/cost_types
```

#### List All Reference Tables

```bash
curl https://localhost:5001/api/reference-tables
```

### Attribute Mapping Examples (Additional Feature)

#### Using cURL

```bash
# Get service info
curl https://localhost:5001/api/mapping/info

# Map a customer
curl -X POST https://localhost:5001/api/mapping/customer/legacy-to-modern \
  -H "Content-Type: application/json" \
  -d '{"id":123,"customerName":"John Doe",...}'
```

### Using PowerShell

```powershell
# Get service info
Invoke-RestMethod -Uri "https://localhost:5001/api/mapping/info"

# Map a customer
$body = @{
    id = 123
    customerName = "John Doe"
    email = "john@example.com"
} | ConvertTo-Json

Invoke-RestMethod -Uri "https://localhost:5001/api/mapping/customer/legacy-to-modern" `
  -Method Post -Body $body -ContentType "application/json"
```

### Using C#

```csharp
using System.Net.Http;
using System.Net.Http.Json;

var client = new HttpClient { BaseAddress = new Uri("https://localhost:5001") };

// Get service info
var info = await client.GetFromJsonAsync<object>("/api/mapping/info");

// Map a customer
var legacy = new LegacyCustomerModel { Id = 123, CustomerName = "John Doe" };
var response = await client.PostAsJsonAsync("/api/mapping/customer/legacy-to-modern", legacy);
var result = await response.Content.ReadFromJsonAsync<MappingResponse>();
```

## Best Practices

### Reference Tables (Primary Use Case)

1. **Use Sync for Automated Creation**: When reference tables come from source data, use the `/sync` endpoint to automatically create and maintain keys
2. **Add Classification Separately**: After syncing keys, use PUT `/rows` to add classification attributes
3. **KeyMapping Outport Pattern**: Reference tables automatically provide KeyMapping outports for Fabric consumption
4. **Consistent Naming**: Use descriptive table names that indicate the data classification purpose (e.g., "produkttyp", "cost_types")
5. **Key Stability**: Don't modify the "key" column - it's automatically managed
6. **Manual Deletion Only**: Removed values are not automatically deleted; clean up manually when needed
7. **Incremental Updates**: Use sync repeatedly - only new keys are added, existing data is preserved

### Integration with Fabric

1. **KeyMapping Outport**: Reference tables are exposed as KeyMapping outports
2. **OneLake Storage**: Store reference table configurations in OneLake for persistence
3. **Cross-Product Consumption**: Other data products can consume your reference tables as lookup sources
4. **Versioning**: Track changes to reference table structures for audit purposes

### Attribute Mapping (Additional Feature)

1. **Validate Input**: Always validate data before sending to the API
2. **Handle Errors**: Check the `success` field in responses
3. **Use Batch Operations**: For multiple items, use batch endpoints for better performance
4. **Monitor Health**: Regularly call the health endpoint to monitor service status
5. **Log Operations**: Keep track of mapping operations for auditing
