# Quick Reference: Reference Table (Lookup) Tool

## What is it?

The **Reference Table Tool** (also called **Lookup Table** or **KeyMapping**) is a data classification and harmonization system for Microsoft Fabric. It helps you create and manage master data tables that standardize data values across different systems.

## Key Concept

```
Inconsistent Data → Reference Table → Standardized Classification

Example:
VTP001 in System A: "Health Insurance"
VTP001 in System B: "Krankenversicherung"  
VTP001 in System C: "Medical Coverage"

↓ Apply Reference Table ↓

VTP001 → Insurance / Health (everywhere)
```

## When to Use

✅ **Product Classification** - Group products into standardized categories  
✅ **Cost Center Mapping** - Standardize cost center codes across systems  
✅ **Medical Codes** - Classify diagnosis or procedure codes  
✅ **Customer Segmentation** - Create customer classification schemes  
✅ **Label Harmonization** - Unify labels from different data sources  
✅ **Master Data Management** - Maintain centralized system-independent master data

## How It Works

### Method 1: Manual Creation

1. **Create empty table** with columns you need
2. **Add rows manually** via UI or API
3. **Use for lookups** in your analytics

### Method 2: Automated Sync

1. **Sync from source data** (e.g., product database)
2. **System creates keys** automatically
3. **Classify manually** - add category information
4. **Use for lookups** in your analytics

## User Interface

### Basic Mode (Table View)
- Point-and-click table editing
- Add, edit, delete rows inline
- Perfect for non-technical users

### Expert Mode (JSON Editor)
- Direct JSON editing with syntax highlighting
- Batch operations
- For power users and automation

## API Operations

```bash
# List tables
GET /api/reference-tables

# Get table data
GET /api/reference-tables/{tableName}

# Create table
POST /api/reference-tables

# Sync with source
POST /api/reference-tables/sync

# Update row
PUT /api/reference-tables/{tableName}/rows

# Delete table
DELETE /api/reference-tables/{tableName}
```

## Microsoft Fabric Integration

Reference tables are exposed as **KeyMapping outports** that can be consumed by:
- Power BI reports
- Data warehouses
- Lakehouse analytics
- Other data products in your workspace

## Quick Example

**Create a product classification table:**

```json
POST /api/reference-tables
{
  "tableName": "producttype",
  "columns": [
    { "name": "ProductType", "dataType": "string" },
    { "name": "TargetGroup", "dataType": "string" }
  ]
}
```

**Add product classifications:**

```json
PUT /api/reference-tables/producttype/rows
{
  "key": "VTP001",
  "attributes": {
    "ProductType": "Insurance",
    "TargetGroup": "Health"
  }
}
```

**Use in analytics:**

```sql
SELECT 
  sales.*, 
  ref.ProductType, 
  ref.TargetGroup
FROM sales_data sales
LEFT JOIN producttype ref ON sales.product_id = ref.key
```

## Benefits

✅ **Consistency** - Same classification everywhere  
✅ **Comparability** - Analyze data across systems  
✅ **Automation** - Sync automatically with sources  
✅ **Single Source of Truth** - One place for master data  
✅ **Easy Maintenance** - Simple UI for updates  
✅ **Fabric Native** - KeyMapping outport support

## Learn More

📖 [Full Documentation](README.md)  
🎨 [Visual Guide with UI Mockups](docs/UI_MOCKUPS.md)  
🔧 [Workload Deployment Guide](docs/WORKLOAD_GUIDE.md)  
📊 [API Reference](docs/API.md)

---

**Built for the Microsoft Fabric Extensibility Toolkit Contest**
