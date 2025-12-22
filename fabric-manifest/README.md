# Fabric Manifest

This directory contains the Microsoft Fabric workload manifest and item type definitions for the Reference Table & Data Mapping Service.

## Structure

```
fabric-manifest/
├── workload-manifest.json          # Main workload manifest
├── Product.json                    # Frontend metadata and UI configuration
├── items/                          # Item type definitions
│   ├── ReferenceTableItem/        # Reference Table item type
│   │   ├── ReferenceTableItem.xml          # Item manifest configuration
│   │   └── ReferenceTableItem.json         # Item UI/routing configuration
│   ├── MappingConfigurationItem/  # Mapping Configuration item type
│   │   ├── MappingConfigurationItem.xml    # Item manifest configuration
│   │   └── MappingConfigurationItem.json   # Item UI/routing configuration
│   └── MappingJobItem/            # Mapping Job item type
│       ├── MappingJobItem.xml              # Item manifest configuration
│       └── MappingJobItem.json             # Item UI/routing configuration
├── assets/                         # Visual assets
│   └── images/                    # Icons and images
│       └── README.md              # Asset documentation
└── translations/                   # Localization files
    └── en-US.json                 # English translations
```

## Workload Manifest

The `workload-manifest.json` file defines the workload's metadata, authentication, backend endpoints, item types, and permissions. This is the main configuration file that Microsoft Fabric uses to understand and integrate the workload.

### Key Sections

- **workloadDetails**: Basic information about the workload (ID, name, version, category)
- **authentication**: AAD/Entra ID authentication configuration
- **backend**: Backend API endpoints and their specifications
- **frontend**: Frontend entry points and routes
- **itemTypes**: Definitions of custom item types (ReferenceTable, MappingConfiguration, MappingJob)
- **permissions**: Required Fabric workspace permissions
- **dataAccess**: OneLake integration settings

## Product Configuration

The `Product.json` file defines the frontend experience:

- **homePage**: Learning materials and recommended item types
- **createExperience**: Cards shown in the create dialog for each item type

## Item Type Definitions

Each item type has two files:

1. **[ItemName].xml**: Item manifest configuration that defines the item's category and workload association
2. **[ItemName].json**: UI configuration including editor paths, icons, context menu items, and quick actions

### Reference Table Item

- **Purpose**: Create and manage reference tables for data classification (KeyMapping outports)
- **Category**: Data
- **Capabilities**: Create, Read, Update, Delete, Share, Export
- **Context Menu**: Sync, Export as KeyMapping
- **Quick Actions**: View Data

### Mapping Configuration Item

- **Purpose**: Define attribute-based mappings between source and target types
- **Category**: Configuration
- **Capabilities**: Create, Read, Update, Delete, Share
- **Context Menu**: Test Mapping
- **Quick Actions**: Edit Configuration

### Mapping Job Item

- **Purpose**: Execute data mapping operations
- **Category**: Jobs
- **Capabilities**: Create, Read, Delete, Execute
- **Context Menu**: Run Job, Cancel Job
- **Quick Actions**: View Status

## Assets

Visual assets (icons, images) are stored in the `assets/images/` directory. See the [Assets README](assets/images/README.md) for specifications.

## Translations

Localization files are stored in the `translations/` directory. Currently includes:

- `en-US.json`: English translations for all display names and descriptions

## Integration with Fabric

This manifest structure follows the Microsoft Fabric Extensibility Toolkit patterns:

1. **Workload Registration**: The workload manifest is used to register the workload with a Fabric tenant
2. **Item Types**: Each item type becomes available in Fabric workspaces for users to create
3. **Backend Integration**: API endpoints are exposed through the Fabric platform
4. **UI Integration**: Frontend experiences are embedded in the Fabric UI

## Development Notes

- Update `YOUR_AAD_APP_ID_HERE` in `workload-manifest.json` with your Entra App ID
- Update `YOUR_BACKEND_URL_HERE` with your actual backend API URL
- Update `YOUR_FRONTEND_URL_HERE` with your actual frontend URL
- Create actual icon files in `assets/images/` directory
- Add additional translations as needed for internationalization

## References

- [Microsoft Fabric Extensibility Toolkit](https://learn.microsoft.com/fabric/extensibility-toolkit/)
- [Workload Manifest Schema](https://developer.microsoft.com/json-schemas/fabric/workload/v1.0/workloadManifest.json)
- [Item Type Development Guide](https://learn.microsoft.com/fabric/extensibility-toolkit/build-your-workload)
