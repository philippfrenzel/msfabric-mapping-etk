# API Documentation

## Overview

The Fabric Mapping Service API provides RESTful endpoints for performing data attribute mapping operations. All endpoints return JSON responses and support standard HTTP status codes.

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

### Using cURL

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

1. **Validate Input**: Always validate data before sending to the API
2. **Handle Errors**: Check the `success` field in responses
3. **Use Batch Operations**: For multiple items, use batch endpoints for better performance
4. **Monitor Health**: Regularly call the health endpoint to monitor service status
5. **Log Operations**: Keep track of mapping operations for auditing
